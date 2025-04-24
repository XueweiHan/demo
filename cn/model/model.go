package model

import (
	"time"
)

const QueryForInventoryCredSMARTCertInfo = `CredSmartSecretInstance
| where ingestion_time() > ago(2d) and datetime_diff('day', validTo, now()) < 90 and datetime_diff('day', validTo, now()) > 0
| summarize arg_max(validTo, *) by secretMetadataId
| join kind = innerunique 
	(CredSmartSecretMetadata
	| where ingestion_time() > ago(2d) and additionalData["AutoRenew"] == false and (additionalData["ValidityInMonths"] > 0 or additionalData["ValidInDays"] > 0)
	| summarize arg_max(createAt, *) by secretName) 
	on $left.secretMetadataId == $right.id
| project secretMetadataId, secretVersion, secretType, thumbprintSHA1, validFrom, validTo, workload, serviceTreeId, icmTeamName, secretName, deploymentPolicy = dynamic_to_json(deploymentPolicy), secretStatus = dynamic_to_json(additionalData["SecretStatus"]), additionalData = dynamic_to_json(additionalData1)`

type InventoryCredSMARTCertInfo struct {
	SecretMetadataId string    `kusto:"secretMetadataId"`
	SecretVersion    string    `kusto:"secretVersion"`
	SecretType       string    `kusto:"secretType"`
	ThumbprintSHA1   string    `kusto:"thumbprintSHA1"`
	ValidFrom        time.Time `kusto:"validFrom"`
	ValidTo          time.Time `kusto:"validTo"`
	Workload         string    `kusto:"workload"`
	ServiceTreeId    string    `kusto:"serviceTreeId"`
	IcmTeamName      string    `kusto:"icmTeamName"`
	SecretName       string    `kusto:"secretName"`
	DeploymentPolicy string    `kusto:"deploymentPolicy" marshal:"json"`
	SecretStatus     string    `kusto:"secretStatus" marshal:"json"`
	AdditionalData   string    `kusto:"additionalData" marshal:"json"`
}

func (o InventoryCredSMARTCertInfo) MarshalJSON() ([]byte, error) {
	return MarshalJSONField(o)
}
