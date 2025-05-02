#!/bin/bash

# Use different resource groups for production and testing
PROD_RESOURCE_GROUP="rg-wikiquiz-production"
TEST_RESOURCE_GROUP="rg-wikiquiz-test"
RESOURCE_GROUP="$PROD_RESOURCE_GROUP"  # Change to $TEST_RESOURCE_GROUP for testing
LOCATION="centralus"

# --- Start: Reading secrets from secrets.txt ---
SECRETS_FILE="secrets.txt"

# Check if secrets file exists
if [ ! -f "$SECRETS_FILE" ]; then
  echo "Error: Secrets file '$SECRETS_FILE' not found!"
  echo "Please create it and add your secrets on separate lines."
  exit 1
fi

# Read secrets into an array
mapfile -t SECRETS < "$SECRETS_FILE"

# Assign array elements to variables
# Ensure the file has at least 4 lines for this to work correctly
POSTGRES_PASSWORD="${SECRETS[0]}"
OPENAI_API_KEY="${SECRETS[1]}"
GOOGLE_CLIENT_ID="${SECRETS[2]}"
GOOGLE_CLIENT_SECRET="${SECRETS[3]}"

# --- End: Reading secrets from secrets.txt ---

# Ensure the resource group exists (idempotent)
echo "Ensuring resource group '$RESOURCE_GROUP' exists in '$LOCATION'..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"

# Run the deployment
echo "Starting Bicep deployment..."
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file main.bicep \
  --parameters \
    postgresAdminPassword="$POSTGRES_PASSWORD" \
    openAiApiKey="$OPENAI_API_KEY" \
    googleClientId="$GOOGLE_CLIENT_ID" \
    googleClientSecret="$GOOGLE_CLIENT_SECRET"

echo "Deployment script finished."