package azure

import (
	"log"
	"os"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/azidentity"
)

var cred azcore.TokenCredential

func GetAzureCredential() azcore.TokenCredential {
	if cred != nil {
		return cred
	}

	// create credential
	var err error

	if _, ok := os.LookupEnv("AZURE_CLIENT_ID"); ok {
		cred, err = azidentity.NewWorkloadIdentityCredential(nil)
	} else {
		cred, err = azidentity.NewDefaultAzureCredential(nil)
	}

	if err != nil {
		log.Fatalf("failed to obtain a credential: %v", err)
	}

	return cred
}
