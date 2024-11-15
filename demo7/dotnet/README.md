# docker build
```
export name=hunter-demo7
export acr=${name/-/}cr.azurecr.io
export ver=0.1

docker build -t $acr/demo7-dotnet:$ver .

docker run --rm -p 8082:8082 -p 4000:4000 $acr/demo7-dotnet:$ver

#-> test http://localhost:8082/hello/wrold?name=Hunter in browser

az acr login -n $acr
docker push $acr/demo7-dotnet:$ver
```

# deploy
```
#-> manually update the dotnet.yaml to the following image
echo $acr/demo7-dotnet:$ver

../ccpolicy.sh dotnet.yaml

kubectl apply -f dotnet-cc.yaml

https://demo7.hunterapp.net/dotnet/wrold?name=Hunter
```

# verify
```
ip=$(kubectl get service -n app-routing-system nginx -o jsonpath="{.status.loadBalancer.ingress[0].ip}")



curl https://test.ajzxhub.net/hello/wrold?name=Hunter | jq

metrics
kubectl port-forward <pod> 80:4000
curl localhost/metrics

```
