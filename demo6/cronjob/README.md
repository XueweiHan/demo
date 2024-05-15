# test pushgateway locally
```
docker run --rm -p 9091:9091 prom/pushgateway:v1.8.0

in local bash console (like git bash)

echo "some_metric 3.14" | curl --data-binary @- localhost:9091/metrics/job/some_job

curl localhost:9091/metrics

# TYPE some_metric untyped
some_metric{instance="",job="some_job"} 3.14



push more complex metrics

cat <<EOF | curl --data-binary @- localhost:9091/metrics/job/some_job/instance/some_instance
# TYPE some_more_metric counter
some_more_metric{label="val1"} 42
# TYPE another_metric gauge
# HELP another_metric Just an example.
another_metric 2398.283
EOF

curl localhost:9091/metrics

# HELP another_metric Just an example.
# TYPE some_more_metric counter
some_more_metric{instance="some_instance",job="some_job",label="val1"} 42
# TYPE another_metric gauge
another_metric{instance="some_instance",job="some_job"} 2398.283

```

# deploy pushgateway to k8s
```
kubectl apply -f pushgateway.yaml

kubectl port-forward service/pushgateway 9091:9091

curl localhost:9091/metrics

echo "some_metric 3.14" | curl --data-binary @- localhost:9091/metrics/job/some_job

curl localhost:9091/metrics
```
# build go job
```
docker build -t xueweihan/demo6-cronjob:0.1 .
docker run --rm xueweihan/demo6-cronjob:0.1
docker push xueweihan/demo6-cronjob:0.1
```
# deploy cronjob
```
kubectl apply -f cronjob.yaml
kubectl get all
kubectl logs job.batch/time-trigger-job2-28576876
```
# assign role to storage account

az role assignment create --role "Storage Table Data Contributor" --scope $(az storage account show -n hunterdemo6sa --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv) --assignee-principal-type User
