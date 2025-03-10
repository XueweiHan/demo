targetScope = 'subscription'

param project_name string = 'hunter-test'
// param location string = 'eastus'

var project = toLower(project_name)
var aks_name = '${project}-aks'
var aks_node_resource_group = '${aks_name}-infra-rg'

module aksNodeRGRoleAssigment './demo-rg-role.bicep' = {
  name: 'deploy-role-assignments-in-aks-node-rg'
  scope: resourceGroup(aks_node_resource_group)
  params: {
    project_name: project
  }
}
