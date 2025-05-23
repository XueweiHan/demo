# metrics 
```
az login
az account set --subscription b47beaaf-7461-4b34-844a-7105d6b8c0d7
az configure --defaults location=eastus

az aks get-credentials -n hunter-demo6-aks -g hunter-demo6-rg

kubectl get configmap -n kube-system
kubectl apply -f ama-metrics-settings-configmap.yaml
```

# deploy azure resource ( or just use the policy)
```
az deployment group create -g hunter-demo6-rg --template-file monitor.bicep

az role assignment create --role "Grafana Admin" --scope $(az grafana show -n hunter-demo6-grafana -g hunter-demo6-rg --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)

```

# shared grafana role
```
az role assignment create --role "Grafana Admin" --scope $(az grafana show -n aks-grafana -g aks-visibility-rg --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv) --assignee-principal-type User
```

# metrics policy (with this policy, we can skip the monitor.bicep deployment)
```

https://github.com/Azure/prometheus-collector/tree/main/AddonPolicyTemplate

az policy definition create -n aks-prometheus-metrics-addon --display-name "AKS Prometheus Metrics Addon" -m Indexed --metadata version=1.0.0 category=Kubernetes --rules ./AddonPolicyMetricsProfile.rules.json --params ./AddonPolicyMetricsProfile.parameters.json

az deployment sub create --template-file metricsPolicyAssign.bicep --parameters "azureMonitorWorkspaceResourceId=$(az monitor account show -n AKS-Monitor-Workspace -g AKS-Visibility-rg --query id -o tsv)" --parameters enableWindowsRecordingRules=false --parameters "policyDefinitionId=$(az policy definition show -n aks-prometheus-metrics-addon --query id -o tsv)"

```
# log policy (with this policy, we can skip the log.bicep deployment)
```

az policy definition create -n aks-log-addon --display-name "AKS Log Addon" -m Indexed --metadata version=1.0.0 category=Kubernetes --rules ./AddonPolicyLogProfile.rules.json --params ./AddonPolicyLogProfile.parameters.json

az deployment sub create --template-file logPolicyAssign.bicep --parameters "logAnalyticsWorkspaceResourceId=$(az monitor log-analytics workspace show -n AKS-Log-Analytics-Workspace -g AKS-Visibility-rg --query id -o tsv)" --parameters "policyDefinitionId=$(az policy definition show -n aks-log-addon --query id -o tsv)"


```

# AzSecPack addon policy
```
az policy definition create -n vmss-azsec-pack-addon --display-name "AzSecPack VMSS Extension Addon" -m Indexed --metadata version=1.0.0 category=Compute --rules ./AzSecPackExtensionPolicyProfile.rules.json

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
