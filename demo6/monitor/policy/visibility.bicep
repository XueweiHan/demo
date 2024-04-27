param location string = resourceGroup().location

var log_analytics_workspace_name = 'AKS-Log-Analytics-Workspace'
var montior_workspace_name = 'AKS-Monitor-Workspace'
param grafana_name string = 'AKS-Grafana'

resource log_analytics_workspace_resource 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: log_analytics_workspace_name
  location: location
}

resource monitor_workspace_resource 'Microsoft.Monitor/accounts@2023-04-03' = {
  name: montior_workspace_name
  location: location
}

resource grafana_resource 'Microsoft.Dashboard/grafana@2023-09-01' = {
  name: grafana_name
  location: location
  sku: {
    name: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    grafanaIntegrations: {
      azureMonitorWorkspaceIntegrations: [
        {
          azureMonitorWorkspaceResourceId: monitor_workspace_resource.id
        }
      ]
    }
  }
}

var MonitoringDataReaderRoleId = 'b0d8363b-8ddd-447d-831f-62ca05bff136'
resource monitoring_data_reader_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(monitor_workspace_resource.id, 'Monitoring Data Reader', grafana_resource.id)
  scope: monitor_workspace_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', MonitoringDataReaderRoleId)
    principalId: grafana_resource.identity.principalId
  }
}

var MonitoringReaderRoleId = '43d0d8ad-25c7-4714-9337-8ba259a9fe05'
resource monitoring_reader_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, 'Monitoring Reader', grafana_resource.id)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', MonitoringReaderRoleId)
    principalId: grafana_resource.identity.principalId
  }
}
