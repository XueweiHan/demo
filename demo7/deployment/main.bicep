targetScope = 'subscription'

param project_name string = 'hunter-demo'
param shared_name string = 'hunter-shared'
param location string = 'eastus'

var project = toLower(project_name)
var shared = toLower(shared_name)

module share_rg './share-rg.bicep' = {
  name: 'deploy-share-rg'
  params: {
    shared_name: shared
    location: location
  }
}

module aks_rg './aks-rg.bicep' = {
  name: 'deploy-aks-rg'
  params: {
    project_name: project
    shared_name: shared
    location: location
  }
  dependsOn: [share_rg]
}
