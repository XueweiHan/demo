# https ingress ??
```
# get you ingress service external ip
kubectl get service -n app-routing-system nginx -o jsonpath="{.status.loadBalancer.ingress[0].ip}"

#-> Update DNS zone record: DNS Zone -> DNS Manangement -> Recordsets -> Add -> Name - demo7 ; Type - A ; TTL - 1 Minutes; IP Address - <external-ip>

#-> Update the domain name in the ingress.yaml current is :<hunterapp.net>

# https ingress
kubectl apply -f ingress.yaml

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.14.4/cert-manager.yaml

kubectl get pods -n cert-manager

kubectl apply -f issuer.yaml

kubectl describe secret letsencrypt-key

#-> check in browser https://demo7.hunterapp.net/?vault=hunter-demo7-keyvault&secret=testkey
```
