param virtualNetworks_vnet_eastus_name string = 'vnet-eastus'
param virtualMachineScaleSets_hunter_vmss_name string = 'hunter-vmss'
param virtualMachines_hunter_vmss_9bb46e8a_name string = 'hunter-vmss_9bb46e8a'
param networkInterfaces_vnet_eastus_nic01_cff6ed40_name string = 'vnet-eastus-nic01-cff6ed40'
param networkSecurityGroups_basicNsgvnet_eastus_nic01_name string = 'basicNsgvnet-eastus-nic01'
param publicIPAddresses_publicIp_vnet_eastus_nic01_cff6ed40_name string = 'publicIp-vnet-eastus-nic01-cff6ed40'

resource networkSecurityGroups_basicNsgvnet_eastus_nic01_name_resource 'Microsoft.Network/networkSecurityGroups@2024-05-01' = {
  name: networkSecurityGroups_basicNsgvnet_eastus_nic01_name
  location: 'eastus'
  properties: {
    securityRules: []
  }
}

resource publicIPAddresses_publicIp_vnet_eastus_nic01_cff6ed40_name_resource 'Microsoft.Network/publicIPAddresses@2024-05-01' = {
  name: publicIPAddresses_publicIp_vnet_eastus_nic01_cff6ed40_name
  location: 'eastus'
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    ipAddress: '4.157.243.56'
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 15
    ipTags: []
  }
}

