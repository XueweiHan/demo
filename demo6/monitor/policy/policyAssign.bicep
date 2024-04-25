
targetScope = 'subscription'

param azureMonitorWorkspaceResourceId string
param azureMonitorWorkspaceLocation string
param policyDefinitionId string
param enableWindowsRecordingRules bool = false

param policyAssignmentName string = split(policyDefinitionId, '/')[6]

resource assignment 'Microsoft.Authorization/policyAssignments@2023-04-01' = {
  name: policyAssignmentName
  scope: subscription()
  location: azureMonitorWorkspaceLocation
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    displayName: policyAssignmentName
    policyDefinitionId: policyDefinitionId
    parameters: {
      azureMonitorWorkspaceResourceId: {
        value: azureMonitorWorkspaceResourceId
      }
      azureMonitorWorkspaceLocation: {
        value: azureMonitorWorkspaceLocation
      }
      enableWindowsRecordingRules :{
        value: enableWindowsRecordingRules
      }
    }
  }
}
