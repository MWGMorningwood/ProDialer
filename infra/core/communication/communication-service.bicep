@description('The name of the Communication Services resource')
param name string

@description('Tags to apply to all resources')
param tags object = {}

@description('The data location for the Communication Services resource')
@allowed(['Africa', 'Asia Pacific', 'Australia', 'Brazil', 'Canada', 'Europe', 'France', 'Germany', 'India', 'Japan', 'Korea', 'Norway', 'Switzerland', 'UAE', 'UK', 'United States'])
param dataLocation string = 'United States'

resource communicationService 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: name
  location: 'global'
  tags: tags
  properties: {
    dataLocation: dataLocation
    linkedDomains: []
  }
}

output id string = communicationService.id
output name string = communicationService.name
output endpoint string = communicationService.properties.hostName
output immutableResourceId string = communicationService.properties.immutableResourceId
output dataLocation string = communicationService.properties.dataLocation
