# Testing the Azure Resource Naming

```bash
cd tests/WikiQuizGenerator.Pulumi.Azure.Tests
dotnet test
```

# Wikipedia Quiz Generator

This project generates high-quality quizzes based on nearly any topic in nine languages using Wikipedia and AI.

## 🚀 Development Setup (Local)

This is the fastest way to get started for local development and testing.

### Requirements

- [Docker](https://www.docker.com/)
- OpenAI API key

### Steps

1. **Clone the repository**
2. **Create a `.env` file** in the `backend` directory. Use the provided `.env.example` as a template and fill in your API keys and other required values.
3. **Build and run the project with Docker Compose:**
   ```bash
   docker-compose up --build
   ```
4. **Access the API:**
   - Open [http://localhost:5543/swagger](http://localhost:5543/swagger) to test the API.

---

## ☁️ Production/Cloud Deployment (Pulumi + Azure)

For production or cloud deployment, use the Pulumi infrastructure-as-code project to provision all required Azure resources and deploy the app securely.

### Prerequisites

- [Pulumi CLI](https://www.pulumi.com/docs/get-started/install/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- .NET SDK 6.0 or later
- Azure Subscription

### Steps

1. **Login to Azure and Pulumi**
   ```bash
   az login
   pulumi login # or pulumi login --local
   ```
2. **Navigate to the infrastructure directory:**
   ```bash
   cd infra
   ```
3. **Create and select a Pulumi stack:**
   ```bash
   pulumi stack init dev # or prod, test, etc.
   pulumi stack select dev
   ```
4. **Configure the stack:**
   ```bash
   pulumi config set azure-native:location eastus
   pulumi config set projectName wikiquizapi
   pulumi config set environmentName Development
   pulumi config set containerImage <your_acr_image>
   pulumi config set containerPort 8080
   pulumi config set postgresAdminLogin pgadmin
   pulumi config set postgresDatabaseName WikiQuizGenerator
   pulumi config set frontendUrl http://yoururl.com
   pulumi config set jwtIssuer https://yoururl.com/
   pulumi config set jwtAudience https://yoururl.net
   # Set secrets (these will be encrypted)
   pulumi config set --secret postgresAdminPassword <your_password>
   pulumi config set --secret openAiApiKey <your_openai_key>
   pulumi config set --secret googleClientId <your_google_client_id>
   pulumi config set --secret googleClientSecret <your_google_client_secret>
   pulumi config set --secret jwtSecret <your_jwt_secret>
   ```
5. **Preview and deploy:**
   ```bash
   pulumi preview
   pulumi up
   ```
6. **Access resource outputs:**
   ```bash
   pulumi stack output
   ```

### Managing Environments

- Use separate Pulumi stacks for dev, test, and prod.
- Each stack has its own config and state.

### Updating or Destroying Infrastructure

- To update: `pulumi up`
- To destroy: `pulumi destroy` (⚠️ this deletes all resources)
