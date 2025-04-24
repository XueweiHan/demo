package model

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestMarshalTag(t *testing.T) {

	obj := &InventoryCredSMARTCertInfo{
		AdditionalData: `{"AutoGenerateValue":false,"AutoRenew":false,"AutoRenewThresholdInDays":0,"CA":"OneCert","CryptoGraphicProvider":"Microsoft RSA SChannel Cryptographic Provider","Description":null,"EnableCertNoti":true,"SecretTags":null,"SecretUsageType":null,"ServiceInstanceType":"Itar","SubjectAlternativeNames":"","SubjectName":"DLMRecordManagementDOD.outlook.com","ValidInDays":0,"ValidityInMonths":12}`,
	}

	b, err := json.Marshal(obj)
	assert.NoError(t, err, b)

	// t.Error(string(b))
}
