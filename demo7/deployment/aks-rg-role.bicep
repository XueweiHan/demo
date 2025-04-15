param project_name string

var aks_name = '${project_name}-aks'
var aks_node_resource_group = '${aks_name}-node-rg'
var aks_nodepool_identity = '${aks_name}-agentpool'

var ContributorRoleId = 'b24988ac-6180-42a0-ab88-20f7382dd24c'

resource resource_group_contributor_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, 'Contributor', resourceId(aks_node_resource_group, 'Microsoft.ManagedIdentity/userAssignedIdentities', aks_nodepool_identity))
  scope: resourceGroup()
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', ContributorRoleId)
    principalId: reference(resourceId(aks_node_resource_group, 'Microsoft.ManagedIdentity/userAssignedIdentities', aks_nodepool_identity), '2023-01-31').principalId
    // principalId: identity_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
