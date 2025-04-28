# deploy istio ingress to aks

name=hunter-demo2

az deployment group create -g $name-rg --template-file istio-ingress.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"



Tutorial: Containerize a .NET app
https://learn.microsoft.com/en-us/dotnet/core/docker/build-container
Run an ASP.NET Core app in Docker containers
https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images

dotnet new webapp -o aspweb
cd aspweb
dotnet run


#-> Use VSCode command : "Docker: Add Docker Files to Workspace..." -> ".NET: ASP.NET Core" -> aspweb.csproj -> Linux -> 5080 
#-> Manully clean up the Dockerfile
or
#-> Use VisualStudio right click the project -> Add -> Container Support...


docker build -t ptmsdev/aspweb .
docker push ptmsdev/aspweb

# local run http
docker run --rm -it -p80:80 ptmsdev/aspweb

browser http://localhost/


# add simple JSON api in Program.cs
# add extension: REST Client
# add API_test.http

Hosting ASP.NET Core images with Docker over HTTPS
https://learn.microsoft.com/en-us/aspnet/core/security/docker-https

# generate localhost aspweb.pfx
mkdir -p $env:USERPROFILE/.aspnet/https
dotnet dev-certs https -ep $env:USERPROFILE/.aspnet/https/aspweb.pfx -p password
dotnet dev-certs https --trust


# local run https
docker run --rm -it -p80:80 -p443:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_Kestrel__Certificates__Default__Password=password -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspweb.pfx -v $env:USERPROFILE\.aspnet\https:/https/ ptmsdev/aspweb


# convert to crt & key
openssl pkcs12 -in $env:USERPROFILE/.aspnet/https/aspweb.pfx -nocerts -nodes -out cert.key
openssl pkcs12 -in $env:USERPROFILE/.aspnet/https/aspweb.pfx -clcerts -nokeys -out cert.crt


https://istio.io/latest/docs/tasks/traffic-management/ingress/ingress-sni-passthrough/

# az dns record (type a)

```bash

ingress_name=aks-istio-ingressgateway-external
ingress_ns=aks-istio-ingress

kubectl get svc $ingress_name -n $ingress_ns

# Get the external IP PORTS of the Istio ingress gateway
external_ip=$(kubectl get svc $ingress_name -n $ingress_ns -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

http_port=$(kubectl -n $ingress_ns get service $ingress_name -o jsonpath='{.spec.ports[?(@.name=="http2")].port}')

https_port=$(kubectl -n $ingress_ns get service $ingress_name -o jsonpath='{.spec.ports[?(@.name=="https")].port}')

echo $external_ip $http_port $https_port

# Add dns record for aspweb.hunterapp.net
az network dns record-set a add-record -g hunter-poc -z hunterapp.net -n aspweb -a $external_ip

# Create a root CA cert & key
openssl req -x509 -sha256 -nodes -days 365 -newkey rsa:2048 -subj '/O=Hunter App Inc./CN=hunterapp.net' -keyout hunterapp.net.key -out hunterapp.net.crt

# Create aspweb cert & key
openssl req -out aspweb.hunterapp.net.csr -newkey rsa:2048 -nodes -keyout aspweb.hunterapp.net.key -subj "/CN=aspweb.hunterapp.net/O=AspWeb Team"

# Sign the cert 
openssl x509 -req -sha256 -days 365 -CA hunterapp.net.crt -CAkey hunterapp.net.key -set_serial 0 -in aspweb.hunterapp.net.csr -out aspweb.hunterapp.net.crt

```

# aks run https
kubectl apply -f aspweb.yaml

kubectl port-forward svc/aspweb-svc 80:80 443:443




https://istio.io/latest/docs/tasks/traffic-management/ingress/ingress-sni-passthrough/

kubectl apply -f gateway.yaml

browser: https://aspweb.hunterapp.net/

curl -v --resolve "aspweb.hunterapp.net:$https_port:$external_ip" --cacert hunterapp.net.crt "https://aspweb.hunterapp.net:$https_port/hello"

or

curl -L -v --cacert hunterapp.net.crt "http://aspweb.hunterapp.net/hello"



reference:
appsettings.json / Kestrel
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints

Hosting ASP.NET Core images with Docker over HTTPS
