#!/bin/bash
set -ex

export kv=hunter-shared-keyvault
export acr=huntersharedcr.azurecr.io
export ver=0.1

docker build -t $acr/$name:$ver .

az acr login -n $acr

docker push $acr/$name:$ver
image=$acr/$name:$ver

sed "s|__IMAGE__|$image|g" demo-template.yaml > demo.yaml

./ccpolicy.sh demo.yaml

kubectl apply -f demo-cc.yaml

kubectl get pod

kubectl wait --for=condition=ready pod -l app=demo --timeout=5m

# ip=$(kubectl get service -n app-routing-system nginx -o jsonpath="{.status.loadBalancer.ingress[0].ip}")
export ip=$(kubectl get ingress demo-ing -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

az role assignment create --role "Key Vault Secrets Officer" --scope $(az keyvault show -n $kv --query id -otsv) --assignee-object-id $(az ad signed-in-user show --query id -otsv) --assignee-principal-type User

az keyvault secret set --vault-name $kv -n testkey --value "Hello world!"

curl -L http://$ip/https://api.ipify.org

# curl http://$ip/pods

curl http://$ip/hello/world?name=hunter

curl http://$ip/?vault=$kv\&secret=testkey
