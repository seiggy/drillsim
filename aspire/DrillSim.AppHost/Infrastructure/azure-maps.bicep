targetScope = 'resourceGroup'

// Aspire supplies location to every custom template; Maps uses the global service location.
#disable-next-line no-unused-params
param location string

@description('Stable seed for the Maps account name within the Aspire-managed resource group.')
@minLength(1)
param nameSeed string = 'maps'

@description('Exact browser origin, including scheme and port, without a path or wildcard.')
@minLength(1)
param browserOrigin string

@description('Current developer principal ID, supplied by Aspire during local provisioning.')
@minLength(36)
@maxLength(36)
param principalId string

@description('Principal type supplied by Aspire for the Maps Data Reader assignment.')
@allowed([
  'User'
  'ServicePrincipal'
])
param principalType string

var mapsAccountName = 'maps-${uniqueString(resourceGroup().id, nameSeed)}'
var allowedBrowserOrigin = contains(browserOrigin, '*')
  ? fail('browserOrigin must be an exact origin without wildcards.')
  : browserOrigin

// Azure Maps Data Reader: https://learn.microsoft.com/azure/role-based-access-control/built-in-roles/web-and-mobile#azure-maps-data-reader
var mapsDataReaderRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '423170ca-a8f6-4b0f-8487-9e4eb8f49bfa'
)

resource mapsAccount 'Microsoft.Maps/accounts@2023-06-01' = {
  name: mapsAccountName
  location: 'global'
  kind: 'Gen2'
  sku: {
    name: 'G2'
  }
  properties: {
    disableLocalAuth: true
    cors: {
      corsRules: [
        {
          allowedOrigins: [
            allowedBrowserOrigin
          ]
        }
      ]
    }
  }
}

resource developerDataReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(mapsAccount.id, principalId, mapsDataReaderRoleId)
  scope: mapsAccount
  properties: {
    roleDefinitionId: mapsDataReaderRoleId
    principalId: principalId
    principalType: principalType
  }
}

output id string = mapsAccount.id
output name string = mapsAccount.name
output clientId string = mapsAccount.properties.uniqueId
output tenantId string = subscription().tenantId
output subscriptionId string = subscription().subscriptionId
