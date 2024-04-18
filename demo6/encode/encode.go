package main

import (
	"encoding/base64"
	"fmt"
	"os"

	"github.com/XueweiHan/demo/demo6/key"

	"github.com/golang-jwt/jwt/v5"
)

func main() {
	keyFile := os.Args[1]
	message := os.Args[2]

	fmt.Printf("\nPublic Key: %s \nMessage:    %s\n\n", keyFile, message)

	pemText, _ := os.ReadFile(keyFile)
	pub, _ := jwt.ParseRSAPublicKeyFromPEM(pemText)

	// Encrypt the message with the public key
	rawData := key.EncryptWithPublicKey([]byte(message), pub)
	// Encode the encrypted message
	base64Data := base64.RawURLEncoding.EncodeToString(rawData)

	fmt.Printf("Encrypted:  %s\n\n", base64Data)
}
