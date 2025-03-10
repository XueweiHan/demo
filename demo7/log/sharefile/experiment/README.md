

# log
```
az provider register --namespace Microsoft.OperationsManagement

az deployment group create -g $name-rg --template-file log.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"


```
