# docker build
```
docker build -t xueweihan/demo6-dotnet:0.1 .
docker run --rm -p 8082:8082 xueweihan/demo6-dotnet:0.1
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
check the loadbalancer ip
curl http://x.x.x.x/hello/wrold?name=Hunter

curl https://test.ajzxhub.net/hello/wrold?name=Hunter | jq
```

# confcom
```
az confcom katapolicygen -y dotnet.yaml --print-policy | base64 -d | sha256sum | cut -d' ' -f1
kubectl apply -f dotnet.yaml
```