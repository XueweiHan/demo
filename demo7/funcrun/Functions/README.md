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

# local function image base container run
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

#-> Manually the Dockerfile_local_fb to use previous image as base image
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





    docker run -d --name linuxbox -v "$Env:USERPROFILE\.azure:/root/.azure" -v "$Env:USERPROFILE\.kube:/root/.kube" -v "c:\\repos:/src" -v "//var/run/docker.sock:/var/run/docker.sock" -p 7071:7071 -p 8000-8060:8000-8060 devlinux



mkdir -p bin/publish
dotnet publish azfunc.csproj --output bin/publish