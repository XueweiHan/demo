# build code
FROM golang:1.22.0-alpine3.19@sha256:8e96e6cff6a388c2f70f5f662b64120941fcd7d4b89d62fec87520323a316bd9 AS build-env

WORKDIR /src
COPY . .
RUN go build -o ./release/func-app .
RUN mv ./host.json ./release/
RUN mv ./hello ./release/
RUN mv ./timer1 ./release/
RUN mv ./queue1 ./release/
RUN mv ./login.sh ./release/


# install az cli
FROM mcr.microsoft.com/azure-functions/dotnet:4 AS install-env

RUN apt install -y curl
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash


# FROM mcr.microsoft.com/azure-functions/dotnet:4-appservice 
FROM mcr.microsoft.com/azure-functions/dotnet:4

# copy az cli
COPY --from=install-env ["/usr/bin/az", "/usr/bin/az"]
COPY --from=install-env ["/opt/az", "/opt/az"]

# copy release
COPY --from=build-env ["/src/release", "/home/site/wwwroot"]

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    AzureWebJobsStorage__accountName=hunterdemo6sa
    # AzureFunctionsJobHost__functions__0="queue1" \
    # AzureFunctionsJobHost__functions__1="timer1" \
    # AzureFunctionsJobHost__functions__2="hello"

CMD ["/home/site/wwwroot/login.sh"]
# CMD ["/bin/bash", "-c", "sleep infinity"]

