# metrics 
```
az login
az account set --subscription b47beaaf-7461-4b34-844a-7105d6b8c0d7
az configure --defaults location=eastus

use policy 
or
az deployment group create -g hunter-demo6-rg --template-file monitor.bicep


az role assignment create --role "Grafana Admin" --scope $(az grafana show -n hunter-demo6-grafana -g hunter-demo6-rg --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)


az aks get-credentials -n hunter-demo6-aks -g hunter-demo6-rg

kubectl get configmap -n kube-system
kubectl apply -f ama-metrics-settings-configmap.yaml
```


# policy (with this policy, we can skip the monitor.bicep deployment)
```
https://github.com/Azure/prometheus-collector/tree/main/AddonPolicyTemplate

az policy definition create -n aks-prometheus-metrics-addon --display-name "AKS Prometheus Metrics Addon" -m Indexed --metadata version=1.0.0 category=Kubernetes --rules ./AddonPolicyMetricsProfile.rules.json --params ./AddonPolicyMetricsProfile.parameters.json

az deployment sub create --template-file policyAssign.bicep --parameters "azureMonitorWorkspaceResourceId=$(az monitor account show -n AKS-Monitor-Workspace -g AKS-Monitoring-rg --query id -o tsv)" --parameters azureMonitorWorkspaceLocation=eastus --parameters enableWindowsRecordingRules=true --parameters "policyDefinitionId=$(az policy definition show -n aks-prometheus-metrics-addon --query id -o tsv)"

```




# metrics (AKS custom metrics https://learn.microsoft.com/en-us/azure/azure-monitor/containers/prometheus-metrics-scrape-configuration?tabs=CRDConfig%2CCRDScrapeConfig)

copy Azure Monitor Agent configmap template to local, and modify it
https://github.com/Azure/prometheus-collector/blob/main/otelcollector/configmaps/ama-metrics-settings-configmap.yaml

kubectl get configmap -n kube-system
kubectl apply -f ama-metrics-settings-configmap.yaml


https://learn.microsoft.com/en-us/azure/azure-monitor/containers/kubernetes-monitoring-enable?tabs=arm#enable-prometheus-and-grafana


    "properties": {
        "grafanaIntegrations": {
            "azureMonitorWorkspaceIntegrations": [
                {
                    "azureMonitorWorkspaceResourceId": "/subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/hunter-demo6-rg/providers/microsoft.monitor/accounts/hunter-demo6-monitor-workspace"
                }
            ]
        },


az aks update --enable-azure-monitor-metrics -n hunter-demo6-aks -g hunter-demo6-rg --azure-monitor-workspace-resource-id /subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourcegroups/hunter-demo6-rg/providers/microsoft.monitor/accounts/hunter-demo6-monitor-workspace --grafana-resource-id /subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/hunter-demo6-rg/providers/Microsoft.Dashboard/grafana/hunter-demo6-grafana


az aks update --enable-azure-monitor-metrics -n hunter-demo6-aks -g hunter-demo6-rg --ksm-metric-annotations-allow-list "namespace=default"
