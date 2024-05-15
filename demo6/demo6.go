package main

import (
	"encoding/base64"
	"fmt"
	"io"
	"log"
	"math/rand"
	"net/http"
	"runtime"
	"strconv"
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

	inFlightGauge = prometheus.NewGauge(prometheus.GaugeOpts{
		Name: "http_in_flight_requests",
		Help: "A gauge of requests currently being served by the wrapped handler.",
	})

	counterVec = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "http_requests_total",
			Help: "A counter for requests to the wrapped handler.",
		},
		[]string{"handler", "code", "method"},
	)

	// durationHisVec is partitioned by the HTTP method and handler. It uses custom
	// buckets based on the expected request durationHisVec.
	durationHisVec = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name:    "http_request_duration_seconds",
			Help:    "A histogram of latencies for requests.",
			Buckets: prometheus.DefBuckets,
		},
		[]string{"handler", "code", "method"},
	)

	// responseSizeHisVec has no labels, making it a zero-dimensional
	// ObserverVec.
	responseSizeHisVec = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name:    "http_response_size_bytes",
			Help:    "A histogram of response sizes for requests.",
			Buckets: []float64{200, 500, 900, 1500, 2500, 5000},
		},
		[]string{"handler", "code", "method"},
	)

	timeToWriteHeaderHisVec = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name: "http_write_header_duration_seconds",
			Help: "A histogram of time to first write latencies.",
		},
		[]string{"handler", "code", "method"},
	)
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

	prometheus.MustRegister(inFlightGauge, counterVec, durationHisVec, responseSizeHisVec, timeToWriteHeaderHisVec)

	// test workloads identity
	http.Handle("/", makeInstrumentedHandler(rootHandler, "root"))

	// test https call
	http.Handle("/https/", makeInstrumentedHandler(webProxyHandler, "webProxy"))

	// test kubenettes api call
	http.Handle("/pods", makeInstrumentedHandler(listPodsHandler, "listPods"))

	http.Handle("/favicon.ico", http.NotFoundHandler())

	http.Handle("/workload", makeInstrumentedHandler(workloadHandler, "workload"))

	// prometheus metrics
	http.Handle("/metrics", promhttp.Handler())

	go testOps()

	http.ListenAndServe(":"+port, nil)
}

func workloadHandler(w http.ResponseWriter, r *http.Request) {
	start := time.Now()

	load := r.URL.Query().Get("load")
	n, err := strconv.Atoi(load)
	if err != nil {
		n = 300
	}

	resultCh := make(chan int)
	timeout := false
	go func() {
		r := 0
		for !timeout {
			r += rand.Intn(100)
		}
		resultCh <- r
	}()
	time.Sleep(time.Duration(n) * time.Millisecond)
	timeout = true
	result := <-resultCh

	duration := time.Since(start)

	if rand.Intn(100) < 5 {
		http.Error(w, "Simulate 5% of fake Server Error\n", http.StatusInternalServerError)
	}

	fmt.Fprintf(w, "execution time: %s\nresult: %d\n", duration, result)
}

func makeInstrumentedHandler(handler http.HandlerFunc, handlerLabel string) http.Handler {
	return promhttp.InstrumentHandlerInFlight(inFlightGauge,
		promhttp.InstrumentHandlerCounter(counterVec.MustCurryWith(prometheus.Labels{"handler": handlerLabel}),
			promhttp.InstrumentHandlerDuration(durationHisVec.MustCurryWith(prometheus.Labels{"handler": handlerLabel}),
				promhttp.InstrumentHandlerTimeToWriteHeader(timeToWriteHeaderHisVec.MustCurryWith(prometheus.Labels{"handler": handlerLabel}),
					promhttp.InstrumentHandlerResponseSize(responseSizeHisVec.MustCurryWith(prometheus.Labels{"handler": handlerLabel}),
						handler,
					),
				),
			),
		),
	)
}

func rootHandler(w http.ResponseWriter, r *http.Request) {

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
}

func webProxyHandler(w http.ResponseWriter, r *http.Request) {
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
}

func listPodsHandler(w http.ResponseWriter, r *http.Request) {
	log.Printf("%s %s", r.Method, r.URL.Path)
	fmt.Fprint(w, k8s.GetK8sPods())
}
