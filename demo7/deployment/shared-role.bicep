param shared_name string
param project_name string

// resources in shared resource group
var container_registry_name = replace('${shared_name}cr', '-', '')
var keyvault_name = '${shared_name}-keyvault'

// resources in aks resource group / aks node resource group
var aks_name = '${project_name}-aks'
var aks_resource_group = '${project_name}-rg'
var aks_node_resource_group = '${aks_name}-node-rg'

var identity_name = '${project_name}-identity'

////////////////////////////////////////////////////////////////////////////////
// acr pull role for aks-agentpool identity

var aks_nodepool_identity_resource_id = resourceId(
  aks_node_resource_group,
  'Microsoft.ManagedIdentity/userAssignedIdentities',
  '${aks_name}-agentpool'
)

var acr_roles = [
  '7f951dda-4ed3-4680-a7ca-43fe172d538d' // AcrPull
]

resource container_registry_resource 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: container_registry_name
}

resource arc_pull_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for role in acr_roles: {
    name: guid(container_registry_resource.id, role, aks_nodepool_identity_resource_id)
    scope: container_registry_resource
    properties: {
      roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', role)
      principalId: reference(aks_nodepool_identity_resource_id, '2023-01-31').principalId
      principalType: 'ServicePrincipal'
    }
  }
]

////////////////////////////////////////////////////////////////////////////////
// keyvault roles for aks-identity (workload identity)

var identity_resource_id = resourceId(
  aks_resource_group,
  'Microsoft.ManagedIdentity/userAssignedIdentities',
  identity_name
)

var keyvault_roles = [
  '4633458b-17de-408a-b874-0445c86b69e6' //'Key Vault Secrets User'
  '14b46e9e-c2b7-41b4-b07b-48a6ebf60603' //'Key Vault Crypto Officer'
]

resource keyvault_resource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvault_name
}

resource keyvault_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for role in keyvault_roles: {
    name: guid(keyvault_resource.id, role, identity_resource_id)
    scope: keyvault_resource
    properties: {
      roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', role)
      principalId: reference(identity_resource_id, '2023-01-31').principalId
      principalType: 'ServicePrincipal'
    }
  }
]
