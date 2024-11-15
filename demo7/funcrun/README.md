# Create service bus
```
$name='hunter-demo7'

linux:
az deployment group create -g $name-rg --template-file sbus.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"

windows:
az deployment group create -g $name-rg --template-file sbus.bicep --parameters "{'project_name':{'value':'$name'}}"

# assign yourself the 'Azure Service Bus Data Owner' role
az role assignment create --role "Azure Service Bus Data Owner" --scope $(az servicebus namespace show -n $name-service-bus -g $name-rg --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv) --assignee-principal-type User


```
# Build project and test queue
```
#-> open *.sln file Visual Studio and build

#-> open two console window

.\MessageReceiver\bin\Debug\net6.0\MessageReceiver.exe $name-service-bus queue1

```

# Build helper az-cli image for local testing
```
docker build -t az-cli .
```


# Continue in the folder FunctionRunner
# Continue in the folder azfunc
