#!/bin/bash
set -e

# Get the YAML file name
yaml_file="$1"
cc_yaml_file="${yaml_file%.*}-cc.${yaml_file##*.}"
cp -f "$yaml_file" "$cc_yaml_file"

echo -e "\033[36mUpdating empty values in the YAML file...\033[0m"
sed -i 's/^\(\s*value:\s\)$/\1""/g' "$cc_yaml_file"

echo -e "\033[36mGenerating the virtual node confidential container policy...\033[0m"
# az confcom acipolicygen --virtual-node-yaml "$cc_yaml_file" --print-policy | base64 -d | sha256sum | cut -d' ' -f1
az confcom acipolicygen --virtual-node-yaml "$cc_yaml_file"

# Read all content from the YAML file
content=$(cat "$cc_yaml_file")

echo -e "\033[36mExtracting the value part of 'microsoft.containerinstance.virtualnode.ccepolicy: <value>' from the YAML...\033[0m"
value=$(echo "$content" | grep 'microsoft.containerinstance.virtualnode.ccepolicy:' | awk '{print $2}')

# Check if the value was found
if [ -z "$value" ]; then
  echo -e "\033[31mConfidential container policy not found in the YAML file.\033[0m"
  exit 1
fi

echo -e "\033[36mDecoding the base64 value...\033[0m"
decoded_value=$(echo "$value" | base64 -d)

# Check if the content contains 'azure.workload.identity/use: 'true''
if echo "$content" | grep -q "azure.workload.identity/use: 'true'"; then

  echo -e "\033[36mFinding the 'containers := <JSON>' and read the containers' JSON...\033[0m"
  containers=$(echo "$decoded_value" | grep -oP 'containers\s*:=\s*\K.*')

  # Check if the containers value was found
  if [ -z "$containers" ]; then
    echo -e "\033[31mContainers value not found in the decoded policy.\033[0m"
    exit 1
  fi

  echo -e "\033[36mInjecting the workload identity object into the env_rules & mounts...\033[0m"
  # updated_containers=$(echo "$containers" | jq -c '.[0].env_rules +=
  updated_containers=$(echo "$containers" | jq -c '.[].env_rules +=
  [{
    "pattern": "AZURE_CLIENT_ID=.+",
    "required": false,
    "strategy": "re2"
  },
  {
    "pattern": "AZURE_TENANT_ID=.+",
    "required": false,
    "strategy": "re2"
  },
  {
    "pattern": "AZURE_FEDERATED_TOKEN_FILE=.+",
    "required": false,
    "strategy": "re2"
  },
  {
    "pattern": "AZURE_AUTHORITY_HOST=.+",
    "required": false,
    "strategy": "re2"
  }]' | jq -c '.[].mounts +=
  [{
    "destination": "/var/run/secrets/azure/tokens",
    "options": [
      "rbind",
      "rshared",
      "ro"
    ],
    "source": "sandbox:///tmp/atlas/emptydir/.+",
    "type": "bind"
  }]')

  echo -e "\033[36mReplacing the old JSON with the injected JSON in the decoded policy...\033[0m"
  updated_decoded_value=$(echo "${decoded_value/"$containers"/"$updated_containers"}")

  echo -e "\033[36mEncoding the updated policy to base64...\033[0m"
  updated_value=$(echo "$updated_decoded_value" | base64 -w 0)

  echo -e "\033[36mReplacing the old policy in the YAML file with the new injected value...\033[0m"
  echo "${content/"$value"/"$updated_value"}" >"$cc_yaml_file"

  # echo "$value"
  # echo "$updated_value"

  value="$updated_value"
fi

# print out the sha256 digest of the cce policy
echo -e "\033[36mCalculating the SHA256 digest of the CCE policy...\033[0m"
echo "$value" | base64 -d | sha256sum | cut -d' ' -f1

echo -e "\033[36mDone!\033[0m"
