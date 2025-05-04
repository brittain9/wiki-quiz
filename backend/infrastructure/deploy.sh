#!/bin/bash

# Use different resource groups for production and testing
PROD_RESOURCE_GROUP="rg-wikiquiz-production"
TEST_RESOURCE_GROUP="rg-wikiquiz-test"
DEV_RESOURCE_GROUP="rg-wikiquiz-dev"

RESOURCE_GROUP="$DEV_RESOURCE_GROUP"  # Change to $TEST_RESOURCE_GROUP for testing
LOCATION="centralus"

# --- Start: Reading secrets from secrets.txt ---
SECRETS_FILE="secrets.txt"

# Check if secrets file exists
if [ ! -f "$SECRETS_FILE" ]; then
  echo "Error: Secrets file '$SECRETS_FILE' not found!"
  echo "Please create it and add your secrets on separate lines."
  exit 1
fi

# Read secrets using a more compatible approach
# Read first line (PostgreSQL password)
POSTGRES_PASSWORD=$(sed -n '1p' "$SECRETS_FILE")
# Read second line (OpenAI API key)
OPENAI_API_KEY=$(sed -n '2p' "$SECRETS_FILE")
# Read third line (Google Client ID)
GOOGLE_CLIENT_ID=$(sed -n '3p' "$SECRETS_FILE")
# Read fourth line (Google Client Secret)
GOOGLE_CLIENT_SECRET=$(sed -n '4p' "$SECRETS_FILE")
# Read fifth line (JWT Secret)
JWT_SECRET=$(sed -n '5p' "$SECRETS_FILE")

# Debug - check if password is empty or contains problematic characters
if [ -z "$POSTGRES_PASSWORD" ]; then
  echo "Error: PostgreSQL password is empty! Check your secrets.txt file."
  exit 1
fi

echo "Postgres password length: ${#POSTGRES_PASSWORD} characters"
echo "First character: ${POSTGRES_PASSWORD:0:1}"

# --- End: Reading secrets from secrets.txt ---

# Ensure the resource group exists (idempotent)
echo "Ensuring resource group '$RESOURCE_GROUP' exists in '$LOCATION'..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"

# Run the deployment
echo "Starting Bicep deployment..."
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file main.bicep \
  --parameters @main.parameters.json \
  --parameters \
    postgresAdminPassword="$POSTGRES_PASSWORD" \
    openAiApiKey="$OPENAI_API_KEY" \
    googleClientId="$GOOGLE_CLIENT_ID" \
    googleClientSecret="$GOOGLE_CLIENT_SECRET" \
    jwtSecret="$JWT_SECRET"

echo "Deployment script finished."