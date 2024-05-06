param project_name string = 'hunter'
param environment string = 'demo6'

/*
param aks_tier string = 'Free'
param keyvault_sku string = 'Standard'
/*/
param aks_tier string = 'Standard'
param keyvault_sku string = 'Premium'
//*/

param aks_vmsize string = (aks_tier == 'Free') ? 'Standard_B2pls_v2' : 'Standard_DC4as_cc_v5'
param aks_availability_zones array = ['1', '2', '3']
param aks_system_node_count int = 2
param aks_user_node_count int = 2

param location string = resourceGroup().location
param identity_name string = toLower('${project_name}-${environment}-identity')
param identity_federated_credentials_name string = toLower('${project_name}-${environment}-service-account-credentials')
param aks_name string = toLower('${project_name}-${environment}-aks')
param aks_dns_prefix string = toLower('${project_name}-${environment}-dns-prefix')
param aks_node_resource_group string = toLower('${project_name}-${environment}-aks-infra-rg')
param aks_service_account_name string = toLower('${project_name}-sa')
param keyvault_name string = toLower('${project_name}-${environment}-keyvault')
param attestation_provider_name string = toLower('${project_name}${environment}Attest')

resource keyvault_resource 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyvault_name
  location: location
  properties: {
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: false
    sku: {
      family: 'A'
      name: keyvault_sku
    }
  }
}

resource identity_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identity_name
  location: location
}

resource identity_federated_credentials 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: identity_resource
  name: identity_federated_credentials_name
  properties: {
    issuer: aks_resource.properties.oidcIssuerProfile.issuerURL
    subject: 'system:serviceaccount:default:${aks_service_account_name}'
    audiences: ['api://AzureADTokenExchange']
  }
}

var KeyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'
resource keyvault_secret_user_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyvault_resource.id, 'Key Vault Secrets User', identity_resource.id)
  scope: keyvault_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', KeyVaultSecretsUserRoleId)
    principalId: identity_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

var KeyVaultCryptoOfficerRoleId = '14b46e9e-c2b7-41b4-b07b-48a6ebf60603'
resource keyvault_crypto_officer_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyvault_resource.id, 'Key Vault Crypto Officer', identity_resource.id)
  scope: keyvault_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', KeyVaultCryptoOfficerRoleId)
    principalId: identity_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource attestation_provider_resource 'Microsoft.Attestation/attestationProviders@2021-06-01' = {
  name: attestation_provider_name
  location: location
  properties: {}
}

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-01-01' = {
  location: location
  name: aks_name
  properties: {
    kubernetesVersion: '1.29.2'
    enableRBAC: true
    dnsPrefix: aks_dns_prefix
    nodeResourceGroup: aks_node_resource_group
    agentPoolProfiles: (aks_tier == 'Free') ? [
      {
        name: 'nodepool'
        mode: 'System'
        vmSize: aks_vmsize
        osType: 'Linux'
        osSKU: 'AzureLinux'
        enableAutoScaling: false
        count: 1
      }
    ] : [
      {
        name: 'agentpool'
        mode: 'System'
        vmSize: aks_vmsize
        osType: 'Linux'
        osSKU: 'AzureLinux'
        enableAutoScaling: true
        count: aks_system_node_count
        minCount: aks_system_node_count
        maxCount: 10
        availabilityZones: aks_availability_zones
        nodeTaints: ['CriticalAddonsOnly=true:NoSchedule']
      }
      {
        name: 'userpool'
        mode: 'User'
        vmSize: aks_vmsize
        osType: 'Linux'
        osSKU: 'AzureLinux'
        workloadRuntime: 'KataCcIsolation'
        enableAutoScaling: true
        count: aks_user_node_count
        minCount: aks_user_node_count
        maxCount: 10
        availabilityZones: aks_availability_zones
      }
    ]
    ingressProfile: {
      webAppRouting: {
        enabled: true
        // dnsZoneResourceIds: [
        //   dns_zone_resource.id
        // ]
      }
    }
    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }
    oidcIssuerProfile: {
      enabled: true
    }
    servicePrincipalProfile: {
      clientId: 'msi'
    }
    workloadAutoScalerProfile: {
      keda: {
        enabled: true
      }
    }
  }
  sku: {
    name: 'Base'
    tier: aks_tier
  }
  identity: {
    type: 'SystemAssigned'
  }
}
