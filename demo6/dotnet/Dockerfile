# build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0@sha256:e5a27c7f2a5f679f994106aa330585a7108e2812a8367e7686c13451e7fd27a0 AS build-env

WORKDIR /src
COPY . .
RUN mv nuget/NuGet.config NuGet.config
RUN dotnet add package Newtonsoft.Json --version 13.0.3
RUN dotnet add package Azure.Messaging.EventGrid --version 4.23.0
RUN dotnet add package Microsoft.AzureAd.Icm.Types --version 3.0.2628.37
RUN dotnet add package Microsoft.AzureAd.Icm.WebService.Client --version 3.0.2628.37

RUN dotnet build IcmProxy.csproj --configuration Release



# final stage
FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine3.19@sha256:43acb80462b51d86cbec0d026d8b7598114c41faa3f79c05b898da9e344ca608

WORKDIR /app

COPY --from=build-env /src/bin/Release/net6.0/ ./

ARG TAG=0.0.0
ENV TAG=$TAG
ENV ROOT_URL=*:8082
EXPOSE 8001

CMD [ "dotnet", "IcmProxy.dll" ]
