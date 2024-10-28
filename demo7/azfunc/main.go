package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"
)

func main() {
	hostname := os.Getenv("HOSTNAME")

	http.HandleFunc("/api/hello", func(w http.ResponseWriter, r *http.Request) {
		fmt.Fprintf(w, "HTTP Triggered Function host: %s\n", hostname)
	})

	http.HandleFunc("/timer1", func(w http.ResponseWriter, r *http.Request) {

		item := readRequestData(r, "myTimer")

		out := map[string]interface{}{
			"msg": fmt.Sprintf("==> MESSAGE created by timer trigger of %s at %s", hostname, time.Now().Format(time.RFC3339)),
		}
		logs := []string{fmt.Sprintf("LOG: timer trigger host: %s, data: %s, out: %v", hostname, item, out)}

		writeResponse(w, out, logs, nil)
	})

	http.HandleFunc("/queue1", func(w http.ResponseWriter, r *http.Request) {

		item := readRequestData(r, "testQueueItem")
		var str string
		json.Unmarshal(item, &str)

		var logs []string
		logs = append(logs, "LOG: the queue data is: "+str)

		writeResponse(w, nil, logs, nil)
	})

	listenAddr := ":8083"
	if val, ok := os.LookupEnv("FUNCTIONS_CUSTOMHANDLER_PORT"); ok {
		listenAddr = ":" + val
	}

	log.Printf("About to listen on https://localhost%s/ from host %s", listenAddr, hostname)
	log.Fatal(http.ListenAndServe(listenAddr, nil))
}

func writeResponse(w http.ResponseWriter, outputs map[string]interface{}, logs []string, returnValue interface{}) {
	type InvokeResponse struct {
		Outputs     map[string]interface{}
		Logs        []string
		ReturnValue interface{}
	}

	iresp := InvokeResponse{
		Outputs:     outputs,
		Logs:        logs,
		ReturnValue: returnValue,
	}

	data, _ := json.Marshal(iresp)

	w.Header().Set("Content-Type", "application/json")
	w.Write(data)
}

func readRequestData(r *http.Request, name string) json.RawMessage {
	defer r.Body.Close()
	type InvokeRequest struct {
		Data     map[string]json.RawMessage
		Metadata map[string]interface{}
	}
	var invokeRequest InvokeRequest
	d := json.NewDecoder(r.Body)
	d.Decode(&invokeRequest)

	return invokeRequest.Data[name]
}
