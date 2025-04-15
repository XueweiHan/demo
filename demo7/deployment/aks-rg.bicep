targetScope = 'subscription'

param project_name string
param shared_name string
param location string

var aks_resource_group = '${project_name}-rg'
var shared_resource_group = '${shared_name}-rg'

resource aks_rg 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: aks_resource_group
  location: location
}

module aks_resources './aks.bicep' = {
  name: 'deploy-aks-resources'
  scope: aks_rg
  params: {
    project_name: project_name
    location: location
  }
}

module shared_role './shared-role.bicep' = {
  name: 'deploy-role-assignments-in-shared-rg'
  scope: resourceGroup(shared_resource_group)
  params: {
    project_name: project_name
    shared_name: shared_name
  }
  dependsOn: [aks_resources]
}
