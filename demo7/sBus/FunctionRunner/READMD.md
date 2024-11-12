# create our own function base image
```
export name=hunter-demo7

export acr=$(az acr show -n ${name/-/}cr --query loginServer -o tsv)
export ver=0.1

docker build -t $acr/function-runner:$ver .

az acr login -n $acr
docker push $acr/function-runner:$ver
```