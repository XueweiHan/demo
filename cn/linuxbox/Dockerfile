# FROM ubuntu:22.04
FROM mcr.microsoft.com/devcontainers/base:ubuntu-22.04

RUN apt update &&\
    apt install ca-certificates curl git gpg libicu70 lsb-release make -y

# Install Go language
RUN rm -rf /usr/local/go &&\
    curl -L https://go.dev/dl/go1.22.0.linux-amd64.tar.gz | tar xzf - -C /usr/local &&\
    echo 'PATH=$PATH:/usr/local/go/bin' >>~/.bashrc

# Install Azure cli
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Add Docker's official GPG key, and then add the Docker package repository to the APT sources list.
RUN install -m 0755 -d /etc/apt/keyrings &&\
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc &&\
    chmod a+r /etc/apt/keyrings/docker.asc &&\
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
      $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null

# Add the Kubernetes package repository GPG key, and then add the Kubernetes package repository to the APT sources list.
RUN curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.29/deb/Release.key | gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg &&\
    echo 'deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.29/deb/ /' | tee /etc/apt/sources.list.d/kubernetes.list

# Add the Microsoft package repository GPG key, and then add the Microsoft package repository to the APT sources list.
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /etc/apt/trusted.gpg.d/microsoft.gpg &&\
    sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -cs)-prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/dotnetdev.list'

# Install docker cli & Azure functions core tools & kubectl & helm
RUN apt update && apt upgrade -y &&\
    apt install docker-ce-cli docker-compose kubectl azure-functions-core-tools-4 -y

# Install bicep, aks-preview, confcom, attestation
RUN az bicep install &&\
    az extension add --name aks-preview --allow-preview true &&\
    az extension update --name aks-preview &&\
    az extension add --name confcom --allow-preview true &&\
    az extension update --name confcom &&\
    az extension add --name attestation --allow-preview true &&\
    az extension update --name attestation

# Install Helm
RUN curl -L https://get.helm.sh/helm-v3.14.3-linux-amd64.tar.gz | tar xzf - -C /tmp &&\
    mv /tmp/linux-amd64/helm /usr/local/bin/helm &&\
    rm -r /tmp/linux-amd64

# download ev2 from https://msazure.visualstudio.com/374a6e5a-a546-406d-9ca9-6978f0e9c780/_apis/git/repositories/829556af-1b86-4665-98e6-430dd4d5bb7b/items?path=/Ev2_CLI/Linux/ev2&versionDescriptor%5BversionOptions%5D=0&versionDescriptor%5BversionType%5D=0&versionDescriptor%5Bversion%5D=master&resolveLfs=true&%24format=octetStream&api-version=5.0&download=true
# Install ev2
# COPY ev2 /usr/local/bin/ev2

WORKDIR /src
