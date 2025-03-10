param project_name string = 'demo7'

param aks_name string = toLower('${project_name}-aks')
param location string = resourceGroup().location

param log_analytics_workspace_name string = toLower('${project_name}-log-analytics-workspace')

resource log_analytics_workspace_resource 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: log_analytics_workspace_name
  location: location
}

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-07-01' = {
  location: location
  name: aks_name
  properties: {
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: log_analytics_workspace_resource.id
        }
      }
    }
  }
}
