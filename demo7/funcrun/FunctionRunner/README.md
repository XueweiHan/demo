# Function Runner

## Usage

1. Copy the appropriate `.cs` file from the `examples` folder into your Azure Function project.
2. For Azure Functions in-process mode, use `FunctionRunnerBuilderExtensions.cs`.
3. For Azure Functions isolated mode, use `FunctionRunnerHostExtensions.cs`.
4. Update the file's namespace to match your application's namespace.


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
