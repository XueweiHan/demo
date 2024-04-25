package main

import (
	"encoding/base64"
	"fmt"
	"io"
	"log"
	"math/rand"
	"net/http"
	"runtime"
	"time"

	"github.com/XueweiHan/demo/demo6/azure"
	"github.com/XueweiHan/demo/demo6/k8s"
	"github.com/XueweiHan/demo/demo6/key"
	"github.com/XueweiHan/demo/demo6/keyrelease"

	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promauto"
	"github.com/prometheus/client_golang/prometheus/promhttp"
)

var (
	opsProcessed = promauto.NewCounter(prometheus.CounterOpts{
		Name: "demo6_test_ops_total",
		Help: "The total number of processed events",
	})
)

func testOps() {
	for {
		opsProcessed.Inc()

		time.Sleep(time.Duration(rand.Int31n(2)) * time.Second)
	}
}

func main() {
	port := "8081"
	log.Printf("Server starting on port %v\n", port)

	// test workloads identity
	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		log.Printf("%s %s", r.Method, r.URL.Path)
		fmt.Fprintf(w, "%v %v\n%v %v\n\n", r.Method, r.URL, runtime.GOOS, runtime.GOARCH)

		attestName := r.URL.Query().Get("attest")
		keyvaultName := r.URL.Query().Get("vault")
		secretName := r.URL.Query().Get("secret")
		keyId := r.URL.Query().Get("key")
		message := r.URL.Query().Get("message")

		// test keyvault secret call
		if secretName != "" && keyvaultName != "" {
			fmt.Fprintf(w, "Keyvault Name : %s\n", keyvaultName)
			fmt.Fprintf(w, "Secret Name   : %s\n", secretName)

			secret := azure.GetKeyVaultSecret(r.Context(), w, keyvaultName, secretName)
			fmt.Fprintf(w, "Secret Value  : %s\n", secret)
		}

		// test keyvault key decrypt call
		if keyId != "" && attestName != "" && keyvaultName != "" {
			fmt.Fprintf(w, "Attestation : %s.attest.azure.net\n", attestName)
			fmt.Fprintf(w, "Keyvault    : %s\n", keyvaultName)
			fmt.Fprintf(w, "Key ID      : %s\n", keyId)

			privateKey, err := keyrelease.RetrieveKey(attestName, keyvaultName, keyId)
			if err != nil {
				fmt.Fprintf(w, "Error: Unable to retrieve key: %v", err)
				return
			}

			data, err := base64.RawURLEncoding.DecodeString(message)
			if err != nil {
				fmt.Fprintln(w, "Error: Unable to decode message: ", err)
			}

			plaintext := key.DecryptWithPrivateKey(data, privateKey)

			fmt.Fprintf(w, "Decrypted Message : %s\n", string(plaintext))
		}
	})

	// test https call
	http.HandleFunc("/https/", func(w http.ResponseWriter, r *http.Request) {
		log.Printf("%s %s", r.Method, r.URL.Path)
		url := "https://" + fmt.Sprint(r.URL)[7:]
		resp, err := http.Get(url)
		if err != nil {
			fmt.Fprintf(w, "Error: %v", err)
			return
		}
		defer resp.Body.Close()
		body, err := io.ReadAll(resp.Body)
		if err != nil {
			fmt.Fprintf(w, "Error: %v", err)
			return
		}

		w.WriteHeader(resp.StatusCode)
		w.Header().Set("Content-Type", resp.Header.Get("Content-Type"))
		w.Write(body)
	})

	// test kubenettes api call
	http.HandleFunc("/pods", func(w http.ResponseWriter, r *http.Request) {
		log.Printf("%s %s", r.Method, r.URL.Path)
		fmt.Fprint(w, k8s.GetK8sPods())
	})

	// prometheus metrics
	http.Handle("/metrics", promhttp.Handler())
	go testOps()

	http.ListenAndServe(":"+port, nil)
}
