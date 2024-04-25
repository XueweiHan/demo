# nginx for AKS
```
az login
az account set --subscription b47beaaf-7461-4b34-844a-7105d6b8c0d7
az configure --defaults location=eastus

az group create -g hunter-demo6-rg
az deployment group create -g hunter-demo6-rg --template-file demo6.bicep
az identity show -g hunter-demo6-rg -n hunter-demo6-identity --query 'clientId' -o tsv
update the workload identity id in demo6.yaml file
```


# docker build 
```
docker build -t xueweihan/demo6:0.1 .
docker run --rm -p 8081:8081 xueweihan/demo6:0.1
test http://localhost:8081/https/www.google.com/search?q=hello in browser
docker login
docker push xueweihan/demo6:0.1
```


# confcom
```
az confcom katapolicygen -y demo6.yaml --print-policy | base64 -d | sha256sum | cut -d' ' -f1
az attestation show -n hunterdemo6attest -g hunter-demo6-rg --query attestUri -o tsv
https://hunterdemo6attest.eus.attest.azure.net
update the key-release-policy.json

az aks get-credentials -n hunter-demo6-aks -g hunter-demo6-rg
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
az role assignment create --role "Key Vault Secrets Officer" --scope $(az keyvault show -n hunter-demo6-keyvault --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)
az keyvault secret set --vault-name hunter-demo6-keyvault -n test1 --value "hello world!"

test:
https://demo6.ajzxhub.net/demo6-2?vault=hunter-demo6-keyvault&secret=test1
https://demo6.ajzxhub.net/demo6-1?vault=hunter-demo6-keyvault&secret=test1
https://demo6.ajzxhub.net/pods
https://demo6.ajzxhub.net/https/www.google.com/search?q=hello
http://4.157.235.68/helloworld?vault=hunter-demo6-keyvault&secret=test1
http://4.157.235.68/pods
http://4.157.235.68/https/www.google.com/search?q=hello


az role assignment create --role "Key Vault Crypto Officer" --scope $(az keyvault show -n hunter-demo6-keyvault --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)
az keyvault key create -n key1 --vault-name hunter-demo6-keyvault --ops wrapKey unwrapkey encrypt decrypt --kty RSA-HSM --size 3072 --exportable --policy key-release-policy.json

az keyvault key download -n key1 --vault-name hunter-demo6-keyvault -f pub.pem

go run encode/encode.go pub.pem "Top secret!"

test:
https://demo6.ajzxhub.net/demo6-2?vault=hunter-demo6-keyvault&attest=hunterdemo6attest.eus&key=key1&message=

```




# ---------------------------------------
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
