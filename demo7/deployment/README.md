
# Deploy command

```
export name=hunter-demo3
az deployment sub create --template-file main.bicep --parameters "{\"project_name\":{\"value\":\"$name\"}}"
```

# Deploy file org tree
```
main.bicep                          (sub)   (project_name, shared_name, location)

    shared-rg.bicep                 (sub)   (shared_name, location)

        shared-rg

        shared.bicep                (shared-rg) (shared_name, location)
            acr
            keyvault
            log-analytics-workspace
            service-bus

    aks-rg.bicep                    (sub)  (shared_name, project_name, location)

        aks-rg

        aks.bicep                   (aks-rg)    (project_name, location)
            public-ip
            nat-gateway
            vnet
            aks
            identity
            fic

            aks-rg-role.bicep       (aks-rg)    (project_name)
            aks-rg-role.bicep       (aks-node-rg)   (project_name)

        shared-role.bicep           (shared-rg) (project_name, shared_name)
            aks-acr-role
            aks-keyvault-role

```