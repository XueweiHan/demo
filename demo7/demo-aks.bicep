param project_name string
param location string = resourceGroup().location

param aks_tier string = 'Standard'
param keyvault_sku string = 'Premium'

param aks_vmsize string = 'Standard_D3'
param aks_system_node_count int = 2
param aks_user_node_count int = 2

param public_ip_name string = '${toLower(project_name)}-public-ip'
param nat_gateway_name string = '${toLower(project_name)}-nat-gateway'
param vnet_name string = '${toLower(project_name)}-vnet'

param aks_name string = '${toLower(project_name)}-aks'
param aks_node_resource_group string = '${aks_name}-infra-rg'
param aks_dns_prefix string = '${toLower(project_name)}-dns-prefix'

param identity_name string = '${toLower(project_name)}-identity'
param identity_federated_credentials_name string = '${toLower(project_name)}-service-account-credentials'
param aks_service_account_name string = 'demo7-sa'

param keyvault_name string = '${toLower(project_name)}-keyvault'
// param container_registry_name string = replace(toLower('${toLower(project_name)}cr'), '-', '')

resource public_ip_resource 'Microsoft.Network/publicIPAddresses@2024-01-01' = {
  name: public_ip_name
  location: location
  properties: {
    publicIPAllocationMethod: 'Static'
    publicIPAddressVersion: 'IPv4'
  }
  sku: {
    name: 'Standard'
  }
}

resource nat_gateway_resource 'Microsoft.Network/natGateways@2024-01-01' = {
  name: nat_gateway_name
  location: location
  properties: {
    idleTimeoutInMinutes: 10
    publicIpAddresses: [
      {
        id: public_ip_resource.id
      }
    ]
  }
  sku: {
    name: 'Standard'
  }
}

resource vnet_resource 'Microsoft.Network/virtualNetworks@2024-01-01' = {
  name: vnet_name
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
        '10.1.0.0/16'
        '10.2.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'default'
        properties: {
          addressPrefix: '10.0.0.0/24'
        }
      }
      {
        name: 'aks'
        properties: {
          addressPrefix: '10.1.0.0/16'
        }
      }
      {
        name: 'cg'
        properties: {
          addressPrefix: '10.2.0.0/16'
          natGateway: {
            id: nat_gateway_resource.id
          }
          delegations: [
            {
              name: 'containerGroups'
              properties: {
                serviceName: 'Microsoft.ContainerInstance/containerGroups'
              }
            }
          ]
        }
      }
    ]
  }
}

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-07-01' = {
  location: location
  name: aks_name
  properties: {
    kubernetesVersion: '1.29.9'
    enableRBAC: true
    dnsPrefix: aks_dns_prefix
    nodeResourceGroup: aks_node_resource_group
    agentPoolProfiles: [
      {
        name: 'agentpool'
        mode: 'System'
        vmSize: aks_vmsize
        osType: 'Linux'
        osSKU: 'AzureLinux'
        enableAutoScaling: true
        count: aks_system_node_count
        minCount: aks_system_node_count
        maxCount: 5
        nodeTaints: ['CriticalAddonsOnly=true:NoSchedule']
        // vnetSubnetID: vnet_resource.properties.subnets[1].id
        vnetSubnetID: resourceId('Microsoft.Network/virtualNetworks/subnets', vnet_name, 'aks')
      }
      {
        name: 'userpool'
        mode: 'User'
        vmSize: aks_vmsize
        osType: 'Linux'
        osSKU: 'AzureLinux'
        enableAutoScaling: true
        count: aks_user_node_count
        minCount: aks_user_node_count
        maxCount: 10
        // vnetSubnetID: vnet_resource.properties.subnets[1].id
        vnetSubnetID: resourceId('Microsoft.Network/virtualNetworks/subnets', vnet_name, 'aks')
      }
    ]
    autoUpgradeProfile: {
      upgradeChannel: 'patch'
    }
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'calico'
      serviceCidr: '10.4.0.0/16'
      dnsServiceIP: '10.4.0.10'
    }
    ingressProfile: {
      webAppRouting: {
        enabled: true
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
  dependsOn: [
    vnet_resource
  ]
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

var keyvault_roles = [
  '4633458b-17de-408a-b874-0445c86b69e6' //'Key Vault Secrets User'
  '14b46e9e-c2b7-41b4-b07b-48a6ebf60603' //'Key Vault Crypto Officer'
]

resource keyvault_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for role in keyvault_roles: {
    name: guid(keyvault_resource.id, role, identity_resource.id)
    scope: keyvault_resource
    properties: {
      roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', role)
      principalId: identity_resource.properties.principalId
      principalType: 'ServicePrincipal'
    }
  }
]

// output aks_node_resource_group string = aks_resource.properties.nodeResourceGroup
