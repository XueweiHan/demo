param project_name string = 'Certnoti'
param environment string = 'Dev'
param osDiskSizeGB int = 256
param location string = resourceGroup().location
param logworkspace_name string = toLower('${project_name})-${environment}-logworkspace')
param appinsights_name string = toLower('${project_name}-${environment}-appinsights')
param keyvault_name string = toLower('${project_name}-${environment}-keyvault')
param identity_name string = toLower('${project_name}-${environment}-identity')
param identity_federated_credentials_name string = toLower('${project_name}-${environment}-service-account-credentials')
param publicip_name string = toLower('${project_name}-${environment}-publicip')
param k8s_name string = toLower('${project_name}-${environment}-aks')
param k8s_dns_prefix string = toLower('${project_name}-${environment}-dns-prefix')
param k8s_node_resource_group string = toLower('${project_name}-${environment}-aks-infra')
param k8s_service_account_name string = toLower('${project_name}-sa')

@allowed([
  'Standard_DS2_v2'
  'Standard_D2ads_v5'
  'Standard_D8ds_v5'
  'Standard_D8ads_v5'
  'Standard_DC4as_cc_v5'
])
param k8_vmsize string = 'Standard_DC4as_cc_v5'

@allowed(['Managed', 'Ephemeral'])
param osDiskType string = 'Managed'

resource logworkspace_resource 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logworkspace_name
  location: location
}

resource appinsights_resource 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: appinsights_name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logworkspace_resource.id
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
    issuer: k8s_resource.properties.oidcIssuerProfile.issuerURL
    subject: 'system:serviceaccount:default:${k8s_service_account_name}'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource keyvault_resource 'Microsoft.KeyVault/vaults@2023-07-01' = {
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

// This role assignment only works for the first time
// Second time it will throw an error "Already Exists"
// https://learn.microsoft.com/en-us/answers/questions/857343/azure-bicep-authorization-roleassignements-to-stor
//
// var KeyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'
// resource keyvault_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(keyvault_resource.id, 'Key Vault Secrets User', deployment().name)
//   scope: keyvault_resource
//   properties: {
//     roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', KeyVaultSecretsUserRoleId)
//     principalId: identity_resource.properties.principalId
//     principalType: 'ServicePrincipal'
//   }
// }

resource publicip_resource 'Microsoft.Network/publicIPAddresses@2023-09-01' = {
  name: publicip_name
  location: location
  properties: {
    publicIPAllocationMethod: 'Static'
    publicIPAddressVersion: 'IPv4'
  }
  sku: {
    name: 'Standard'
  }
}



resource k8s_resource 'Microsoft.ContainerService/managedClusters@2023-10-02-preview' = {
  location: location
  name: k8s_name
  properties: {
    kubernetesVersion: '1.27.9'
    enableRBAC: true
    dnsPrefix: k8s_dns_prefix
    nodeResourceGroup: k8s_node_resource_group
    agentPoolProfiles: [
      {
        name: 'agentpool'
        mode: 'System'
        vmSize: k8_vmsize
        osDiskType: osDiskType
        osDiskSizeGB: osDiskSizeGB
        osType: 'Linux'
        osSKU: 'AzureLinux'
        type: 'VirtualMachineScaleSets'
        enableAutoScaling: true
        count: 2
        minCount: 2
        maxCount: 5
        maxPods: 110
        enableNodePublicIP: false
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
        nodeTaints: [
          'CriticalAddonsOnly=true:NoSchedule'
        ]
      }
      {
        name: 'userpool'
        mode: 'User'
        vmSize: k8_vmsize
        osDiskType: osDiskType
        osDiskSizeGB: osDiskSizeGB
        osType: 'Linux'
        osSKU: 'AzureLinux'
        workloadRuntime: 'KataCcIsolation'
        type: 'VirtualMachineScaleSets'
        enableAutoScaling: true
        count: 2
        minCount: 2
        maxCount: 5
        maxPods: 110
        enableNodePublicIP: false
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
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
      azurepolicy: {
        enabled: false
      }
      azureKeyvaultSecretsProvider: {
        enabled: false
      }
      omsAgent: {
        enabled: false
      }
      ingressApplicationGateway: {
        enabled: false
        config: {
          sku: 'Standard_v2'
          applicationGatewayId: ''
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
    azureMonitorProfile: {
      metrics: {
        enabled: true
        kubeStateMetrics: {
          metricLabelsAllowlist: ''
          metricAnnotationsAllowList: ''
        }
      }
    }
  }
  tags: {}
  sku: {
    name: 'Base'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  dependsOn: []
}
