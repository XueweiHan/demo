# <deprecated> - switch to kubectl-logs solution

# deploy pvc, log writer, log reader 
```
kubectl apply -f sharelog.yaml
```


# deprecated steps
```
# create file share in existing storage account (azfunc created)
az provider register --namespace Microsoft.OperationsManagement
az deployment group create -g $name-rg --template-file fileshare.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"


# deploy testlog.yaml
kubectl apply -f testlog.yaml

```