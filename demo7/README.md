# xpme azure portal login
PIM -> Manage -> Azure resource -> Subscriptions -> select -> Manage resource -> Activate -> Activate

```
docker start linuxbox
docker exec -it linuxbox /bin/bash
az login
az account set -s cd8a92db-a0c2-4273-816c-5149a1a1c5c9
az configure --defaults location=eastus
export name=hunter-demo7

az group create -g $name-rg

az deployment group create -g $name-rg --template-file demo7.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"
```

# assign roles to aks agentpool
```
# assign acr pull role

az aks update -n $name-aks -g $name-rg --attach-acr ${name/-/}cr
or
az role assignment create --assignee-object-id $(az identity show -n $name-aks-agentpool -g $name-aks-infra-rg --query principalId -o tsv) --role "AcrPull" --scope $(az acr show -n ${name/-/}cr --query id -o tsv) --assignee-principal-type ServicePrincipal

# assign resource group contributor role for creating ACI CGs.

az role assignment create --assignee-object-id $(az identity show -n $name-aks-agentpool -g $name-aks-infra-rg --query principalId -o tsv) --role "Contributor" --scope $(az group show -g $name-rg --query id -o tsv) --assignee-principal-type ServicePrincipal

az role assignment create --assignee-object-id $(az identity show -n $name-aks-agentpool -g $name-aks-infra-rg --query principalId -o tsv) --role "Contributor" --scope $(az group show -g $name-aks-infra-rg --query id -o tsv) --assignee-principal-type ServicePrincipal
```

# install vn2 app
```
az aks get-credentials -n $name-aks -g $name-rg
az provider register -n Microsoft.ContainerInstance
# https://github.com/azure-core-compute/VirtualNodesOnACI-1P
helm install $name vn2-helm

# check the vn2 running
kubectl get nodes
```

# deploy service account to aks
```
# get clientId of the user assigned identity
az identity show -g $name-rg -n $name-identity --query clientId -o tsv
#-> manually update the workload identity in sa.yaml file

# kubelogin by default will use the kubeconfig from ${KUBECONFIG}.
# Specify --kubeconfig to override this converts to use azurecli login mode
kubelogin convert-kubeconfig -l azurecli

kubectl apply -f sa.yaml
```

# docker build 
```
export acr=$(az acr show -n ${name/-/}cr --query loginServer -o tsv)
export ver=0.1

docker build -t $acr/$name:$ver .

docker run --rm -p 8081:8081 $acr/${name}:$ver
#-> test http://localhost:8081/https:/www.google.com/search?q=hello in browser

az acr login -n $acr

docker push $acr/$name:$ver
```

# confidential container deployment
```
#-> maually update the image name in the demo7.yaml to following name
echo $acr/$name:$ver

# create cc policy
./ccpolicy.sh demo7.yaml

kubectl apply -f demo7-cc.yaml

# check pods
kubectl get pods
kubectl describe pod <pod-name>

# verify pod is confidential
./ccpod.sh $name-aks-infra-rg <pod-id>
```

# ingress & app is running
```
kubectl apply -f ingress.yaml

# get you ingress service external ip
ip=$(kubectl get service -n app-routing-system nginx -o jsonpath="{.status.loadBalancer.ingress[0].ip}")

curl http://$ip/hello/world?name=hunter

echo $ip
#-> use browser to visit http://<external-ip>/https:/www.google.com/search?q=hello
```

# verify the egress ip
```
# show expected egress ip address
az network public-ip show -n $name-public-ip -g $name-rg --query ipAddress -o tsv

curl http://$ip/http:/checkip.dyndns.org
```

# verify the workload identity works
```
# assign role to yourself to create a secret into the kevvault for testing

az role assignment create --role "Key Vault Secrets Officer" --scope $(az keyvault show -n $name-keyvault --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv) --assignee-principal-type User

az keyvault secret set --vault-name $name-keyvault -n testkey --value "hello world!"

curl http://$ip/?vault=hunter-demo7-keyvault&secret=testkey
```

# Other Tests
```
https -> https ingress + cert-manager + let's encrypt
dotnet -> .NET linux container

```







# attestation?
```
az attestation show -n hunter${name}attest -g hunter-$name-rg --query attestUri -o tsv
https://hunterdemo6attest.eus.attest.azure.net
update the key-release-policy.json

kubectl apply -f demo6.yaml
```


# ingress
```

kubectl apply -f demo6-1.yaml
kubectl apply -f ingress.yaml

kubectl get ingress
update dns record

```

# https 3 (aks + nginx + cert-manager + let's encrypt)
```

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.14.4/cert-manager.yaml
kubectl get pods -n cert-manager
kubectl apply -f issuer.yaml
kubectl describe secret letsencrypt-key
```

