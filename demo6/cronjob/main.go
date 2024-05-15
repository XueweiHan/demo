package main

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log"
	"math/rand"
	"net/http"
	"os"
	"strconv"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/azidentity"
	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"

	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/push"
)

var (
	completionTimeGauge = prometheus.NewGauge(prometheus.GaugeOpts{
		Name: "completion_timestamp_seconds",
		Help: "The timestamp of the last completion of cronjob, successful or not.",
	})
	durationGauge = prometheus.NewGauge(prometheus.GaugeOpts{
		Name: "duration_seconds",
		Help: "The duration of the last cronjob in seconds.",
	})
	resultCounter = prometheus.NewCounter(prometheus.CounterOpts{
		Name: "result_processed",
		Help: "The number of result processed in the last run.",
	})
)

func main() {
	// select job
	jobName := "job0"
	if name, ok := os.LookupEnv("JOB_NAME"); ok {
		jobName = name
	}

	jobs := map[string](func() int){
		"job1": performBatchJob1,
		"job2": performBatchJob2,
	}
	job, ok := jobs[jobName]
	if !ok {
		job = func() int { return 0 }
	}

	log.Printf("Starting %s\n", jobName)

	// run job and measure time
	start := time.Now()

	result := job()

	duration := time.Since(start)

	log.Printf("Job completed in %v second, result = %v.\n", duration.Seconds(), result)

	// update total count
	total := updateTheTotalCount(jobName, result)

	// push metrics to pushgateway
	pushUrl := "http://localhost:9091"
	if url, ok := os.LookupEnv("PUSHGATEWAY_ENDPOINT"); ok {
		pushUrl = url
	}
	prometheus.DefaultRegisterer.MustRegister(completionTimeGauge, durationGauge, resultCounter)

	durationGauge.Set(duration.Seconds())
	resultCounter.Add(float64(total))
	completionTimeGauge.SetToCurrentTime()

	err := push.New(pushUrl, jobName).Gatherer(prometheus.DefaultGatherer).Push()
	if err != nil {
		log.Printf("Error pushing metrics to pushgateway: %v", err)
	} else {
		log.Println("Metrics pushed to pushgateway.")
	}
}

func performBatchJob1() int {
	count := 2
	if workCount, ok := os.LookupEnv("WORK_COUNT"); ok {
		if c, err := strconv.Atoi(workCount); err == nil {
			count = c
		}
	}

	baseUrl := os.Getenv("WORKLOAD_URL")
	for i := 0; i < count; i++ {
		r := -1.0
		for r < 0 {
			r = NormalDistributionRand(250, 250)
		}
		load := int(r)
		http.Get(fmt.Sprint(baseUrl, load))
	}

	return count
}

func NormalDistributionRand(desiredMean, desiredStdDev float64) float64 {
	return rand.NormFloat64()*desiredStdDev + desiredMean
}

func performBatchJob2() int {
	return fakeJob(10, 20)
}

func fakeJob(from, to int) int {
	result := 0
	t := from + rand.Intn(to-from)
	log.Println(t)
	for i := 0; i < t*123_456_789; i++ {
		result += rand.Intn(10)
	}

	return result / (rand.Intn(100) + 1) / 100_000
}

func updateTheTotalCount(jobName string, count int) int {
	const partitionKey = "CronJob"
	rowKey := jobName

	accountName, ok := os.LookupEnv("TABLES_STORAGE_ACCOUNT_NAME")
	if !ok {
		panic("TABLES_STORAGE_ACCOUNT_NAME could not be found")
	}
	serviceURL := fmt.Sprintf("https://%s.table.core.windows.net/%s", accountName, "demoTable")

	var cred azcore.TokenCredential
	var err error
	if _, ok := os.LookupEnv("AZURE_CLIENT_ID"); ok {
		cred, err = azidentity.NewWorkloadIdentityCredential(nil)
	} else {
		cred, err = azidentity.NewDefaultAzureCredential(nil)
	}
	if err != nil {
		log.Fatalf("failed to obtain a credential: %v", err)
	}

	client, err := aztables.NewClient(serviceURL, cred, nil)
	if err != nil {
		log.Fatalf("failed to create client: %v", err)
	}
	ctx := context.Background()

	_, err = client.CreateTable(ctx, nil)
	if err != nil {
		var respErr *azcore.ResponseError
		isTableAlreadyExists := errors.As(err, &respErr) && respErr.ErrorCode == string(aztables.TableAlreadyExists)
		if !isTableAlreadyExists {
			log.Fatalf("failed to create table: %v", err)
		}
	}

	var total int
	retry := 3
	for i := 0; ; i++ {
		retry--

		// init total and etag
		total = 0
		etag := azcore.ETag(azcore.ETagAny)
		resp, err := client.GetEntity(ctx, partitionKey, rowKey, nil)
		if err == nil {
			var entity aztables.EDMEntity
			_ = json.Unmarshal(resp.Value, &entity)
			total = int(entity.Properties["Counter"].(int32))
			etag = resp.ETag
		}

		// plus new count
		total += count

		// marshal table entity
		marshalled, _ := json.Marshal(aztables.EDMEntity{
			Entity: aztables.Entity{
				PartitionKey: partitionKey,
				RowKey:       rowKey,
			},
			Properties: map[string]any{
				"Counter": total,
			},
		})

		// update table entity with etag
		_, err = client.UpsertEntity(ctx, marshalled, &aztables.UpsertEntityOptions{ETag: etag, UpdateMode: aztables.UpdateModeReplace})
		if err == nil {
			log.Printf("Counter updated to %d\n", total)
			break
		}
		log.Printf("failed to update counter: %v", err)

		if retry == 0 {
			log.Printf("failed to update counter: %v", err)
			break
		}

		// backoff with increasing and random delay
		delay := 100*i + rand.Intn(100)
		log.Printf("failed to update counter, retrying (%d)...", delay)
		time.Sleep(time.Duration(delay) * time.Millisecond)
	}

	return total
}
