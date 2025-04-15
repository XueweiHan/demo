param project_name string
param location string = resourceGroup().location

var aks_tier = 'Standard'
var aks_vmsize = 'Standard_D3'
var aks_system_node_count = 2
var aks_user_node_count = 2

var public_ip_name = '${project_name}-public-ip'
var nat_gateway_name = '${project_name}-nat-gateway'
var vnet_name = '${project_name}-vnet'

var aks_name = '${project_name}-aks'
var aks_node_resource_group = '${aks_name}-node-rg'
var aks_dns_prefix = '${project_name}-dns-prefix'

var identity_name = '${project_name}-identity'
var identity_federated_credentials_name = '${project_name}-service-account-credentials'
var aks_service_account_name = 'demo-sa'

resource public_ip_resource 'Microsoft.Network/publicIPAddresses@2024-05-01' = {
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

resource nat_gateway_resource 'Microsoft.Network/natGateways@2024-05-01' = {
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

resource vnet_resource 'Microsoft.Network/virtualNetworks@2024-05-01' = {
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
          defaultOutboundAccess: false
        }
      }
      {
        name: 'aks'
        properties: {
          addressPrefix: '10.1.0.0/16'
          defaultOutboundAccess: false
        }
      }
      {
        name: 'cg'
        properties: {
          addressPrefix: '10.2.0.0/16'
          defaultOutboundAccess: false
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

resource subnet_aks 'Microsoft.Network/virtualNetworks/subnets@2024-05-01' existing = {
  parent: vnet_resource
  name: 'aks'
}

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-07-01' = {
  location: location
  name: aks_name
  properties: {
    // kubernetesVersion: '1.29.9'
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
        // vnetSubnetID: resourceId('Microsoft.Network/virtualNetworks/subnets', vnet_name, 'aks')
        vnetSubnetID: subnet_aks.id
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
        // vnetSubnetID: resourceId('Microsoft.Network/virtualNetworks/subnets', vnet_name, 'aks')
        vnetSubnetID: subnet_aks.id
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

module aks_rg_role './aks-rg-role.bicep' = {
  name: 'deploy-role-assignments-in-aks-rg'
  // scope: resourceGroup()
  params: {
    project_name: project_name
  }
  dependsOn: [aks_resource]
}

module aks_node_rg_role './aks-rg-role.bicep' = {
  name: 'deploy-role-assignments-in-aks-node-rg'
  scope: resourceGroup(aks_node_resource_group)
  params: {
    project_name: project_name
  }
  dependsOn: [aks_resource]
}
