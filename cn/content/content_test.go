package content

import (
	"certnoti/model"
	"os"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
)

func TestTemplate(t *testing.T) {
	certInfo := []model.InventoryCredSMARTCertInfo{
		{
			ValidFrom:      time.Now(),
			SecretVersion:  "version.1",
			ThumbprintSHA1: "thumb",
		},
		{
			ValidFrom:      time.Now(),
			SecretVersion:  "version.2",
			ThumbprintSHA1: "thumbprint",
		},
	}

	str, err := generateEmailBody(`../template/email.html`, certInfo)
	assert.NoError(t, err)
	// assert.Fail(t, str)

	if _, err = os.Stat("../bin"); os.IsNotExist(err) {
		err = os.Mkdir("../bin", 0755)
		assert.NoError(t, err)
	}
	err = os.WriteFile("../bin/test.html", []byte(str), 0644)
	assert.NoError(t, err)
}
