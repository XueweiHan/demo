package demo

import (
	"bytes"
	"certnoti/azure/credential"
	"context"
	"crypto/rsa"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"math/big"
	"net/http"
	"os"

	"github.com/Azure/azure-sdk-for-go/sdk/keyvault/azsecrets"
)

func Demo() {
	//println("{\"maa_endpoint\": \"" + "SkrClientMAAEndpoint" + "\", \"akv_endpoint\": \"" + "SkrClientAKVEndpoint" + "\", \"kid\": \"" + "SkrClientKID" + "\"}")

	log.Println("Starting the application...")

	// create credential
	cred := credential.GetAzureCredential()
	//cred, err := azidentity.NewDefaultAzureCredential(nil)

	keyVaultName := os.Getenv("KEYVAULT_NAME")
	keyVaultUrl := fmt.Sprintf("https://%s.vault.azure.net/", keyVaultName)

	// create azsecrets client
	client, err := azsecrets.NewClient(keyVaultUrl, cred, nil)
	if err != nil {
		log.Fatal(err)
	}

	secretName := os.Getenv("SECRET_NAME")

	ctx := context.Background()

	// get the secret
	resp, err := client.GetSecret(ctx, secretName, "", nil)
	if err != nil {
		log.Println(err)
	} else {
		log.Println("Secret value: ", *resp.Value)
	}

	secretValue := *resp.Value

	key, err := retrieveKey()
	if err != nil {
		log.Println(err)
	} else {
		log.Println("Serect key: ", key)
	}

	// create a simple server
	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		fmt.Fprint(w, "Secret value: ", secretValue)
	})

	log.Fatal(http.ListenAndServe(":8000", nil))
}

var datakey struct {
	Key string `json:"key"`
}

func retrieveKey() (*rsa.PrivateKey, error) {
	client := &http.Client{}
	// var data = strings.NewReader("{\"maa_endpoint\": \"" + os.Getenv("SkrClientMAAEndpoint") + "\", \"akv_endpoint\": \"" + os.Getenv("SkrClientAKVEndpoint") + "\", \"kid\": \"" + os.Getenv("SkrClientKID") + "\"}")

	keyVaultName := os.Getenv("KEYVAULT_NAME")

	jsonStr, _ := json.Marshal(map[string]string{
		"maa_endpoint": os.Getenv("SKR_MMA_ENDPOINT"),
		"akv_endpoint": fmt.Sprintf("%s.vault.azure.net", keyVaultName),
		"kid":          os.Getenv("SKR_KEY_ID"),
	})
	data := bytes.NewBuffer(jsonStr)

	req, err := http.NewRequest("POST", "http://localhost:8080/key/release", data)
	if err != nil {
		return nil, err
	}
	req.Header.Set("Content-Type", "application/json")
	resp, err := client.Do(req)
	var bodyText []byte
	if resp != nil && resp.Body != nil {
		defer resp.Body.Close()

		limitSize := resp.ContentLength
		const mb134 = 1 << 27 //134MB
		if limitSize < 1 || limitSize > mb134 {
			limitSize = mb134
		}
		bodyText, _ = io.ReadAll(io.LimitReader(resp.Body, int64(limitSize)))
	}
	if err != nil {
		log.Printf("Error response body from SKR: %s", bodyText)
		return nil, err
	}
	if resp.StatusCode < 200 || resp.StatusCode > 207 {
		return nil, fmt.Errorf("unable to retrieve key from SKR.  Response Code %d.  Message %s", resp.StatusCode, string(bodyText))
	}

	if err := json.Unmarshal(bodyText, &datakey); err != nil {
		log.Printf("retrieve key unmarshal error: %s", err.Error())
	}

	key, err := RSAPrivateKeyFromJWK([]byte(datakey.Key))
	if err != nil {
		log.Printf("construct private rsa key from jwk key error: %s", err.Error())
	}

	return key, nil
}

func RSAPrivateKeyFromJWK(key1 []byte) (*rsa.PrivateKey, error) {

	var jwkData struct {
		N string `json:"n"`
		E string `json:"e"`
		D string `json:"d"`
		P string `json:"p"`
		Q string `json:"q"`
	}

	if err := json.Unmarshal(key1, &jwkData); err != nil {
		log.Println(err.Error())
	}
	n, err := base64.RawURLEncoding.DecodeString(jwkData.N)
	if err != nil {
		log.Println(err.Error())
	}
	e, err := base64.RawURLEncoding.DecodeString(jwkData.E)
	if err != nil {
		log.Println(err.Error())
	}
	d, err := base64.RawURLEncoding.DecodeString(jwkData.D)
	if err != nil {
		log.Println(err.Error())
	}
	p, err := base64.RawURLEncoding.DecodeString(jwkData.P)
	if err != nil {
		log.Println(err.Error())
	}
	q, err := base64.RawURLEncoding.DecodeString(jwkData.Q)
	if err != nil {
		log.Println(err.Error())
	}

	key := &rsa.PrivateKey{
		PublicKey: rsa.PublicKey{
			N: new(big.Int).SetBytes(n),
			E: int(new(big.Int).SetBytes(e).Int64()),
		},
		D: new(big.Int).SetBytes(d),
		Primes: []*big.Int{
			new(big.Int).SetBytes(p),
			new(big.Int).SetBytes(q),
		},
	}

	return key, nil
}
