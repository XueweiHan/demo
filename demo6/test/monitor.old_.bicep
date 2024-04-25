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

param montior_workspace_name string = toLower('${project_name}-${environment}-monitor-workspace')
param dceName string = toLower('${project_name}-${environment}-dce')
param dcrName string = toLower('${project_name}-${environment}-dcr')
param dcraName string = toLower('${project_name}-${environment}-dcra')

resource monitor_workspace_resource 'Microsoft.Monitor/accounts@2023-04-03' = {
  name: montior_workspace_name
  location: location
}

resource dce 'Microsoft.Insights/dataCollectionEndpoints@2022-06-01' = {
  name: dceName
  location: location
  kind: 'Linux'
}

resource dcr 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: dcrName
  location: location
  kind: 'Linux'
  properties: {
    dataCollectionEndpointId: dce.id
    description: 'DCR for Azure Monitor Metrics Profile (Managed Prometheus)'
    dataSources: {
      prometheusForwarder: [
        {
          name: 'PrometheusDataSource'
          streams: ['Microsoft-PrometheusMetrics']
        }
      ]
    }
    dataFlows: [
      {
        streams: ['Microsoft-PrometheusMetrics']
        destinations: ['MonitoringAccount1']
      }
    ]
    destinations: {
      monitoringAccounts: [
        {
          name: 'MonitoringAccount1'
          accountResourceId: monitor_workspace_resource.id
        }
      ]
    }
  }
}


// module azuremonitormetrics_dcra_clusterResourceId './nested_azuremonitormetrics_dcra_clusterResourceId.bicep' = {
//   name: 'azuremonitormetrics-dcra-${uniqueString(clusterResourceId)}'
//   scope: resourceGroup(clusterSubscriptionId, clusterResourceGroup)
//   params: {
//     resourceId_Microsoft_Insights_dataCollectionRules_variables_dcrName: dcr.id
//     variables_clusterName: clusterName
//     variables_dcraName: dcraName
//     clusterLocation: clusterLocation
//   }

// }


resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-01-01' existing = {
  name: aks_name
}

resource dcra 'Microsoft.ContainerService/managedClusters/providers/dataCollectionRuleAssociations@2022-06-01' = {
  name: dcraName
  location: location
  properties: {
    description: 'Association of data collection rule. Deleting this association will break the data collection for this AKS Cluster.'
    dataCollectionRuleId: dcr.id
  }
  dependsOn: [dce]
}
