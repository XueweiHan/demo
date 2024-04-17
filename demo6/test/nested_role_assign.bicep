param ingress_msi_resource_name string

var NetworkContributorRoleId = '4d97b98b-1d4f-4787-a291-c67834d212e7'

resource Microsoft_ManagedIdentity_userAssignedIdentities_ingress_msi_resource_name_Network_Contributor_name 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: 'Microsoft.Network/virtualNetworks/${ingress_msi_resource_name}/subnets/test-app-gateway-subnet'
  name: guid(
    resourceId('Microsoft.ManagedIdentity/userAssignedIdentities', ingress_msi_resource_name),
    'Network Contributor',
    deployment().name
  )
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', NetworkContributorRoleId)
    principalId: reference(
      resourceId('Microsoft.ManagedIdentity/userAssignedIdentities', ingress_msi_resource_name),
      '2023-01-31'
    ).principalId
    principalType: 'ServicePrincipal'
  }
}
