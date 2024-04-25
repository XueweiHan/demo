resource ContainerInsightsExtension 'Microsoft.Insights/dataCollectionRuleAssociations@2022-06-01' = {
  properties: {
    description: 'Association of data collection rule. Deleting this association will break the data collection for this AKS Cluster.'
    dataCollectionRuleId: '/subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/hunter-test1-rg/providers/Microsoft.Insights/dataCollectionRules/MSCI-eastus-hunter-test1-aks'
  }
  name: 'ContainerInsightsExtension'
}
