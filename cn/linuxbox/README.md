# Build
    docker build -t torusdev.azurecr.io/devlinux .

# Push
    az login
    az acr login -n torusdev
    docker push torusdev.azurecr.io/devlinux

# Create container
    Switch to the folder of the code enlistment.
    docker-compose -f linuxbox.yaml up --no-start
    or
    docker run -d --name linuxbox -v "c:\\repos:/src" -v "//var/run/docker.sock:/var/run/docker.sock" -p 7071:7071 torusdev/devlinux
    
# Run
    docker start linuxbox
    docker exec -it linuxbox /bin/bash
