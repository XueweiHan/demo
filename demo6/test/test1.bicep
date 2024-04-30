resource deployment 'Microsoft.Resources/deployments@2023-07-01' = {
  name: 'deploy-aks'
  location: 'eastus'
  properties: {
    mode: 'Incremental'
    template: {
      $schema: 'https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#',
      contentVersion: '
