# Create our own function base image
```
#linux:
acr=huntersharedcr.azurecr.io
ver=8.0-alpine

#windows:
$acr="huntersharedcr.azurecr.io"
$ver="8.0-alpine"

docker build --build-arg dotnet_image_tag=$ver -t $acr/function-runner:$ver .

az acr login -n $acr
docker push $acr/function-runner:$ver
```

//<PublishSingleFile>true</PublishSingleFile>
//<SelfContained>true</SelfContained>
