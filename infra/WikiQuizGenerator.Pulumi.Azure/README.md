# WikiQuiz Infrastructure (Pay-as-you-go, Scale-to-zero)

This infrastructure provisions a minimal-cost, usage-based backend using only production resources:

- Azure Container Apps for the API with scale-to-zero (consumption pricing)
- Azure Cosmos DB (Serverless) as the data store (pure pay-per-use)
- Log Analytics workspace for Container Apps logs
- Azure Application Insights for telemetry

You develop locally against the Cosmos Emulator; no hosted dev environment is provisioned.

## What gets created

- `Resource Group`: `rg-wikiquiz-prd`
- `Log Analytics Workspace`: Container Apps logging (low retention)
- `Container Apps Environment`: hosting for the API
- `Container App`: pulls image from GitHub Container Registry (GHCR), scales to zero
- `Cosmos DB Account`: Serverless (SQL/Core API), session consistency
- `Cosmos DB Database`: `WikiQuizDb`
- `Application Insights`: telemetry with connection string exposed to the app

## Configuration

Pulumi config keys (set per stack):

- `wikiquiz:location` (e.g., `Central US`)
- `wikiquiz:frontendUri` (CORS)
- `wikiquiz:ghcrUsername` (GitHub handle)
- `wikiquiz:ghcrPassword` (secret; token with `read:packages`)
- `wikiquiz:openAiApiKey` (secret)
- `wikiquiz:googleClientId` (secret)
- `wikiquiz:googleClientSecret` (secret)
- `wikiquiz:jwtSecret` (secret)

Example (production only):

```bash
pulumi stack select prd
pulumi config set wikiquiz:location "Central US"
pulumi config set wikiquiz:frontendUri "https://quiz.alexanderbrittain.com"
pulumi config set wikiquiz:ghcrUsername brittain9
pulumi config set --secret wikiquiz:ghcrPassword <GHCR_TOKEN_WITH_READ_PACKAGES>
pulumi config set --secret wikiquiz:openAiApiKey <OPENAI_KEY>
pulumi config set --secret wikiquiz:googleClientId <CLIENT_ID>
pulumi config set --secret wikiquiz:googleClientSecret <CLIENT_SECRET>
pulumi config set --secret wikiquiz:jwtSecret <JWT_SECRET>
```

## Image source

The API container is expected at:

```
ghcr.io/<ghcrUsername>/wiki-quiz:<environment>
```

Tag used: `ghcr.io/brittain9/wiki-quiz:prd`.

## Environment variables provided to the API

- `CosmosDb__ConnectionString` (secret)
- `CosmosDb__DatabaseName` = `WikiQuizDb`
- `CosmosDb__QuizContainerName` = `Quizzes`
- `CosmosDb__UserContainerName` = `Users`
- `APPLICATIONINSIGHTS_CONNECTION_STRING` (secret)
- `wikiquizapp__OpenAIApiKey`, `wikiquizapp__GoogleOAuthClientId`, `wikiquizapp__GoogleOAuthClientSecret`, `wikiquizapp__JwtSecret`
- `ASPNETCORE_ENVIRONMENT` = `Production`
- `ASPNETCORE_URLS` = `http://+:8080`

## Notes

- The Container App is configured with `MinReplicas = 0` and an HTTP scale rule, so it scales to zero when idle.
- Cosmos DB Free Tier is enabled at the account level and should be sufficient for initial production.
- Application Insights is provisioned; the app can opt-in to telemetry by reading `APPLICATIONINSIGHTS_CONNECTION_STRING`.
- All secrets are stored and injected by Pulumi.

## Deploy

```bash
pulumi preview
pulumi up
pulumi stack output
```

Key outputs:

- `ApiUrl`: Public endpoint of the API
- `ApplicationInsightsConnectionString`: App Insights connection string
- `LogAnalyticsWorkspaceId`: Workspace resource id

## Local development

- Use the Cosmos DB Emulator locally; no cloud dev infra is required.
- Build and push your API image to GHCR as part of your CI/CD; infra will pull latest tag you reference in config.
