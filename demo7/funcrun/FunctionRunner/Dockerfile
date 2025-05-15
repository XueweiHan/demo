# tag source: https://github.com/dotnet/dotnet-docker/blob/main/README.runtime.md
ARG dotnet_version=8.0



FROM mcr.microsoft.com/dotnet/sdk:${dotnet_version} AS build

ARG configuration=Release
WORKDIR /src
COPY . .

RUN dotnet publish *.csproj -c $configuration -o /publish



FROM mcr.microsoft.com/dotnet/runtime:${dotnet_version}

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    FunctionRunnerHttpPort=0

COPY --from=build /publish /app

CMD [ "/app/FunctionRunner" ]
