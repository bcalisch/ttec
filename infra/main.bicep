targetScope = 'resourceGroup'

@description('Environment name (dev, prod)')
param environmentName string = 'dev'

@description('Azure region')
param location string = resourceGroup().location

@description('SQL admin password')
@secure()
param sqlAdminPassword string

@description('JWT signing key for the API')
@secure()
param jwtSigningKey string

// Short unique suffix for globally unique names
var suffix = substring(uniqueString(resourceGroup().id), 0, 8)
var acrName = 'ttec${suffix}'

// ─── Container Registry ───
module acr 'modules/container-registry.bicep' = {
  name: 'acr-deployment'
  params: {
    name: acrName
    location: location
  }
}

// ─── SQL Server + Database ───
module sql 'modules/sql-server.bicep' = {
  name: 'sql-deployment'
  params: {
    serverName: 'ttec-sql-${suffix}'
    location: location
    adminPassword: sqlAdminPassword
    databaseName: 'ttecdb'
  }
}

// Build connection string from SQL outputs + secure password
var sqlConnectionString = 'Server=tcp:${sql.outputs.serverFqdn},1433;Database=${sql.outputs.databaseName};User Id=${sql.outputs.adminLogin};Password=${sqlAdminPassword};Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;'

// ─── Container Apps Environment ───
module containerEnv 'modules/container-apps-env.bicep' = {
  name: 'container-env-deployment'
  params: {
    name: 'ttec-env-${environmentName}'
    location: location
  }
}

// Predictable FQDNs for cross-app configuration
var envDomain = containerEnv.outputs.defaultDomain
var geoopsApiFqdn = 'geoops-api.${envDomain}'
var geoopsFrontendFqdn = 'geoops-frontend.${envDomain}'
var ticketingApiFqdn = 'ticketing-api.${envDomain}'
var ticketingFrontendFqdn = 'ticketing-frontend.${envDomain}'

// ─── GeoOps API ───
module geoopsApi 'modules/container-app.bicep' = {
  name: 'geoops-api-deployment'
  params: {
    name: 'geoops-api'
    environmentId: containerEnv.outputs.environmentId
    location: location
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.adminUsername
    registryPassword: acr.outputs.adminPassword
    imageName: 'geoops-api'
    imageTag: 'latest'
    targetPort: 8080
    minReplicas: 0
    maxReplicas: 1
    envVars: [
      { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
      { name: 'ConnectionStrings__Default', value: sqlConnectionString }
      { name: 'Jwt__SigningKey', value: jwtSigningKey }
      { name: 'Jwt__Issuer', value: 'ttec-${environmentName}' }
      { name: 'Jwt__Audience', value: 'ttec-spa' }
      { name: 'Cors__AllowedOrigins__0', value: 'https://${geoopsFrontendFqdn}' }
    ]
  }
}

// ─── GeoOps Frontend ───
module geoopsFrontend 'modules/container-app.bicep' = {
  name: 'geoops-frontend-deployment'
  params: {
    name: 'geoops-frontend'
    environmentId: containerEnv.outputs.environmentId
    location: location
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.adminUsername
    registryPassword: acr.outputs.adminPassword
    imageName: 'geoops-frontend'
    imageTag: 'latest'
    targetPort: 80
    minReplicas: 0
    maxReplicas: 1
    envVars: [
      { name: 'API_URL', value: 'http://geoops-api' }
      { name: 'PUBLIC_API_URL', value: 'https://${geoopsApiFqdn}' }
      { name: 'TICKETING_APP_URL', value: 'https://${ticketingFrontendFqdn}' }
    ]
  }
}

// ─── Ticketing API ───
module ticketingApi 'modules/container-app.bicep' = {
  name: 'ticketing-api-deployment'
  params: {
    name: 'ticketing-api'
    environmentId: containerEnv.outputs.environmentId
    location: location
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.adminUsername
    registryPassword: acr.outputs.adminPassword
    imageName: 'ticketing-api'
    imageTag: 'latest'
    targetPort: 8080
    minReplicas: 0
    maxReplicas: 1
    envVars: [
      { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
      { name: 'ConnectionStrings__Default', value: sqlConnectionString }
      { name: 'Jwt__SigningKey', value: jwtSigningKey }
      { name: 'Jwt__Issuer', value: 'ttec-${environmentName}' }
      { name: 'Jwt__Audience', value: 'ttec-spa' }
      { name: 'Cors__AllowedOrigins__0', value: 'https://${ticketingFrontendFqdn}' }
    ]
  }
}

// ─── Ticketing Frontend ───
module ticketingFrontend 'modules/container-app.bicep' = {
  name: 'ticketing-frontend-deployment'
  params: {
    name: 'ticketing-frontend'
    environmentId: containerEnv.outputs.environmentId
    location: location
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.adminUsername
    registryPassword: acr.outputs.adminPassword
    imageName: 'ticketing-frontend'
    imageTag: 'latest'
    targetPort: 80
    minReplicas: 0
    maxReplicas: 1
    envVars: [
      { name: 'BACKEND_HOST', value: 'ticketing-api' }
      { name: 'PUBLIC_API_URL', value: 'https://${ticketingApiFqdn}' }
      { name: 'GEOOPS_APP_URL', value: 'https://${geoopsFrontendFqdn}' }
    ]
  }
}

// ─── Landing Page ───
module landing 'modules/container-app.bicep' = {
  name: 'landing-deployment'
  params: {
    name: 'landing'
    environmentId: containerEnv.outputs.environmentId
    location: location
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.adminUsername
    registryPassword: acr.outputs.adminPassword
    imageName: 'landing'
    imageTag: 'latest'
    targetPort: 80
    minReplicas: 0
    maxReplicas: 1
    envVars: [
      { name: 'GEOOPS_URL', value: 'https://${geoopsFrontendFqdn}' }
      { name: 'TICKETING_URL', value: 'https://${ticketingFrontendFqdn}' }
    ]
  }
}

// ─── Outputs ───
output acrLoginServer string = acr.outputs.loginServer
output acrName string = acr.outputs.name
output geoopsApiFqdn string = geoopsApi.outputs.fqdn
output geoopsFrontendFqdn string = geoopsFrontend.outputs.fqdn
output ticketingApiFqdn string = ticketingApi.outputs.fqdn
output ticketingFrontendFqdn string = ticketingFrontend.outputs.fqdn
output landingFqdn string = landing.outputs.fqdn
output sqlServerFqdn string = sql.outputs.serverFqdn
