#!/bin/bash
set -ex

name=hunter-demo1
loc=eastus

echo -e "\033[36mDeploy   = $name\033[0m"
echo -e "\033[36mLoaction = $loc\033[0m"

az configure --defaults location=$loc

# https://learn.microsoft.com/en-us/azure/azure-monitor/containers/kubernetes-monitoring-enable?tabs=cli#enable-container-insights
az deployment group create -g $name-rg --template-file log.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"

# https://learn.microsoft.com/en-us/azure/azure-monitor/containers/container-insights-data-collection-configure
curl -sL https://raw.githubusercontent.com/microsoft/Docker-Provider/ci_prod/kubernetes/container-azm-ms-agentconfig.yaml > configmap.yaml

kubectl apply -f configmap.yaml

../ccpolicy.sh kubectl-logs.yaml

kubectl apply -f kubectl-logs-cc.yaml

#/subscriptions/cd8a92db-a0c2-4273-816c-5149a1a1c5c9/resourcegroups/hunter-demo-rg/providers/Microsoft.ContainerService/managedClusters/hunter-demo-aks
#/subscriptions/cd8a92db-a0c2-4273-816c-5149a1a1c5c9/resourceGroups/hunter-demo-rg/providers/Microsoft.ContainerService/managedClusters/hunter-demo-aks
