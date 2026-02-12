@description('SQL Server name')
param serverName string

@description('Azure region')
param location string = resourceGroup().location

@description('SQL admin username')
param adminLogin string = 'sqladmin'

@description('SQL admin password')
@secure()
param adminPassword string

@description('Database name')
param databaseName string = 'ttecdb'

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: adminLogin
    administratorLoginPassword: adminPassword
    version: '12.0'
  }
}

// Allow Azure services to access
resource firewallAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    maxSizeBytes: 2147483648 // 2 GB
  }
}

output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = databaseName
output adminLogin string = adminLogin
