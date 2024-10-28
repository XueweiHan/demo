# deployment
```
name=hunter-demo7

az deployment group create -g $name-rg --template-file skr.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"

az attestation show -n ${name/-/}attest -g $name-rg --query attestUri -o tsv
# get the attestation url, like: https://hunterdemo7attest.eus.attest.azure.net

#-> manually update the value section of x-ms-sevsnpvm-hostdata in key-release-policy.json
```

# skr sercurity key release
```
../ccpolicy.sh demo7-skr.yaml

kubectl apply -f demo7-skr-cc.yaml

#-> copy the sha256sum, manually update the `x-ms-sevsnpvm-hostdata` value

az role assignment create --role "Key Vault Crypto Officer" --scope $(az keyvault show -n $name-keyvault --query id -o tsv) --assignee-object-id $(az ad signed-in-user show --query id -o tsv) --assignee-principal-type User

az keyvault key create -n key1 --vault-name $name-keyvault --ops wrapKey unwrapkey encrypt decrypt --kty RSA-HSM --size 3072 --exportable --policy key-release-policy.json

rm pub.pem
az keyvault key download -n key1 --vault-name $name-keyvault -f pub.pem

go run encode.go pub.pem "Top secret!"

#-> test:
https://demo7.hunterapp.net/skr?vault=hunter-demo7-keyvault&attest=hunterdemo7attest.eus&key=key1&message=TcWHyaglwxC0v6cuwTs7xsU1F3djWYEppqIMAbJeCl6nXNI8vquz7J9Nx78X52OLdYkb_lbw_6zGJX9F1shmNfqivlR9bNkmc7d2MBObmw3HFv_VTvvGv607AIjxcCyg19VST-tK-p72UGYxS5eGUkUboE2fcMSOgeZvuWxBLfoPVhUdza2BNmW5dGPKLGwft9bgB2HP6HoUzWe76rHonN3q_J4w7kRgOUkJ4z-Dw6lc3yh7o1-hhWfKNjRnc2sVGXQFVPAuCHkZIQdBGfe099er4buaZf-Kb8kqljGgHDIAK0NtnFFVI1STbre-AGVjW8uFEIEwR7qhCGhk_6LtVU6A5iTKM8Th94wWyYppBCyLaEiuewelxdRmJfumeCUXu8eXRBGV6BaaEAjE_xAjzGhiRnCDxxm4rC505RQgQe9QC3OiPEfzyuOQGqoAqfl6eLiuW3pEqFd_6S17rAhXRbYYBJtRMemp4ocUGkyneRyRfcVYw-NBZV9prcg7L--x
```
