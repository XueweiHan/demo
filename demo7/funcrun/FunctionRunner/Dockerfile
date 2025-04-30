# tag source: https://github.com/dotnet/dotnet-docker/blob/main/README.runtime.md
ARG dotnet_image_tag=8.0

FROM mcr.microsoft.com/dotnet/sdk:${dotnet_image_tag} AS build
ARG configuration=Release
WORKDIR /src
COPY . .
RUN dotnet publish *.csproj -c $configuration -o /publish



FROM mcr.microsoft.com/dotnet/runtime:${dotnet_image_tag}

ENV AzureWebJobsScriptRoot=/home/site/wwwroot
# ENV FuctionRunnerHttpPort=80

COPY --from=build /publish /app

CMD [ "/app/FunctionRunner" ]
