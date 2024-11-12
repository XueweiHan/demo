# our own base image for azure functions in AKS

# create azure function
```
func init --dotnet --docker
dotnet add package Microsoft.Azure.WebJobs.Extensions.ServiceBus
func new --name ServiceBusTrigger --template ServiceBusQueueTrigger

#-> add following value to local.settings.json in field "Values"
"ServiceBusConnection__fullyQualifiedNamespace": "hunter-demo7-test.servicebus.windows.net"

#-> change the queue name and connection string in ServiceBusTrigger.cs
ServiceBusTrigger("demo7queue", Connection = "ServiceBusConnection")
```

# local func run
```
func start

#-> send message from other console
dotnet .\QueueSender\bin\Debug\net6.0\QueueSender.dll
```

# local container run
```
#-> Manually update the servcie bus namespace name in all 4 Dockerfiles

# test local run the functions with function base image
docker build -t demo7-func-sb:debug -f Dockerfile_local .
docker run --rm -it demo7-func-sb:debug

# test local run the functions with our own base image
docker build -t demo7-func-fr-sb:debug -f Dockerfile_func_runner_local .
docker run --rm -it demo7-func-fr-sb:debug
```

# deploy to aks 
```
#-> add 'Azure Service Bus Data Receiver' role to the msi

export name=hunter-demo7
export acr=$(az acr show -n ${name/-/}cr --query loginServer -o tsv)
export ver=0.1
az acr login -n $acr

# deploy functions with function base image

docker build -t $acr/demo7-func-sb:$ver .
docker push $acr/demo7-func-sb:$ver

kubectl apply -f azfunc-sb.yaml

kubectl get all 
kubectl logs pod/azfunc-sb-dpl-84849bfd74-r785h --follow

# deploy functions with function runner base image
docker build -t $acr/demo7-func-sb-fr:$ver -f Dockerfile_func_runner .
docker push $acr/demo7-func-sb-fr:$ver

kubectl apply -f azfunc-sb-fr.yaml

kubectl get all 
kubectl logs pod/azfunc-sb-dpl-84849bfd74-r785h --follow









mkdir -p bin/publish
dotnet publish azfunc.csproj --output bin/publish