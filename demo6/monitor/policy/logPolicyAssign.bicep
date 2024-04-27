
targetScope = 'subscription'

param logAnalyticsWorkspaceResourceId string
param logAnalyticsWorkspaceLocation string

param policyDefinitionId string
param policyAssignmentName string = split(policyDefinitionId, '/')[6]

resource policy_assignment_resource 'Microsoft.Authorization/policyAssignments@2023-04-01' = {
  name: policyAssignmentName
  scope: subscription()
  location: logAnalyticsWorkspaceLocation
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    displayName: policyAssignmentName
    policyDefinitionId: policyDefinitionId
    parameters: {
      logAnalyticsWorkspaceResourceId: {
        value: logAnalyticsWorkspaceResourceId
      }
      logAnalyticsWorkspaceLocation: {
        value: logAnalyticsWorkspaceLocation
      }
    }
  }
}

var ContributorRoleId = 'b24988ac-6180-42a0-ab88-20f7382dd24c'
resource contributor_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(policy_assignment_resource.id, 'Contributor', subscription().subscriptionId)
  scope: subscription()
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', ContributorRoleId)
    principalId: policy_assignment_resource.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
