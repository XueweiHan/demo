# build stage
FROM golang:1.22.0-alpine3.19@sha256:8e96e6cff6a388c2f70f5f662b64120941fcd7d4b89d62fec87520323a316bd9 AS build-env

WORKDIR /src
COPY . .
RUN go build -o ./release/func-app .
RUN cp ./host.json ./release/
RUN cp -r ./hello ./release/
RUN cp -r ./timer1 ./release/
RUN cp -r ./queue1 ./release/



# FROM mcr.microsoft.com/azure-functions/dotnet:4-appservice 
FROM mcr.microsoft.com/azure-functions/dotnet:4

ARG KEY
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    AzureWebJobsStorage="DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=hunterdemo6sa;AccountKey=${KEY}"
    # AzureFunctionsJobHost__functions__0="queue1" \
    # AzureFunctionsJobHost__functions__1="timer1" \
    # AzureFunctionsJobHost__functions__2="hello"

COPY --from=build-env ["/src/release", "/home/site/wwwroot"]
