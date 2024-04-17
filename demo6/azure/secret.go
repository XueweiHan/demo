package azure

import (
	"context"
	"fmt"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/security/keyvault/azsecrets"
)

func GetKeyVaultSecret(ctx context.Context, w http.ResponseWriter, keyvault, secretName string) string {

	keyVaultUrl := fmt.Sprintf("https://%s.vault.azure.net/", keyvault)

	// create azsecrets client
	client, err := azsecrets.NewClient(keyVaultUrl, GetAzureCredential(), nil)
	if err != nil {
		fmt.Fprintf(w, "Error: Unable to create azsecrets client: %v", err)
		return ""
	}

	// get the secret
	resp, err := client.GetSecret(ctx, secretName, "", nil)
	if err != nil {
		fmt.Fprintf(w, "Error: Unable to get secret: %v", err)
		return ""
	}

	return *resp.Value
}
