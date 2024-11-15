# Create our own function base image
```
#linux:
name=hunter-demo7
acr=${name/-/}cr.azurecr.io
ver=0.1
#windows:
$name='hunter-demo7'
$acr="$($name.replace('-',''))cr.azurecr.io"
$ver=0.1

docker build -t $acr/function-runner:$ver .

az acr login -n $acr
docker push $acr/function-runner:$ver
```