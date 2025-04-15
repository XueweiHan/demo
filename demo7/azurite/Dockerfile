#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet8.0 AS base

################################################################################
# build stage
FROM base AS build

# create the startup script
COPY <<'EOF' /start.sh
#!/bin/bash

heartbeat() {
    while true; do
        echo "$(date +'%Y-%m-%d %H:%M:%S') pod $HOSTNAME heartbeat"
        sleep 60
    done
}

wait_for_azurite() {
    # wait for azurite to accept connections on port 10000
    while ! $(nc -z localhost 10000); do
        echo "$(date +'%Y-%m-%d %H:%M:%S') Waiting on Azurite to become ready..."
        sleep 1
    done
}

wait_for_azurite

heartbeat &

echo "$(date +'%Y-%m-%d %H:%M:%S') Azurite is ready, starting the Azure Function"
/opt/startup/start_nonappservice.sh

EOF
RUN chmod +x /start.sh

################################################################################
# final stage
FROM base AS final
WORKDIR /home/site/wwwroot
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# install nc command
RUN apt-get update && apt-get install -y netcat-openbsd

# copy service binaries
COPY . .

# copy the startup script from the build stage
COPY --from=build /start.sh /start.sh

CMD [ "/start.sh" ]
