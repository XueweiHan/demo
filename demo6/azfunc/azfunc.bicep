param project_name string = 'hunter'
param environment string = 'demo6'

param location string = resourceGroup().location
param storage_account_name string = toLower('${project_name}${environment}sa')
param identity_name string = toLower('${project_name}-${environment}-identity')


resource identity_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identity_name
}

resource storage_account_resource 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storage_account_name
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: {
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    encryption: {
      requireInfrastructureEncryption: true
      services: {
        blob: { enabled: true }
        file: { enabled: true }
        queue: { enabled: true }
        table: { enabled: true }
      }
    }
  }
}

var StorageAccountContributorRoleId = '17d1049b-9a84-46fb-8f53-869881c3d3ab'
resource storage_account_contributor_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage_account_resource.id, 'Storage Account Contributor', identity_resource.id)
  scope: storage_account_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', StorageAccountContributorRoleId)
    principalId: identity_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

var StorageBlobDataOwnerRoleId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
resource storage_blob_data_owner_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage_account_resource.id, 'Storage Blob Data Owner', identity_resource.id)
  scope: storage_account_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', StorageBlobDataOwnerRoleId)
    principalId: identity_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

var StorageQueueDataContributorRoleId = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
resource storage_queue_data_contributor_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage_account_resource.id, 'Storage Queue Data Contributor', identity_resource.id)
  scope: storage_account_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', StorageQueueDataContributorRoleId)
    principalId: identity_resource.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
