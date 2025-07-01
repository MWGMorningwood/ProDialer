// Bicep template for setting up RBAC permissions for managed identity
targetScope = 'resourceGroup'

@description('The principal ID of the Function App managed identity')
param functionAppPrincipalId string

@description('The name of the Storage Account')
param storageAccountName string

@description('The name of the Communication Service')
param communicationServiceName string

// Reference existing resources
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource communicationService 'Microsoft.Communication/communicationServices@2023-04-01' existing = {
  name: communicationServiceName
}

// Storage Table Data Contributor role
resource storageTableDataContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' // Storage Table Data Contributor
}

// Communication Services Contributor role  
resource communicationServicesContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '8097ce4e-c95a-4d85-bbb0-4abdf77f824b' // Communication Services Contributor
}

// Assign Storage Table Data Contributor to Function App managed identity
resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, functionAppPrincipalId, storageTableDataContributorRole.id)
  scope: storageAccount
  properties: {
    roleDefinitionId: storageTableDataContributorRole.id
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Assign Communication Services Contributor to Function App managed identity
resource communicationRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(communicationService.id, functionAppPrincipalId, communicationServicesContributorRole.id)
  scope: communicationService
  properties: {
    roleDefinitionId: communicationServicesContributorRole.id
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Output role assignment IDs for verification
output storageRoleAssignmentId string = storageRoleAssignment.id
output communicationRoleAssignmentId string = communicationRoleAssignment.id
