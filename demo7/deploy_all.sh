#!/bin/bash
set -ex

if [ -z "$name" ]; then
    name=hunter-demo
fi

loc=eastus

echo -e "\033[36mDeploy   = $name\033[0m"
echo -e "\033[36mLoaction = $loc\033[0m"

az configure --defaults location=$loc

az deployment sub create --template-file deployment/main.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"

az aks get-credentials -n $name-aks -g $name-rg --overwrite-existing

kubectl apply -f vn2.yaml

while ! kubectl get node hunter-virtualnode-0 >/dev/null 2>&1; do
    sleep 1
done
kubectl wait --for=condition=Ready node/hunter-virtualnode-0 --timeout=5m

identity_client_id=$(az identity show -g $name-rg -n $name-identity --query clientId -otsv)

sed "s/__IDENTITY_CLIENT_ID__/$identity_client_id/g" sa-template.yaml > sa.yaml

kubectl apply -f sa.yaml

kubectl apply -f ingress.yaml

./deploy_app.sh
