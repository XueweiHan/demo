
# Install dev build
```
# download the dev build
az artifacts universal download --organization "https://msazure.visualstudio.com/" --project "b32aa71e-8ed2-41b2-9d77-5bc261222004" --scope project --feed "acc-confcom-azure-cli-extensions" --name "confcom" --version "1.2.2-alpha.2" --path .

# uninstall confcom extension
az extension remove -n confcom

# install the dev build confcom extension
az extension add --source ./confcom-1.2.2a2-py3-none-any.whl -y
```

# run it in Docker
```
docker build -t podman .

docker run -it --rm -v "C:\repos\demo\demo7\podman:/src/test" -v "$Env:USERPROFILE\.azure:/root/.azure" -v "$Env:USERPROFILE\.kube:/root/.kube" podman /bin/bash
```


# generate ccpolicy using podman
```
# az login --

# install podman in Ubuntu build agent
apt-get -y install podman

# acr login (change to the sweeper acr name)
podman login hunterdemo7cr.azurecr.io -u 00000000-0000-0000-0000-000000000000 -p $(az acr login -n hunterdemo7cr --expose-token --query accessToken -o tsv)

# pull image (change to sweeper image)
podman pull hunterdemo7cr.azurecr.io/hunter-demo7:0.1

# save image to tarball (change to sweeper image)
podman save --format oci-archive -o ./image.tar hunterdemo7cr.azurecr.io/hunter-demo7:0.1

# update the ccpolicy.sh to use local tarball for policy gen
# az confcom acipolicygen -y --virtual-node-yaml demo7-podman-cc.yaml --tar ./image.tar
./ccpolicy-podman.sh demo7-podman.yaml
```
