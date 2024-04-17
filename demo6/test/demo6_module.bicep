param project_name string = 'demo6'
param environment string = 'dev'

param k8s_name string = toLower('${project_name}-${environment}-aks')
param ingress_msi_resource_name string = toLower('ingressapplicationgateway-${k8s_name}')

resource ingress_msi_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: ingress_msi_resource_name
}

var NetworkContributorRoleId = '4d97b98b-1d4f-4787-a291-c67834d212e7'
resource subnet_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(ingress_msi_resource.id, 'Network Contributor', deployment().name)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', NetworkContributorRoleId)
    principalId: ingress_msi_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
