param project_name string = 'hunter'
param environment string = 'demo6'

param location string = resourceGroup().location
param aks_name string = toLower('${project_name}-${environment}-aks')

param log_analytics_workspace_name string = toLower('${project_name}-${environment}-log-analytics-workspace')
param log_dcr_name string = toLower('${project_name}-${environment}-log-dcr')
param log_dcra_name string = toLower('${project_name}-${environment}-log-dcra')

resource log_analytics_workspace_resource 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: log_analytics_workspace_name
  location: location
}

resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-01-01' = {
  name: aks_name
  location: location
  properties: {
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: log_analytics_workspace_resource.id
          useAADAuth: 'true'
        }
      }
    }
  }
}

resource log_dcr_resource 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: log_dcr_name
  location: location
  kind: 'Linux'
  properties: {
    dataSources: {
      extensions: [
        {
          name: 'ContainerInsightsExtension'
          extensionName: 'ContainerInsights'
          streams: [
            'Microsoft-ContainerLog'
            'Microsoft-ContainerLogV2'
            'Microsoft-KubeEvents'
            'Microsoft-KubePodInventory'
          ]
          extensionSettings: {
            dataCollectionSettings: {
              interval: '1m'
              namespaceFilteringMode: 'Off'
              enableContainerLogV2: true
            }
          }
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Microsoft-ContainerLog'
          'Microsoft-ContainerLogV2'
          'Microsoft-KubeEvents'
          'Microsoft-KubePodInventory'
        ]
        destinations: ['ciworkspace']
      }
    ]
    destinations: {
      logAnalytics: [
        {
          name: 'ciworkspace'
          workspaceResourceId: log_analytics_workspace_resource.id
        }
      ]
    }
  }
}

resource log_dcra_resource 'Microsoft.Insights/dataCollectionRuleAssociations@2022-06-01' = {
  name: log_dcra_name
  scope: aks_resource
  properties: {
    description: 'Association of data collection rule. Deleting this association will break the data collection for this AKS Cluster.'
    dataCollectionRuleId: log_dcr_resource.id
  }
}
