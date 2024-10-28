#!/bin/bash

if [[ -v AZURE_CLIENT_ID ]]; then
    az login --federated-token "$(cat $AZURE_FEDERATED_TOKEN_FILE)" --service-principal -u $AZURE_CLIENT_ID -t $AZURE_TENANT_ID
else
    az login --use-device-code
fi

# Check if the login was successful
if [ $? -eq 0 ]; then
    echo "Azure login successful"
else
    echo "Azure login failed"
    exit 1
fi

# Start the Azure Function service
/opt/startup/start_nonappservice.sh
