param project_name string = 'hunter-demo'
param shared_name string = 'hunter-shared'
param location string = resourceGroup().location

var shared_resource_group = '${shared_name}-rg'
var log_analytics_workspace_name = '${shared_name}-log-analytics-workspace'

var log_analytics_workspace_id = resourceId(
  shared_resource_group,
  'Microsoft.OperationalInsights/workspaces',
  log_analytics_workspace_name
)

var aks_name = '${project_name}-aks'

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-10-01' = {
  location: location
  name: aks_name
  properties: {
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: log_analytics_workspace_id
        }
      }
    }
  }
}
