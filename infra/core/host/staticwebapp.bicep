@description('The name of the Static Web App')
param name string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Tags to apply to all resources')
param tags object = {}

@description('The name of the Function App for API integration')
param functionAppName string

@description('SKU for the Static Web App')
@allowed(['Free', 'Standard'])
param sku string = 'Free'

resource functionApp 'Microsoft.Web/sites@2023-01-01' existing = {
  name: functionAppName
}

resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: name
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  sku: {
    name: sku
    tier: sku
  }
  properties: {
    repositoryUrl: ''
    branch: ''
    buildProperties: {
      appLocation: 'src/ProDialer.Web'
      apiLocation: ''
      outputLocation: 'wwwroot'
    }
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    provider: 'None'
    enterpriseGradeCdnStatus: 'Disabled'
  }
}

// Link the Static Web App to the Function App for API calls
resource staticWebAppConfig 'Microsoft.Web/staticSites/config@2023-01-01' = {
  parent: staticWebApp
  name: 'appsettings'
  properties: {
    API_BASE_URL: functionApp.properties.defaultHostName
  }
}

output id string = staticWebApp.id
output name string = staticWebApp.name
output uri string = 'https://${staticWebApp.properties.defaultHostname}'
