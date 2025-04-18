FROM mcr.microsoft.com/cbl-mariner/base/core:2.0
# FROM mcr.microsoft.com/mirror/docker/library/ubuntu:22.04

RUN tdnf install -y skopeo                              &&\
# RUN apt-get update                                      &&\
#     apt-get install -y curl skopeo                      &&\
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash   &&\
    az extension add -n confcom

COPY test.yaml /src/
WORKDIR /src