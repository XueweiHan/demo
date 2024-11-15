# our own base image for azure functions in AKS

# create azure function
```
func init --dotnet --docker
dotnet add package Microsoft.Azure.WebJobs.Extensions.ServiceBus
func new --name ServiceBusTrigger1 --template ServiceBusQueueTrigger
func new --name TimerTrigger1 --template TimerTrigger

#-> add following value to local.settings.json in field "Values"
"ServiceBusConnection1__fullyQualifiedNamespace": "hunter-demo7-service-bus.servicebus.windows.net"

#-> change the queue name and connection string in ServiceBusTrigger1.cs
ServiceBusTrigger("queue1", Connection = "ServiceBusConnection1")
```

# local func run
```
func start

#-> send message from other console
dotnet .\QueueSender\bin\Debug\net6.0\QueueSender.dll
```

# local function runner
```
#-> Manually update the FunctionRunner/Properties/launchSettings.json
#-> Build FunctionRunner.sln
#-> Run FunctionRunner in VS
```

# local function base container run
```
linux:
export name=hunter-demo7
export acr=${name/-/}cr.azurecr.io
export ver=0.1
windows:
$name='hunter-demo7'
$acr="$($name.replace('-',''))cr.azurecr.io"
$ver=0.1

#-> Manually update the servcie bus namespace name in all 2 non-local Dockerfiles
docker build -t $acr/demo7-func-fb:$ver -f Dockerfile_fb .

#-> Manually update the Dockerfile_local_fb to use previous image as base image
docker build -t demo7-func-fb -f Dockerfile_local_fb .

docker run --rm -it demo7-func-fb
```

# local function runner container run
```
docker build -t $acr/demo7-func-fr:$ver -f Dockerfile_fr .
#-> Manually the Dockerfile_local_fr to use previous image as base image
docker build -t demo7-func-fr -f Dockerfile_local_fr .

docker run --rm -it -v "$Env:USERPROFILE/.azure:/root/.azure" demo7-func-fr
```

# deploy to aks 
```
#-> add 'Azure Service Bus Data Receiver' role to the msi

export name=hunter-demo7
export acr=${name/-/}cr.azurecr.io
export ver=0.1
az acr login -n $acr

# deploy functions with function base image

docker push $acr/demo7-func-fb:$ver

kubectl apply -f azfunc-fb.yaml

kubectl get all 
kubectl logs pod/azfunc-fb-dpl-6ff8db68b9-4n4jp --follow

# deploy functions with function runner image

docker push $acr/demo7-func-fr:$ver

kubectl apply -f azfunc-fr.yaml

kubectl get all 
kubectl logs pod/azfunc-fr-dpl-58cdcf44-s7q5v --follow
```