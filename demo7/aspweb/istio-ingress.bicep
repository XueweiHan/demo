param project_name string = 'hunter-demo'

param location string = resourceGroup().location

var aks_name = '${project_name}-aks'

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-10-01' = {
  location: location
  name: aks_name
  properties: {
    serviceMeshProfile: {
      mode: 'Istio'
      istio: {
        components: {
          ingressGateways: [
            {
              mode: 'External'
              enabled: true
            }
          ]
        }
      }
    }
  }
}
