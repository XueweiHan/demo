FROM mcr.microsoft.com/mirror/docker/library/ubuntu:22.04

RUN apt-get update                                      &&\
    apt-get install -y curl skopeo podman               &&\
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash   &&\
    az extension add -n confcom

COPY test.yaml /src/
WORKDIR /src