# enable cluster log to write to log analytics workspace
```
# https://learn.microsoft.com/en-us/azure/azure-monitor/containers/kubernetes-monitoring-enable?tabs=cli#enable-container-insights
az deployment group create -g $name-rg --template-file log.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"

# config the agent
# https://learn.microsoft.com/en-us/azure/azure-monitor/containers/container-insights-data-collection-configure
kubectl apply -f configmap.yaml

```

# deploy ClusterRoleBinding, ServiceAccount, log writer, log follow
```
kubectl apply -f kubectl-logs-r.yaml
kubectl apply -f kubectl-logs-w-cc.yaml

```