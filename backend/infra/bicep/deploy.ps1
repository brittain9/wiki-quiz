$PROD_RESOURCE_GROUP = "rg-wikiquiz-production"
$TEST_RESOURCE_GROUP = "rg-wikiquiz-test"
$RESOURCE_GROUP = $PROD_RESOURCE_GROUP # Change to $TEST_RESOURCE_GROUP for testing
$LOCATION = "centralus"

# --- Start: Reading secrets from secrets.txt ---
$SECRETS_FILE = "secrets.txt"

# Check if secrets file exists
if (-not (Test-Path -Path $SECRETS_FILE -PathType Leaf)) {
  Write-Error "Error: Secrets file '$SECRETS_FILE' not found!"
  Write-Error "Please create it and add your secrets on separate lines."
  exit 1
}

# Read secrets into an array
$secrets = Get-Content -Path $SECRETS_FILE

# Validate that the file has enough lines
if ($secrets.Count -lt 5) {
    Write-Error "Error: Secrets file '$SECRETS_FILE' must contain at least 5 lines."
    Write-Error "Please ensure it has your PostgreSQL password, OpenAI API Key, Google Client ID, Google Client Secret, and JWT Secret on separate lines."
    exit 1
}

# Assign array elements to variables
# Ensure the file has at least 5 lines for this to work correctly
$POSTGRES_PASSWORD = $secrets[0]
$OPENAI_API_KEY = $secrets[1]
$GOOGLE_CLIENT_ID = $secrets[2]
$GOOGLE_CLIENT_SECRET = $secrets[3]
$JWT_SECRET = $secrets[4]

# --- End: Reading secrets from secrets.txt ---

# Ensure the resource group exists (idempotent)
Write-Host "Ensuring resource group '$RESOURCE_GROUP' exists in '$LOCATION'..."
az group create --name $RESOURCE_GROUP --location $LOCATION

# Define deployment parameters using a hashtable for splatting
# This makes the command cleaner, especially with many parameters
$DeploymentParameters = @{
  ResourceGroupName = $RESOURCE_GROUP
  TemplateFile      = "main.bicep"
  Parameters        = @{
    postgresAdminPassword = @{ value = $POSTGRES_PASSWORD }
    openAiApiKey          = @{ value = $OPENAI_API_KEY }
    googleClientId        = @{ value = $GOOGLE_CLIENT_ID }
    googleClientSecret    = @{ value = $GOOGLE_CLIENT_SECRET }
    jwtSecret             = @{ value = $JWT_SECRET }
  }
}

# Run the deployment using splatting (@ symbol)
Write-Host "Starting Bicep deployment..."
az deployment group create @DeploymentParameters

Write-Host "Deployment script finished."
