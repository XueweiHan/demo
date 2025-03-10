param project_name string = 'demo7'

param location string = resourceGroup().location
param storage_account_name string = replace(toLower('${project_name}sa'), '-', '')

param file_share_name string = toLower('${project_name}-log-file-share')

resource storage_account_resource 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storage_account_name
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  }

resource file_service 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  parent: storage_account_resource
  name: 'default'
}

resource file_share 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  parent: file_service
  name: file_share_name
  properties: {
    shareQuota: 4  // Define the quota in GB
  }
}
