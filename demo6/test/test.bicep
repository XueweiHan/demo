param project_name string = 'hunter'
param environment string = 'demo6'

/*
param aks_vmsize string = 'Standard_B2pls_v2'
param aks_tier string = 'Free'
param aks_node_count int = 1
param keyvault_sku string = 'Standard'
/*/
param aks_vmsize string = 'Standard_DC4as_cc_v5'
param aks_tier string = 'Standard'
param aks_node_count int = 3
param keyvault_sku string = 'Premium'
//*/

param location string = resourceGroup().location
param identity_name string = toLower('${project_name}-${environment}-identity')
param identity_federated_credentials_name string = toLower('${project_name}-${environment}-service-account-credentials')
param aks_name string = toLower('${project_name}-${environment}-aks')
param aks_dns_prefix string = toLower('${project_name}-${environment}-dns-prefix')
param aks_node_resource_group string = toLower('${project_name}-${environment}-aks-infra-rg')
param aks_service_account_name string = toLower('${project_name}-sa')
param keyvault_name string = toLower('${project_name}-${environment}-keyvault')
param attestation_provider_name string = toLower('${project_name}${environment}Attest') 

resource keyvault_resource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvault_name
  // location: location
  // properties: {
  //   tenantId: subscription().tenantId
  //   enableRbacAuthorization: true
  //   enableSoftDelete: false
  //   sku: {
  //     family: 'A'
  //     name: keyvault_sku
  //   }
  // }
}

resource identity_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identity_name
  // location: location
}

// var KeyVaultCryptoUserRoleId = '12338af0-0e69-4776-bea7-57ae8d297424'
// resource keyvault_crypto_user_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(keyvault_resource.id, 'Key Vault Crypto User', deployment().name)
//   scope: keyvault_resource
//   properties: {
//     roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', KeyVaultCryptoUserRoleId)
//     principalId: identity_resource.properties.principalId
//     principalType: 'ServicePrincipal'
//   }
// }

var KeyVaultCryptoOfficerRoleId = '14b46e9e-c2b7-41b4-b07b-48a6ebf60603'
resource keyvault_crypto_officer_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyvault_resource.id, 'Key Vault Crypto Officer', identity_resource.id)
  scope: keyvault_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', KeyVaultCryptoOfficerRoleId)
    principalId: identity_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
