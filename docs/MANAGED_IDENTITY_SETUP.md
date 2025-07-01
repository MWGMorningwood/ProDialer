# Managed Identity Setup Guide

This document outlines the configuration required to use Azure Managed Identity for authentication across all Azure services in the ProDialer application.

## Overview

The application is configured to use Managed System Identity for:
- Azure SQL Database
- Azure Table Storage
- Azure Communication Services
- Azure Application Insights

## Local Development Configuration

For local development, update your `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Azure__StorageAccountName": "your-storage-account-name",
    "CommunicationService__Endpoint": "https://your-acs-resource.communication.azure.com/",
    "CommunicationService__CallerPhoneNumber": "+1234567890",
    "CommunicationService__CallbackBaseUri": "https://your-function-app.azurewebsites.net/api/"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=your-database;Encrypt=True;Connection Timeout=30;Authentication=Active Directory Default;",
    "Storage": "UseDevelopmentStorage=true"
  }
}
```

## Azure Production Configuration

### Application Settings
Configure these application settings in your Azure Function App:

- `Azure__StorageAccountName`: Name of your storage account
- `CommunicationService__Endpoint`: Your ACS endpoint URL
- `CommunicationService__CallerPhoneNumber`: Phone number for outbound calls
- `CommunicationService__CallbackBaseUri`: Base URL for webhooks

### Connection Strings
- `DefaultConnection`: SQL connection string with managed identity authentication

### Required Azure RBAC Permissions

#### For SQL Database
- Assign the Function App's managed identity as a user in the database
- Grant necessary database permissions (db_datareader, db_datawriter, etc.)

#### For Storage Account
- **Storage Table Data Contributor**: Required for Table Storage operations
- **Storage Blob Data Contributor**: If blob storage is used

#### For Communication Services
- **Contributor**: Required for call automation operations

### SQL Database Configuration

After deploying the infrastructure, you need to configure the SQL Database to allow the Function App's managed identity. Connect to your SQL Database and run:

```sql
-- Replace 'your-function-app-name' with your actual Function App name
CREATE USER [your-function-app-name] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [your-function-app-name];
ALTER ROLE db_datawriter ADD MEMBER [your-function-app-name];
ALTER ROLE db_ddladmin ADD MEMBER [your-function-app-name]; -- If Entity Framework needs to create/modify schema
```

### PowerShell Script to Configure Permissions

```powershell
# Variables
$subscriptionId = "your-subscription-id"
$resourceGroupName = "your-resource-group"
$functionAppName = "your-function-app"
$storageAccountName = "your-storage-account"
$communicationServiceName = "your-communication-service"
$sqlServerName = "your-sql-server"
$databaseName = "your-database"

# Get the Function App's managed identity
$functionApp = Get-AzWebApp -ResourceGroupName $resourceGroupName -Name $functionAppName
$principalId = $functionApp.Identity.PrincipalId

# Assign Storage Table Data Contributor role
$storageAccount = Get-AzStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName
New-AzRoleAssignment -ObjectId $principalId -RoleDefinitionName "Storage Table Data Contributor" -Scope $storageAccount.Id

# Assign Communication Services Contributor role
$communicationService = Get-AzResource -ResourceGroupName $resourceGroupName -Name $communicationServiceName -ResourceType "Microsoft.Communication/communicationServices"
New-AzRoleAssignment -ObjectId $principalId -RoleDefinitionName "Contributor" -Scope $communicationService.ResourceId

# For SQL Database, you need to connect to the database and run:
# CREATE USER [your-function-app-name] FROM EXTERNAL PROVIDER;
# ALTER ROLE db_datareader ADD MEMBER [your-function-app-name];
# ALTER ROLE db_datawriter ADD MEMBER [your-function-app-name];
```

## Authentication Flow

1. **Local Development**: Uses `DefaultAzureCredential` which attempts:
   - Environment variables
   - Managed Identity (if available)
   - Azure CLI credentials
   - Visual Studio credentials

2. **Azure Production**: Uses the Function App's system-assigned managed identity

## Troubleshooting

### Common Issues

1. **"Authentication failed" errors**
   - Verify managed identity is enabled on the Function App
   - Check RBAC role assignments
   - Ensure firewall rules allow Azure services

2. **Table Storage connection issues**
   - Verify storage account name is correct
   - Check Storage Table Data Contributor role assignment

3. **Communication Services authentication issues**
   - Verify endpoint URL format
   - Check Contributor role assignment on ACS resource

### Diagnostic Commands

```powershell
# Check managed identity status
Get-AzWebApp -ResourceGroupName $resourceGroupName -Name $functionAppName | Select-Object Identity

# List role assignments
Get-AzRoleAssignment -ObjectId $principalId

# Test Azure CLI authentication (for local dev)
az account show
```

## Security Benefits

- No connection strings or keys stored in application code
- Automatic token rotation
- Centralized access management through Azure RBAC
- Audit trail of authentication attempts
- Reduced risk of credential leakage