# test key vault
```
az role assignment create --role "Key Vault Secrets Officer" --scope $(az keyvault show -n hunter-$name-keyvault --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)
az keyvault secret set --vault-name hunter-$name-keyvault -n test1 --value "hello world!"

test:
https://demo6.ajzxhub.net/demo6-2?vault=hunter-demo6-keyvault&secret=test1
https://demo6.ajzxhub.net/demo6-1?vault=hunter-demo6-keyvault&secret=test1
https://demo6.ajzxhub.net/pods
https://demo6.ajzxhub.net/https/www.google.com/search?q=hello



az role assignment create --role "Key Vault Crypto Officer" --scope $(az keyvault show -n hunter-$name-keyvault --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)

az keyvault key create -n key1 --vault-name hunter-$name-keyvault --ops wrapKey unwrapkey encrypt decrypt --kty RSA-HSM --size 3072 --exportable --policy key-release-policy.json

az keyvault key download -n key1 --vault-name hunter-$name-keyvault -f pub.pem

go run encode/encode.go pub.pem "Top secret!"

test:
https://demo6.ajzxhub.net/demo6?vault=hunter-demo6-keyvault&attest=hunterdemo6attest.eus&key=key1&message=

```




# ---------------------------------------------------------------------------------------------
# self-signed tls for testing

# https 1 (self-signed tls cert)
```
openssl req -new -x509 -nodes -out crt.pem -keyout key.pem -subj "/CN=test.ajzxhub.net" -addext "subjectAltName=DNS:test.ajzxhub.net"

openssl pkcs12 -export -in crt.pem -inkey key.pem -out tls.pfx

az keyvault certificate import --vault-name hunter-demo6-keyvault -n tls -f tls.pfx

az keyvault certificate show --vault-name hunter-demo6-keyvault -n tls --query id -o tsv
https://hunter-demo6-keyvault.vault.azure.net/certificates/tls/7aad9353dd9041a0820318a237ce0695

az keyvault show --name hunter-demo6-keyvault --query id -o tsv
/subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/hunter-demo6-rg/providers/Microsoft.KeyVault/vaults/hunter-demo6-keyvault

az aks approuting update -n hunter-demo6-aks -g hunter-demo6-rg --enable-kv --attach-kv /subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/hunter-demo6-rg/providers/Microsoft.KeyVault/vaults/hunter-demo6-keyvault

az network dns zone create -g hunter-demo6-rg -n demo6.ajzxhub.net

az network dns zone show -g hunter-demo6-rg -n demo6.ajzxhub.net --query id -o tsv
/subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/hunter-demo6-rg/providers/Microsoft.Network/dnszones/demo6.ajzxhub.net

az aks approuting zone add -g hunter-demo6-rg -n hunter-demo6-aks --attach-zones --ids=/subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/hunter-demo6-rg/providers/Microsoft.Network/dnszones/demo6.ajzxhub.net


```
# https 2 (self-signed tls cert)

```
kubectl create secret tls http-tls --key key.pem --cert crt.pem

ingress spec:
  tls:
  - secretName: http-tls
    hosts:
    - test.ajzxhub.net
    - demo6.ajzxhub.net
```
  




# --- deprecated -------------------------------------------------- 
az group create -g test1-rg
az aks create -g test1-rg -n test1-aks --enable-app-routing --node-vm-size standard_b2pls_v2 --os-sku AzureLinux --node-count 1 --node-resource-group test1-aks-infra
az aks get-credentials -n test1-aks -g test1-rg


# application gateway for AKS



az login
az account set --subscription b47beaaf-7461-4b34-844a-7105d6b8c0d7
az configure --defaults location=eastus

az keyvault purge -n demo6-dev-keyvault

az group create -g hunter-demo6-rg
az deployment group create -g hunter-demo6-rg --template-file demo6.bicep
//az deployment group create -g demo6-dev-aks-infra --template-file demo6_module.bicep


az aks show -n test-aks -g test-rg -o tsv --query "addonProfiles.ingressApplicationGateway.identity.clientId"




az aks get-credentials -n demo6-dev-aks -g hunter-demo6-rg


kubectl apply -f demo6.yaml
----------------------------------------------------------------
# create applcation gateway with aks ( with az cli )


az login
az configure --defaults location=westus3


# az group create -g test-rg

az aks create -n test-aks -g test-rg --network-plugin azure --enable-managed-identity -a ingress-appgw --appgw-name  test-app-gateway --appgw-subnet-cidr "10.225.0.0/16" --generate-ssh-keys --node-vm-size standard_b2pls_v2 --os-sku AzureLinux --node-count 1 --node-resource-group test-aks-infra


