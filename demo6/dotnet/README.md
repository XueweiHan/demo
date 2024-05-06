# docker build
```
docker build -t xueweihan/demo6-dotnet:0.1 .
docker run --rm -p 8082:8082 -p 4000:4000 xueweihan/demo6-dotnet:0.1
test http://localhost:8082/hello/wrold?name=Hunter in browser
docker login
docker push xueweihan/demo6-dotnet:0.1
```

# deploy
```
kubectl apply -f dotnet.yaml
```

# verify
```
kubectl get all
kubectl port-forward <pod> 80:8082
curl localhost/hello/wrold?name=Hunter

curl https://test.ajzxhub.net/hello/wrold?name=Hunter | jq

metrics
kubectl port-forward <pod> 80:4000
curl localhost/metrics

```

# confcom
```
az confcom katapolicygen -y dotnet.yaml --print-policy | base64 -d | sha256sum | cut -d' ' -f1
kubectl apply -f dotnet.yaml
```