targetScope = 'subscription'

param shared_name string
param location string

var shared_resource_group = '${shared_name}-rg'

resource shared_rg 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: shared_resource_group
  location: location
}

module shared_resources './shared.bicep' = {
  name: 'deploy-acr'
  scope: shared_rg //resourceGroup(shared_resource_group)
  params: {
    shared_name: shared_name
    location: location
  }
}
