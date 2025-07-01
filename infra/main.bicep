// Main Bicep template for ProDialer
targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment which is used to generate a short unique hash for resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string

// Optional parameters
@description('The name of the Communication Services resource')
param communicationServiceName string = ''

@description('The name of the SQL Server')
param sqlServerName string = ''

@description('The name of the SQL Database')
param sqlDatabaseName string = ''

@description('The name of the Storage Account')
param storageAccountName string = ''

@description('The name of the Function App')
param functionAppName string = ''

@description('The name of the Static Web App')
param staticWebAppName string = ''

@description('The name of the Application Insights resource')
param applicationInsightsName string = ''

@description('The name of the Log Analytics Workspace')
param logAnalyticsName string = ''

// Variables
var abbrs = loadJsonContent('abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }

// Create resources
module monitoring './core/monitor/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    tags: tags
    logAnalyticsName: !empty(logAnalyticsName) ? logAnalyticsName : '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: !empty(applicationInsightsName) ? applicationInsightsName : '${abbrs.insightsComponents}${resourceToken}'
  }
}

module storage './core/storage/storage-account.bicep' = {
  name: 'storage'
  params: {
    location: location
    tags: tags
    name: !empty(storageAccountName) ? storageAccountName : '${abbrs.storageStorageAccounts}${resourceToken}'
  }
}

module database './core/database/sqlserver.bicep' = {
  name: 'database'
  params: {
    location: location
    tags: tags
    serverName: !empty(sqlServerName) ? sqlServerName : '${abbrs.sqlServers}${resourceToken}'
    databaseName: !empty(sqlDatabaseName) ? sqlDatabaseName : '${abbrs.sqlServersDatabases}prodialer'
    principalId: principalId
  }
}

module communication './core/communication/communication-service.bicep' = {
  name: 'communication'
  params: {
    location: location
    tags: tags
    name: !empty(communicationServiceName) ? communicationServiceName : '${abbrs.communicationCommunicationServices}${resourceToken}'
  }
}

module functionApp './core/host/functions.bicep' = {
  name: 'functions'
  params: {
    location: location
    tags: tags
    name: !empty(functionAppName) ? functionAppName : '${abbrs.webSitesFunctions}${resourceToken}'
    storageAccountName: storage.outputs.name
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    communicationServiceName: communication.outputs.name
    sqlServerName: database.outputs.serverName
    sqlDatabaseName: database.outputs.databaseName
  }
}

module staticWebApp './core/host/staticwebapp.bicep' = {
  name: 'staticwebapp'
  params: {
    location: location
    tags: tags
    name: !empty(staticWebAppName) ? staticWebAppName : '${abbrs.webStaticSites}${resourceToken}'
    functionAppName: functionApp.outputs.name
  }
}

// Set up RBAC permissions for managed identity
module roleAssignments './core/security/role-assignments.bicep' = {
  name: 'role-assignments'
  params: {
    functionAppPrincipalId: functionApp.outputs.identityPrincipalId
    storageAccountName: storage.outputs.name
    communicationServiceName: communication.outputs.name
  }
}

// Outputs
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_RESOURCE_GROUP_NAME string = resourceGroup().name

output AZURE_STORAGE_ACCOUNT_NAME string = storage.outputs.name
output AZURE_STORAGE_ACCOUNT_ID string = storage.outputs.id

output AZURE_SQL_SERVER_NAME string = database.outputs.serverName
output AZURE_SQL_DATABASE_NAME string = database.outputs.databaseName
output AZURE_SQL_CONNECTION_STRING string = database.outputs.connectionString

output AZURE_COMMUNICATION_SERVICE_NAME string = communication.outputs.name
output AZURE_COMMUNICATION_SERVICE_ENDPOINT string = communication.outputs.endpoint

output AZURE_FUNCTION_APP_NAME string = functionApp.outputs.name
output AZURE_FUNCTION_APP_URL string = functionApp.outputs.uri

output AZURE_STATIC_WEB_APP_NAME string = staticWebApp.outputs.name
output AZURE_STATIC_WEB_APP_URL string = staticWebApp.outputs.uri

output APPLICATION_INSIGHTS_NAME string = monitoring.outputs.applicationInsightsName
output APPLICATION_INSIGHTS_CONNECTION_STRING string = monitoring.outputs.applicationInsightsConnectionString
