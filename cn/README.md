# Introduction 
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project. 

# Getting Started

- **Container dev environment**: All command line dev tools are in the one container, we call it **Devbox**.
    1. The only software you need to install in your machine is [Docker Desktop](https://www.docker.com/products/docker-desktop/) Windows version
    2. open `PowerShell` and get to the code root path then run following commands
        - Initialize Linux **Devbox**
            ```
            docker-compose -f devbox.yaml up --no-start
            ```
        - Enter **Devbox**
            ```
            docker start -i devbox
            ```
        - Delete **Devbox** (this is just for rest everything)
            ```
            docker rm -f devbox
            ```
        The **Devbox** is a Linux contianer running under Hyper-V Linux kernel, so you get a pure Linux command line enviroment in **Devbox**, and you can also use your Windows tools, like using **Git Extensions** for changings; using **VS Code** for coding and debugging.
        
        In the **Devbox** you get all Linux command line tools you need: `azure-cli`, `git`, `go`, `docker`, `make`. If you need more tools, just simply run `apk add <package>`. 

- **The old way** (not recommended): You need to install everything (Windows version) in your dev machine
    - install `git` with `git-bash`
    - install `go` lang
    - install `GNU make`
    - install `Docker Desktop`
    - install `VS Code` with some extensions
        - `Azure Account`
        - `Azure Functions`
        - `Go`

# Build
We can build and run different version of app in local box.
- Build service for your current enviroment `make`
- Build Windows app `make windows`
- Build Linux app `make linux`

# Run
- Run local Windows app `make run`
- Run Linux app in container `make dockerrun`
- Run Windows app in local Azure functions `make azrun`

# Plagyground Deploying
- Deploy Linux app to playground `make publish`

# Debugging
- VS code debugging configures in the file `./.vscode/launch.json` 
    ```
    {
        "version": "0.2.0",
        "configurations": [
            {
                "name": "Launch App",
                "type": "go",
                "request": "launch",
                "mode": "auto",
                "program": "."
            },
            {
                "name": "Remote Docker App",
                "type": "go",
                "request": "attach",
                "mode": "remote",
                "port": 4000,
                "host": "localhost"
            }
        ]
    }
    ```
- Debug local Windows app

    - Go to VS code `Run and debug` tab on the left side bar, select `Launch App`, and click run.

- Debug remote Linux app in the container

    - Run `make dockerdbg` in the the bash terminal
    - Wait the debugging version docker build and running, you will see `debug layer=debugger Adding target 13 "/app/go-app"`
    - Set your debugging break point in the VS code editor.
    - Go to VS code `Run and debug` tab, select `Remote Docker App`, and run.
    - Now you are in remote debugging mode, you can trigger your code path and debug the code in VS code.
    - Stop debugging, just `ctrl-c` in the bash terminal to stop the container.

# API references
TODO


# Deployment
- Enable the pipeline build pool to push image to acr
    - Build pool:
        ```
        /subscriptions/7f16526e-db30-44dc-ad47-73bcffc664a4/resourceGroups/torusaks/providers/Microsoft.CloudTest/hostedpools/Torus-1eshp-unofficial

        /subscriptions/7f16526e-db30-44dc-ad47-73bcffc664a4/resourceGroups/torusaks/providers/Microsoft.CloudTest/hostedpools/Torus-1eshp-official
        ```
    - Build pool MSI:
        ```
        /subscriptions/7f16526e-db30-44dc-ad47-73bcffc664a4/resourcegroups/TorusAKS/providers/Microsoft.ManagedIdentity/userAssignedIdentities/Torus-1eshp-unofficial-MSI

        /subscriptions/7f16526e-db30-44dc-ad47-73bcffc664a4/resourcegroups/TorusAKS/providers/Microsoft.ManagedIdentity/userAssignedIdentities/Torus-1eshp-official-MSI
        ```
    - Assign the AcrPush role to build pool msi:
        ```
        az role assignment create --role "AcrPush" --assignee <pool-msi-client-id> --scope /subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/torusdev-rg/providers/Microsoft.ContainerRegistry/registries/torusacr

        (Contributor)
        ```
    - Assign the AcrPull role to aks pool msi:
        ```
        az role assignment create --role "AcrPull" --assignee <aks-agentpool-msi-client-id> --scope /subscriptions/b47beaaf-7461-4b34-844a-7105d6b8c0d7/resourceGroups/torusdev-rg/providers/Microsoft.ContainerRegistry/registries/torusacr
        ```
    - Assign what ever role to aks -identiy for aks serverAccount
        ```
        31baa029-02f1-49d4-8685-431c701eb276
        ```

# AKS login
- PPE
    ```
    az login
    az account set --subscription b47beaaf-7461-4b34-844a-7105d6b8c0d7
    az aks get-credentials -g certnoti-ppe-rg -n certnoti-ppe-aks
    ```


# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)