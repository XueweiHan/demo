
# azure functions in AKS


# deploy storeage account
```
az login
az account set --subscription b47beaaf-7461-4b34-844a-7105d6b8c0d7
az configure --defaults location=eastus

az deployment group create -g hunter-demo6-rg --template-file azfunc.bicep
```
# Local func run
```

az role assignment create --role "Storage Account Contributor" --scope $(az storage account show -n hunterdemo6sa --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)

az role assignment create --role "Storage Blob Data Owner" --scope $(az storage account show -n hunterdemo6sa --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)

az role assignment create --role "Storage Queue Data Contributor" --scope $(az storage account show -n hunterdemo6sa --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv)

go build .

func start

if you are running in local box not in dev container, you can test url: http://localhost/api/hello
```
# Local container run
```
docker build -t xueweihan/demo6-func:0.1 .
docker run --rm -it -p 8083:80 xueweihan/demo6-func:0.1
device login...
test url: http://localhost:8083/api/hello
```
# deploy to azure function app
```
docker login
docker push xueweihan/demo6-func:0.1

?? restart func app with c

?? test url: https://demo3-fa1.azurewebsites.net/api/hello

```
# deploy to aks
```
az aks get-credentials -n hunter-demo6-aks -g hunter-demo6-rg

kubectl apply -f azfunc.yaml

curl https://demo6.ajzxhub.net/api/hello

```
# install keda ??
```
func kubernetes install --namespace default --dry-run >keda.install.yaml
replace `namespace: keda` to `namespace: default`
kubectl apply -f keda.install.yaml

```
# deploy func in aks

func kubernetes deploy --image-name xueweihan/demo3:app3 --name demo3aksfunc --dry-run >deploy.yaml
remove top few lines
kubectl apply -f deploy.yaml


# delete func in aks

func kubernetes delete --image-name xueweihan/demo3:app3 --name demo3aks
func kubernetes remove --namespace keda

az aks delete -n demo3-aks -g huntertest-rg