--appgw-watch-namespace test-aks-infra


# Get application gateway id from AKS addon profile
az aks show -n test-aks -g test-rg -o tsv --query "addonProfiles.ingressApplicationGateway.config.effectiveApplicationGatewayId"

/subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/test-aks-infra/providers/Microsoft.Network/applicationGateways/test-app-gateway

# Get Application Gateway subnet id
az network application-gateway show --ids /subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/test-aks-infra/providers/Microsoft.Network/applicationGateways/test-app-gateway -o tsv --query "gatewayIPConfigurations[0].subnet.id"
or
az network application-gateway show -n test-app-gateway -g test-aks-infra -o tsv --query "gatewayIPConfigurations[0].subnet.id"

/subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/test-aks-infra/providers/Microsoft.Network/virtualNetworks/aks-vnet-34430404/subnets/test-app-gateway-subnet

# Get AGIC addon identity
az aks show -n test-aks -g test-rg -o tsv --query "addonProfiles.ingressApplicationGateway.identity.clientId"

3cc71cb8-8eec-4789-9bff-f241c5c03e82



# Assign network contributor role to AGIC addon identity to subnet that contains the Application Gateway
az role assignment create --assignee 3cc71cb8-8eec-4789-9bff-f241c5c03e82 --scope /subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/test-aks-infra/providers/Microsoft.Network/virtualNetworks/aks-vnet-34430404/subnets/test-app-gateway-subnet --role "Network Contributor"


# all-in-one to set the network contributor role to the AGIC addon identity to subnet
az role assignment create --assignee $(az aks show -n demo6-dev-aks -g hunter-demo6-rg -o tsv --query "addonProfiles.ingressApplicationGateway.identity.clientId") --scope $(az network application-gateway show --ids $(az aks show -n demo6-dev-aks -g hunter-demo6-rg -o tsv --query "addonProfiles.ingressApplicationGateway.config.effectiveApplicationGatewayId") -o tsv --query "gatewayIPConfigurations[0].subnet.id") --role "Network Contributor"



az aks get-credentials -n test-aks -g test-rg





---------------------------------------------------------------
# create application gate way separately ( not working for Torus Playground ) 

az aks create -n test-aks -g test-rg --enable-managed-identity --generate-ssh-keys --node-vm-size standard_b2pls_v2 --os-sku AzureLinux --node-count 1


az aks get-credentials -n test-aks -g test-rg


az network public-ip create -n test-public-ip -g test-rg --allocation-method Static --sku Standard


az network vnet create -n test-vnet -g test-rg --address-prefix 10.0.0.0/16 --subnet-name test-subnet --subnet-prefix 10.0.0.0/24 


az network application-gateway create -n test-app-gateway -g test-rg --sku Standard_v2 --public-ip-address test-public-ip --vnet-name test-vnet --subnet test-subnet --priority 100


az network application-gateway show -n test-app-gateway -g test-rg --query "id" -o tsv

/subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/test-rg/providers/Microsoft.Network/applicationGateways/test-app-gateway


az aks enable-addons -n test-aks -g test-rg -a ingress-appgw --appgw-id /subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/test-rg/providers/Microsoft.Network/applicationGateways/test-app-gateway


# vnet peer from appGW to aksVnet
az aks show -n test-aks -g test-rg --query "nodeResourceGroup" -o tsv
MC_test-rg_test-aks_westus3

az network vnet list -g MC_test-rg_test-aks_westus3 -o tsv --query "[0].name"
aks-vnet-34430404

az network vnet show -n aks-vnet-34430404 -g MC_test-rg_test-aks_westus3 -o tsv --query "id"
/subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/MC_test-rg_test-aks_westus3/providers/Microsoft.Network/virtualNetworks/aks-vnet-34430404

az network vnet peering create -n test-AppGWtoAKSVnetPeering -g test-rg --vnet-name test-vnet --remote-vnet /subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/MC_test-rg_test-aks_westus3/providers/Microsoft.Network/virtualNetworks/aks-vnet-34430404 --allow-vnet-access

# vnet peer from aksVnet to appGW
az network vnet show -n test-vnet -g test-rg -o tsv --query "id"
/subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/test-rg/providers/Microsoft.Network/virtualNetworks/test-vnet

az network vnet peering create -n test-AKStoAppGWVnetPeering -g MC_test-rg_test-aks_westus3 --vnet-name aks-vnet-34430404 --remote-vnet /subscriptions/10e43d30-2be8-42eb-8fef-b334040d4cc0/resourceGroups/test-rg/providers/Microsoft.Network/virtualNetworks/test-vnet --allow-vnet-access
