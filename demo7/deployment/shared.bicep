param shared_name string
param location string = resourceGroup().location

var container_registry_name = replace('${shared_name}cr', '-', '')
var keyvault_name = '${shared_name}-keyvault'
var log_analytics_workspace_name = '${shared_name}-log-analytics-workspace'
var service_bus_namespace_name = '${shared_name}-service-bus'

resource container_registry_resource 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: container_registry_name
  location: location
  properties: {
    adminUserEnabled: false
  }
  sku: {
    name: 'Premium'
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
      name: 'premium'
    }
  }
}

resource log_analytics_workspace_resource 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: log_analytics_workspace_name
  location: location
}

resource service_bus_namespace_resource 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: service_bus_namespace_name
  location: location
  sku: { name: 'Standard' }
}
