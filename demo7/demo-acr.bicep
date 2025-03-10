param project_name string
param location string = resourceGroup().location

param aks_name string = '${toLower(project_name)}-aks'
param aks_node_resource_group string = '${aks_name}-infra-rg'
param container_registry_name string = replace(toLower('${toLower(project_name)}cr'), '-', '')

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

var aks_nodepool_identity_resource_id = resourceId(
  aks_node_resource_group,
  'Microsoft.ManagedIdentity/userAssignedIdentities',
  '${aks_name}-agentpool'
)

var AcrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'
resource container_registry_arc_pull_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(container_registry_resource.id, 'AcrPull', aks_nodepool_identity_resource_id)
  scope: container_registry_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', AcrPullRoleId)
    principalId: reference(aks_nodepool_identity_resource_id, '2023-01-31').principalId
    principalType: 'ServicePrincipal'
  }
}