resource virtualNetworks_vnet_eastus_name_resource 'Microsoft.Network/virtualNetworks@2024-05-01' = {
  name: virtualNetworks_vnet_eastus_name
  location: 'eastus'
  properties: {
    addressSpace: {
      addressPrefixes: [
        '172.16.0.0/16'
      ]
    }
    privateEndpointVNetPolicies: 'Disabled'
    subnets: [
      {
        name: 'snet-eastus-1'
        id: virtualNetworks_vnet_eastus_name_snet_eastus_1.id
        properties: {
          addressPrefixes: [
            '172.16.0.0/24'
          ]
          delegations: []
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
    ]
    virtualNetworkPeerings: []
    enableDdosProtection: false
  }
}

resource virtualMachineScaleSets_hunter_vmss_name_virtualMachineScaleSets_hunter_vmss_name_9bb46e8a 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines@2024-07-01' = {
  parent: virtualMachineScaleSets_hunter_vmss_name_resource
  name: '${virtualMachineScaleSets_hunter_vmss_name}_9bb46e8a'
  location: 'eastus'
  zones: [
    null
  ]
}

resource virtualNetworks_vnet_eastus_name_snet_eastus_1 'Microsoft.Network/virtualNetworks/subnets@2024-05-01' = {
  name: '${virtualNetworks_vnet_eastus_name}/snet-eastus-1'
  properties: {
    addressPrefixes: [
      '172.16.0.0/24'
    ]
    delegations: []
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_vnet_eastus_name_resource
  ]
}

resource virtualMachines_hunter_vmss_9bb46e8a_name_resource 'Microsoft.Compute/virtualMachines@2024-07-01' = {
  name: virtualMachines_hunter_vmss_9bb46e8a_name
  location: 'eastus'
  tags: {
    VirtualMachineProfileTimeCreated: '4/3/2025 7:34:39 PM +00:00'
  }
  properties: {
    hardwareProfile: {
      vmSize: 'Standard_B2s'
    }
    virtualMachineScaleSet: {
      id: virtualMachineScaleSets_hunter_vmss_name_resource.id
    }
    additionalCapabilities: {
      hibernationEnabled: false
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsServer'
        offer: 'WindowsServer'
        sku: '2022-datacenter-azure-edition'
        version: 'latest'
      }
      osDisk: {
        osType: 'Windows'
        name: '${virtualMachines_hunter_vmss_9bb46e8a_name}_disk1_c1ceb79ed576494fb435c26224a1e2d0'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        managedDisk: {
          storageAccountType: 'Premium_LRS'
          id: resourceId(
            'Microsoft.Compute/disks',
            '${virtualMachines_hunter_vmss_9bb46e8a_name}_disk1_c1ceb79ed576494fb435c26224a1e2d0'
          )
        }
        deleteOption: 'Delete'
        diskSizeGB: 127
      }
      dataDisks: []
      diskControllerType: 'SCSI'
    }
    osProfile: {
      computerName: 'hunter-vmRTTJLI'
      adminUsername: 'hunter'
      windowsConfiguration: {
        provisionVMAgent: true
        enableAutomaticUpdates: true
        patchSettings: {
          patchMode: 'AutomaticByOS'
          assessmentMode: 'ImageDefault'
          enableHotpatching: false
        }
      }
      secrets: []
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
    }
    securityProfile: {
      uefiSettings: {
        secureBootEnabled: true
        vTpmEnabled: true
      }
      securityType: 'TrustedLaunch'
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: networkInterfaces_vnet_eastus_nic01_cff6ed40_name_resource.id
          properties: {
            primary: true
            deleteOption: 'Delete'
          }
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
  }
}

resource virtualMachineScaleSets_hunter_vmss_name_resource 'Microsoft.Compute/virtualMachineScaleSets@2024-07-01' = {
  name: virtualMachineScaleSets_hunter_vmss_name
  location: 'eastus'
  sku: {
    name: 'Standard_B2s'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    singlePlacementGroup: false
    orchestrationMode: 'Flexible'
    upgradePolicy: {
      mode: 'Manual'
    }
    scaleInPolicy: {
      rules: [
        'Default'
      ]
      forceDeletion: false
    }
    virtualMachineProfile: {
      osProfile: {
        computerNamePrefix: 'hunter-vm'
        adminUsername: 'hunter'
        windowsConfiguration: {
          provisionVMAgent: true
          enableAutomaticUpdates: true
          patchSettings: {
            patchMode: 'AutomaticByOS'
            assessmentMode: 'ImageDefault'
            enableHotpatching: false
          }
        }
        secrets: []
        allowExtensionOperations: true
        requireGuestProvisionSignal: true
      }
      storageProfile: {
        osDisk: {
          osType: 'Windows'
          createOption: 'FromImage'
          caching: 'ReadWrite'
          managedDisk: {
            storageAccountType: 'Premium_LRS'
          }
          deleteOption: 'Delete'
          diskSizeGB: 127
        }
        imageReference: {
          publisher: 'MicrosoftWindowsServer'
          offer: 'WindowsServer'
          sku: '2022-datacenter-azure-edition'
          version: 'latest'
        }
        diskControllerType: 'SCSI'
      }
      networkProfile: {
        networkApiVersion: '2020-11-01'
        networkInterfaceConfigurations: [
          {
            name: 'vnet-eastus-nic01'
            properties: {
              primary: true
              enableAcceleratedNetworking: false
              disableTcpStateTracking: false
              enableIPForwarding: false
              auxiliaryMode: 'None'
              auxiliarySku: 'None'
              deleteOption: 'Delete'
              ipConfigurations: [
                {
                  name: 'vnet-eastus-nic01-defaultIpConfiguration'
                  properties: {
                    privateIPAddressVersion: 'IPv4'
                    subnet: {
                      id: virtualNetworks_vnet_eastus_name_snet_eastus_1.id
                    }
                    primary: true
                    publicIPAddressConfiguration: {
                      name: 'publicIp-vnet-eastus-nic01'
                      properties: {
                        idleTimeoutInMinutes: 15
                        ipTags: []
                        publicIPAddressVersion: 'IPv4'
                      }
                    }
                    applicationSecurityGroups: []
                    loadBalancerBackendAddressPools: []
                    applicationGatewayBackendAddressPools: []
                  }
                }
              ]
              networkSecurityGroup: {
                id: networkSecurityGroups_basicNsgvnet_eastus_nic01_name_resource.id
              }
              dnsSettings: {
                dnsServers: []
              }
            }
          }
        ]
      }
      diagnosticsProfile: {
        bootDiagnostics: {
          enabled: true
        }
      }
      extensionProfile: {
        extensions: []
      }
      securityProfile: {
        uefiSettings: {
          secureBootEnabled: true
          vTpmEnabled: true
        }
        securityType: 'TrustedLaunch'
      }
    }
    additionalCapabilities: {
      hibernationEnabled: false
    }
    platformFaultDomainCount: 1
    constrainedMaximumCapacity: false
  }
}

resource networkInterfaces_vnet_eastus_nic01_cff6ed40_name_resource 'Microsoft.Network/networkInterfaces@2024-05-01' = {
  name: networkInterfaces_vnet_eastus_nic01_cff6ed40_name
  location: 'eastus'
  tags: {
    fastpathenabled: 'True'
  }
  kind: 'Regular'
  properties: {
    ipConfigurations: [
      {
        name: 'vnet-eastus-nic01-defaultIpConfiguration'
        id: '${networkInterfaces_vnet_eastus_nic01_cff6ed40_name_resource.id}/ipConfigurations/vnet-eastus-nic01-defaultIpConfiguration'
        type: 'Microsoft.Network/networkInterfaces/ipConfigurations'
        properties: {
          privateIPAddress: '172.16.0.4'
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIPAddresses_publicIp_vnet_eastus_nic01_cff6ed40_name_resource.id
            properties: {
              deleteOption: 'Delete'
            }
          }
          subnet: {
            id: virtualNetworks_vnet_eastus_name_snet_eastus_1.id
          }
          primary: true
          privateIPAddressVersion: 'IPv4'
        }
      }
    ]
    dnsSettings: {
      dnsServers: []
    }
    enableAcceleratedNetworking: false
    enableIPForwarding: false
    disableTcpStateTracking: false
    networkSecurityGroup: {
      id: networkSecurityGroups_basicNsgvnet_eastus_nic01_name_resource.id
    }
    nicType: 'Standard'
    auxiliaryMode: 'None'
    auxiliarySku: 'None'
  }
}
