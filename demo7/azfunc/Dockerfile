# build code
FROM golang:1.22.0-alpine3.19@sha256:8e96e6cff6a388c2f70f5f662b64120941fcd7d4b89d62fec87520323a316bd9 AS build-env

WORKDIR /src
COPY . .
RUN go build -o ./release/azfunc .
RUN mv ./host.json ./release/
RUN mv ./hello ./release/
RUN mv ./timer1 ./release/
RUN mv ./queue1 ./release/


# FROM mcr.microsoft.com/azure-functions/dotnet:4-appservice 
FROM mcr.microsoft.com/azure-functions/dotnet:4

# copy release
COPY --from=build-env ["/src/release", "/home/site/wwwroot"]

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
