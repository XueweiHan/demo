targetScope = 'subscription'

param k8s_node_resource_group string
param location string

resource k8s_node_rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: k8s_node_resource_group
  location: location
}

output k8s_node_rg_name string = k8s_node_rg.name
