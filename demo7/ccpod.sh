#!/bin/bash
set -e

# Get the pod id
rg="$1"
pod_id="$2"

pod=$(kubectl get pod --output="jsonpath={.metadata.uid}" $pod_id)
az container show -g $rg -n ${pod}_0 --query sku -o tsv
