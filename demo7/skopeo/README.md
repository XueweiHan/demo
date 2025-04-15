
```
docker build -t ptmsdev/skopeo .

docker run -it --rm ptmsdev/skopeo /bin/bash


skopeo login huntersharedcr.azurecr.io -u 00000000-0000-0000-0000-000000000000 -p $(az acr login -n huntersharedcr --expose-token --query accessToken -otsv)

skopeo copy -f oci docker://huntersharedcr.azurecr.io/test:ubuntu2204 oci-archive:./test.tar

az confcom acipolicygen -y --virtual-node-yaml test.yaml --tar test.tar
```