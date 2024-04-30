


resource uai 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'uai'
  location: resourceGroup().location
}

resource uai2 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'uai2'
  location: resourceGroup().location
}

resource uai3 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'uai3'
  location: resourceGroup().location
}

var uaiList = [
     uai.id
     uai2.id 
     uai3.id 
]

resource vm 'Microsoft.Compute/virtualMachines@2023-09-01' = {
  name: 'vm'
  location: resourceGroup().location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: union(uaiList, [uai.id])
  }
}
