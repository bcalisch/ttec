# Azure Deployment Guide

## Prerequisites

- Azure CLI installed and authenticated (`az login`)
- Azure subscription with Contributor access
- Docker installed locally
- GitHub repository with Actions enabled

## Infrastructure Setup

### 1. Resource Group

```bash
az group create --name rg-geoops --location eastus
```

### 2. Azure SQL Database

Create the SQL Server and database:

```bash
# Create SQL Server
az sql server create \
  --name geoops-sql-server \
  --resource-group rg-geoops \
  --location eastus \
  --admin-user sqladmin \
  --admin-password '<StrongPassword>'

# Create database
az sql db create \
  --resource-group rg-geoops \
  --server geoops-sql-server \
  --name GeoOps \
  --service-objective S0

# Allow Azure services to access the server
az sql server firewall-rule create \
  --resource-group rg-geoops \
  --server geoops-sql-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Allow your local IP (for migrations/debugging)
az sql server firewall-rule create \
  --resource-group rg-geoops \
  --server geoops-sql-server \
  --name AllowLocalDev \
  --start-ip-address <YOUR_IP> \
  --end-ip-address <YOUR_IP>
```

Connection string format for the API:

```
Server=tcp:geoops-sql-server.database.windows.net,1433;Database=GeoOps;User Id=sqladmin;Password=<StrongPassword>;Encrypt=true;TrustServerCertificate=false;
```

### 3. Azure Container Registry (ACR)

```bash
# Create registry
az acr create \
  --resource-group rg-geoops \
  --name geoopsregistry \
  --sku Basic

# Log in to registry
az acr login --name geoopsregistry

# Build and push backend image
docker build -t geoopsregistry.azurecr.io/geoops-api:latest ./backend
docker push geoopsregistry.azurecr.io/geoops-api:latest

# Build and push frontend image
docker build -t geoopsregistry.azurecr.io/geoops-frontend:latest ./frontend
docker push geoopsregistry.azurecr.io/geoops-frontend:latest
```

### 4. Azure Container Apps

```bash
# Install the Container Apps extension
az extension add --name containerapp --upgrade

# Register required providers
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights

# Create Container Apps environment
az containerapp env create \
  --name geoops-env \
  --resource-group rg-geoops \
  --location eastus

# Create API container app
az containerapp create \
  --name geoops-api \
  --resource-group rg-geoops \
  --environment geoops-env \
  --image geoopsregistry.azurecr.io/geoops-api:latest \
  --registry-server geoopsregistry.azurecr.io \
  --target-port 8080 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 3 \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__Default="Server=tcp:geoops-sql-server.database.windows.net,1433;Database=GeoOps;User Id=sqladmin;Password=<StrongPassword>;Encrypt=true;TrustServerCertificate=false;"

# Get the API FQDN
API_FQDN=$(az containerapp show \
  --name geoops-api \
  --resource-group rg-geoops \
  --query properties.configuration.ingress.fqdn -o tsv)

# Create Frontend container app
az containerapp create \
  --name geoops-frontend \
  --resource-group rg-geoops \
  --environment geoops-env \
  --image geoopsregistry.azurecr.io/geoops-frontend:latest \
  --registry-server geoopsregistry.azurecr.io \
  --target-port 80 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 3
```

**Note:** When deploying to Container Apps, update the frontend `nginx.conf` to proxy API requests to the API container app's internal FQDN instead of `http://api:8080`. Within a Container Apps environment, apps communicate via their internal URLs (e.g., `http://geoops-api.internal.<env-default-domain>`).

### 5. Azure Maps

Replace Leaflet/OpenStreetMap with Azure Maps for production:

```bash
# Create Azure Maps account
az maps account create \
  --name geoops-maps \
  --resource-group rg-geoops \
  --sku S1

# Get subscription key
az maps account keys list \
  --name geoops-maps \
  --resource-group rg-geoops
```

To integrate in the Angular frontend:

1. Install the Azure Maps Web SDK:
   ```bash
   npm install azure-maps-control azure-maps-drawing-tools
   ```

