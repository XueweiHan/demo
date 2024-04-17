param project_name string = 'demo6'
param environment string = 'dev'
param k8_vmsize string = 'Standard_B2pls_v2'
param location string = resourceGroup().location
param identity_name string = toLower('${project_name}-${environment}-identity')
param identity_federated_credentials_name string = toLower('${project_name}-${environment}-service-account-credentials')
param k8s_name string = toLower('${project_name}-${environment}-aks')
param k8s_dns_prefix string = toLower('${project_name}-${environment}-dns-prefix')
param k8s_node_resource_group string = toLower('${project_name}-${environment}-aks-infra')
param k8s_service_account_name string = toLower('${project_name}-sa')
param applcation_gateway_name string = toLower('${project_name}-${environment}-app-gateway')
param keyvault_name string = toLower('${project_name}-${environment}-keyvault')
param ingress_msi_resource_name string = toLower('ingressapplicationgateway-${k8s_name}')

var KeyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource keyvault_name_resource 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyvault_name
  location: location
  properties: {
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    sku: {
      family: 'A'
      name: 'standard'
    }
  }
}

resource identity_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identity_name
  location: location
}

resource identity_name_identity_federated_credentials_name 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: identity_name_resource
  name: '${identity_federated_credentials_name}'
  properties: {
    issuer: reference(k8s_name_resource.id, '2023-10-02-preview').oidcIssuerProfile.issuerURL
    subject: 'system:serviceaccount:default:${k8s_service_account_name}'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource Microsoft_KeyVault_vaults_keyvault_name_Key_Vault_Secrets_User_name 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyvault_name_resource
  name: guid(keyvault_name_resource.id, 'Key Vault Secrets User', deployment().name)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', KeyVaultSecretsUserRoleId)
    principalId: reference(identity_name_resource.id, '2023-01-31').principalId
    principalType: 'ServicePrincipal'
  }
}

resource k8s_name_resource 'Microsoft.ContainerService/managedClusters@2023-10-02-preview' = {
  name: k8s_name
  location: location
  properties: {
    kubernetesVersion: '1.29.2'
    enableRBAC: true
    dnsPrefix: k8s_dns_prefix
    nodeResourceGroup: k8s_node_resource_group
    agentPoolProfiles: [
      {
        name: 'nodepool'
        mode: 'System'
        vmSize: k8_vmsize
        osType: 'Linux'
        osSKU: 'AzureLinux'
        enableAutoScaling: false
        count: 1
        enableNodePublicIP: false
      }
    ]
    networkProfile: {
      loadBalancerSku: 'standard'
      networkPlugin: 'azure'
      networkPolicy: 'calico'
    }
    autoUpgradeProfile: {
      upgradeChannel: 'patch'
    }
    disableLocalAccounts: false
    apiServerAccessProfile: {
      enablePrivateCluster: false
    }
    addonProfiles: {
      ingressApplicationGateway: {
        enabled: true
        config: {
          sku: 'Standard_v2'
          name: applcation_gateway_name
          subnetCIDR: '10.225.0.0/16'
          watchNamespace: k8s_node_resource_group
        }
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
  }
  identity: {
    type: 'SystemAssigned'
  }
}

module role_assign './nested_role_assign.bicep' = {
  name: 'role_assign'
  scope: resourceGroup(k8s_node_resource_group)
  params: {
    ingress_msi_resource_name: ingress_msi_resource_name
  }
}
