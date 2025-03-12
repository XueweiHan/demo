FROM ubuntu:22.04

COPY confcom-1.2.2a2-py3-none-any.whl /src/

WORKDIR /src

RUN apt-get update                                                      &&\
    apt-get -y install podman curl jq                                   &&\
    # install az cli
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash                   &&\
    # az extension remove -n confcom                                    &&\
    # install the dev build of confcom extension
    az extension add --source ./confcom-1.2.2a2-py3-none-any.whl -y


# CMD sleep infinity
