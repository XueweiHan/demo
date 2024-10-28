param project_name string = 'demo7'

param location string = resourceGroup().location

param attestation_provider_name string = replace(toLower('${project_name}Attest'), '-', '')

resource attestation_provider_resource 'Microsoft.Attestation/attestationProviders@2021-06-01' = {
  name: attestation_provider_name
  location: location
  properties: {}
}
