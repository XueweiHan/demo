param project_name string = 'demo7'

param location string = resourceGroup().location
param identity_name string = toLower('${project_name}-identity')
param service_bus_namespace_name string = toLower('${project_name}-service-bus')
param service_bus_queue_name1 string = 'queue1'
param service_bus_queue_name2 string = 'queue2'

resource identity_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identity_name
}

resource service_bus_namespace_resource 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: service_bus_namespace_name
  location: location
  sku: { name: 'Standard' }
}

resource serviceBusQueue1 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: service_bus_namespace_resource
  name: service_bus_queue_name1
  properties: {
    lockDuration: 'PT1M'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusQueue2 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: service_bus_namespace_resource
  name: service_bus_queue_name2
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

var AzureServiceBusDataReceiverRoleId = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
resource storage_account_contributor_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(service_bus_namespace_resource.id, 'Azure Service Bus Data Receiver', identity_resource.id)
  scope: service_bus_namespace_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', AzureServiceBusDataReceiverRoleId)
    principalId: identity_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
