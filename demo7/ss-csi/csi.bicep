param project_name string = 'demo7'

param location string = resourceGroup().location
param aks_name string = toLower('${project_name}-aks')

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-07-01' = {
  location: location
  name: aks_name
  properties: {
    addonProfiles: {
      azureKeyVaultSecretsProvider: {
        enabled: true
        config: {
          enableSecretRotation: 'true'
          rotationPollInterval: '2m'
        }
      }
    }
  }
}
