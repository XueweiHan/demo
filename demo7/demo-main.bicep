targetScope = 'subscription'

param project_name string = 'hunter-test2'
param location string = 'eastus'

var project = toLower(project_name)

module aks_rg './demo-aks-rg.bicep' = {
  name: 'deploy-aks-rg'
  params: {
    project_name: project
    location: location
  }
}

module aks_node_rg './demo-aks-node-rg.bicep' = {
  name: 'deploy-aks-node-rg'
  params: {
    project_name: project
  }
  dependsOn: [aks_rg]
}
