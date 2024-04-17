package azure

import (
	"context"
	"fmt"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/security/keyvault/azsecrets"
)

func CreateQueue(ctx context.Context, w http.ResponseWriter, queue, value string) string {

}
