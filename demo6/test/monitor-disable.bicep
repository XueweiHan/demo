// https://ms.portal.azure.com/#view/HubsExtension/DeploymentDetailsBlade/~/overview/id/%2Fsubscriptions%2Fb47beaaf-7461-4b34-844a-7105d6b8c0d7%2FresourceGroups%2Fhunter-test1-rg%2Fproviders%2FMicrosoft.Resources%2Fdeployments%2FCIArmTemplateOnboardingDeployment-d760e29a-ebee-4c1a-8518-667bd3


param project_name string = 'hunter'
param environment string = 'test2'

param location string = resourceGroup().location
param aks_name string = toLower('${project_name}-${environment}-aks')

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-01-01' = {
  name: aks_name
  location: location
  properties: {
    azureMonitorProfile: {
      metrics: {
        enabled: false
      }
    }
  }
}
