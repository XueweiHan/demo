#!/bin/bash
set -ex

if [ "$APP_SETTINGS_FILE" ]; then
    mv "$APP_SETTINGS_FILE" appsettings.json
fi

dotnet aspweb.dll
