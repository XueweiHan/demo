#!/bin/bash
set -e

if [ -z "$name" ]; then
  name="hunter-test"
fi


az deployment sub create --template-file demo-aks-rg.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"

node_rg=$name"-aks-infra-rg"
while ! az group exists --name "$node_rg"; do
  echo "Waiting for resource group $node_rg to be created..."
  sleep 5
done

az deployment sub create --template-file demo-aks-node-rg.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"