2. Add to `angular.json` styles and scripts:
   ```json
   "styles": ["node_modules/azure-maps-control/dist/atlas.min.css"],
   "scripts": []
   ```

3. Update environment configuration:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: '/api',
     azureMaps: {
       subscriptionKey: '<AZURE_MAPS_KEY>'
     }
   };
   ```

4. Replace Leaflet map component with Azure Maps:
   ```typescript
   import * as atlas from 'azure-maps-control';

   const map = new atlas.Map('map', {
     center: [-98.5, 39.8],
     zoom: 4,
     authOptions: {
       authType: atlas.AuthenticationType.subscriptionKey,
       subscriptionKey: environment.azureMaps.subscriptionKey
     }
   });
   ```

### 6. Application Insights

```bash
# Create Application Insights resource
az monitor app-insights component create \
  --app geoops-insights \
  --location eastus \
  --resource-group rg-geoops \
  --application-type web

# Get the connection string
az monitor app-insights component show \
  --app geoops-insights \
  --resource-group rg-geoops \
  --query connectionString -o tsv
```

Add the NuGet package to the backend:

```bash
dotnet add backend/Backend.Api.csproj package Microsoft.ApplicationInsights.AspNetCore
```

Register in `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

Set the connection string as an environment variable on the Container App:

```bash
az containerapp update \
  --name geoops-api \
  --resource-group rg-geoops \
  --set-env-vars \
    APPLICATIONINSIGHTS_CONNECTION_STRING="<connection-string>"
```

### 7. Azure Blob Storage

```bash
# Create storage account
az storage account create \
  --name geoopsstorage \
  --resource-group rg-geoops \
  --location eastus \
  --sku Standard_LRS

# Create a container for file uploads
az storage container create \
  --name uploads \
  --account-name geoopsstorage

# Get connection string
az storage account show-connection-string \
  --name geoopsstorage \
  --resource-group rg-geoops \
  --query connectionString -o tsv
```

Add the NuGet package to the backend:

```bash
dotnet add backend/Backend.Api.csproj package Azure.Storage.Blobs
```

Set the connection string as an environment variable on the Container App:

```bash
az containerapp update \
  --name geoops-api \
  --resource-group rg-geoops \
  --set-env-vars \
    AzureBlobStorage__ConnectionString="<connection-string>" \
    AzureBlobStorage__ContainerName="uploads"
```

## CI/CD with GitHub Actions

Extend the existing CI workflow to build, push, and deploy on merge to main:

```yaml
# Add to .github/workflows/ci.yml after existing jobs

  deploy:
    if: github.ref == 'refs/heads/main' && github.event_name == 'push'
    needs: [backend, frontend]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Log in to Azure
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Log in to ACR
        run: az acr login --name geoopsregistry

      - name: Build and push API image
        run: |
          docker build -t geoopsregistry.azurecr.io/geoops-api:${{ github.sha }} ./backend
          docker push geoopsregistry.azurecr.io/geoops-api:${{ github.sha }}

      - name: Build and push Frontend image
        run: |
          docker build -t geoopsregistry.azurecr.io/geoops-frontend:${{ github.sha }} ./frontend
          docker push geoopsregistry.azurecr.io/geoops-frontend:${{ github.sha }}

      - name: Deploy API to Container Apps
        uses: azure/container-apps-deploy-action@v1
        with:
          resourceGroup: rg-geoops
          containerAppName: geoops-api
          imageToDeploy: geoopsregistry.azurecr.io/geoops-api:${{ github.sha }}

      - name: Deploy Frontend to Container Apps
        uses: azure/container-apps-deploy-action@v1
        with:
          resourceGroup: rg-geoops
          containerAppName: geoops-frontend
          imageToDeploy: geoopsregistry.azurecr.io/geoops-frontend:${{ github.sha }}
```

### Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `AZURE_CREDENTIALS` | Service principal JSON for Azure login |

Create the service principal:

```bash
az ad sp create-for-rbac \
  --name geoops-github-actions \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/rg-geoops \
  --json-auth
```

Copy the JSON output and add it as the `AZURE_CREDENTIALS` secret in your GitHub repository settings.
