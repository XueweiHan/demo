
# azure functions in AKS


# deploy storeage account
```
az deployment group create -g $name-rg --template-file azfunc.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"
```
# Local func run
```
storage=${name/-/}sa

az role assignment create --role "Storage Account Contributor" --scope $(az storage account show -n $storage --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv) --assignee-principal-type User

az role assignment create --role "Storage Blob Data Owner" --scope $(az storage account show -n $storage --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv) --assignee-principal-type User

az role assignment create --role "Storage Queue Data Contributor" --scope $(az storage account show -n $storage --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv) --assignee-principal-type User

#-> Manually update the storage account name in local.settings.json
echo $storage

go build .

func start

curl localhost:7071/api/hello

```
# Local container run
```
#-> Manually update the storage account name in Dockerfile_local
echo $storage

docker build -t demo7-func:debug -f Dockerfile_local .
docker run --rm -it -p 8083:80 demo7-func:debug
#-> device login...
#-> browser http://localhost:8083/api/hello
```

# deploy to aks
```
export name=hunter-demo7
export acr=${name/-/}cr.azurecr.io
export ver=0.1

docker build -t $acr/demo7-func:$ver .
az acr login -n $acr
docker push $acr/demo7-func:$ver

#-> update the image name in the azfunc.yaml to following name
echo $acr/demo7-func:$ver

#-> update the storage account in the azfunc.yaml
echo $storage

../ccpolicy.sh azfunc.yaml

kubectl apply -f azfunc-cc.yaml



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