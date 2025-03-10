# secret storage csi driver
```
# enable secrets storage csi driver for the cluster
az deployment group create -g $name-rg --template-file csi.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"


# show the key vault secret provider client Id
az aks show -n $name-aks -g $name-rg --query addonProfiles.azureKeyvaultSecretsProvider.identity.clientId -o tsv


# show work-load identity client id
az identity show -n $name-identity -g $name-rg --query 'clientId' -o tsv

# show aks tenant id
az aks show -n $name-aks -g $name-rg --query identity.tenantId -o tsv


# assign self the 'Key Vault Administrator' role on portal
# create a test key
az keyvault key create -n testKey1 --vault-name $name-keyvault

# download the key
az keyvault key download -n testKey1 --vault-name $name-keyvault -f a.pem


# update the csi.yaml (clientID, keyvalutName, tenantId, objects)
# deploy csi.yaml for testing

kubectl apply -f csi.yaml

kubectl exec busybox-secrets-store-inline-wi -- cat /mnt/secrets-store/testKey1

kubectl exec busybox-secrets-store-inline-wi -- cat /mnt/secrets-store/testsecret1

# show k8s secret values
kubectl get secrets/keyvault-csi-secrets --template={{.data.testKey1}} | base64 -d
kubectl get secrets/keyvault-csi-secrets --template={{.data.testsecret1}} | base64 -d

```
