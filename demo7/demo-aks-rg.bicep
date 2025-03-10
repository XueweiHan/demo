targetScope = 'subscription'

param project_name string = 'hunter-test'
param location string = 'eastus'

var project = toLower(project_name)
var aks_resource_group = '${project}-rg'

resource aksResourceGroup 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: aks_resource_group
  location: location
}

module aksResources './demo-aks.bicep' = {
  name: 'deploy-resources'
  scope: resourceGroup(aks_resource_group)
  params: {
    project_name: project
    location: location
  }
  dependsOn: [aksResourceGroup]
}

module acrResources './demo-acr.bicep' = {
  name: 'deploy-acr'
  scope: resourceGroup(aks_resource_group)
  params: {
    project_name: project
    location: location
  }
  dependsOn: [aksResources]
}

module aksRGRoleAssignment './demo-rg-role.bicep' = {
  name: 'deploy-role-assignments-in-aks-rg'
  scope: resourceGroup(aksResourceGroup.name)
  params: {
    project_name: project
  }
  dependsOn: [aksResources]
}
