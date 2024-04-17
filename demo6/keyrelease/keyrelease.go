package keyrelease

import (
	"bytes"
	"crypto/rsa"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"math/big"
	"net/http"
)

func RetrieveKey(attestName, keyVaultName, keyId string) (*rsa.PrivateKey, error) {
	client := &http.Client{}

	jsonStr, _ := json.Marshal(map[string]string{
		"maa_endpoint": attestName + ".attest.azure.net",
		"akv_endpoint": keyVaultName + ".vault.azure.net",
		"kid":          keyId,
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

	var datakey struct {
		Key string `json:"key"`
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
