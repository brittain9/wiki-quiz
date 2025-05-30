#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Deploy WikiQuiz infrastructure to Azure using Pulumi

.DESCRIPTION
    This script simplifies the deployment of WikiQuiz infrastructure across different environments.
    It handles stack selection, secret validation, and deployment.

.PARAMETER Environment
    The environment to deploy to (dev, tst, prd)

.PARAMETER Action
    The action to perform (preview, deploy, destroy, outputs)

.PARAMETER SetSecrets
    Switch to prompt for and set required secrets

.EXAMPLE
    ./deploy.ps1 -Environment dev -Action preview
    ./deploy.ps1 -Environment prd -Action deploy -SetSecrets
    ./deploy.ps1 -Environment tst -Action outputs
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "tst", "prd")]
    [string]$Environment,
    
    [Parameter(Mandatory = $true)]
    [ValidateSet("preview", "deploy", "destroy", "outputs", "config")]
    [string]$Action,
    
    [switch]$SetSecrets
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = $Reset)
    Write-Host "$Color$Message$Reset"
}

function Test-PulumiLogin {
    try {
        $null = pulumi whoami
        return $true
    }
    catch {
        return $false
    }
}

function Test-AzureLogin {
    try {
        $null = az account show
        return $true
    }
    catch {
        return $false
    }
}

function Set-RequiredSecrets {
    param([string]$Environment)
    
    Write-ColorOutput "Setting secrets for $Environment environment..." $Blue
    
    $secrets = @(
        "postgresAdminPassword",
        "openAiApiKey", 
        "googleClientId",
        "googleClientSecret",
        "jwtSecret"
    )
    
    foreach ($secret in $secrets) {
        $currentValue = ""
        try {
            $currentValue = pulumi config get $secret --show-secrets 2>$null
        }
        catch {
            # Secret doesn't exist
        }
        
        if ([string]::IsNullOrEmpty($currentValue)) {
            $secureValue = Read-Host "Enter value for $secret" -AsSecureString
            $plainValue = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureValue))
            pulumi config set --secret $secret $plainValue
            Write-ColorOutput "✓ Set $secret" $Green
        }
        else {
            Write-ColorOutput "✓ $secret already configured" $Yellow
        }
    }
}

function Invoke-PulumiAction {
    param([string]$Action, [string]$Environment)
    
    switch ($Action) {
        "preview" {
            Write-ColorOutput "Previewing changes for $Environment..." $Blue
            pulumi preview
        }
        "deploy" {
            Write-ColorOutput "Deploying to $Environment..." $Blue
            pulumi up --yes
        }
        "destroy" {
            Write-ColorOutput "WARNING: This will destroy all resources in $Environment!" $Red
            $confirm = Read-Host "Type 'yes' to confirm destruction"
            if ($confirm -eq "yes") {
                pulumi destroy --yes
            }
            else {
                Write-ColorOutput "Destruction cancelled." $Yellow
            }
        }
        "outputs" {
            Write-ColorOutput "Stack outputs for $Environment:" $Blue
            pulumi stack output
        }
        "config" {
            Write-ColorOutput "Stack configuration for $Environment:" $Blue
            pulumi config
        }
    }
}

# Main execution
try {
    Write-ColorOutput "🚀 WikiQuiz Infrastructure Deployment" $Blue
    Write-ColorOutput "Environment: $Environment" $Green
    Write-ColorOutput "Action: $Action" $Green
    Write-Host ""

    # Check prerequisites
    Write-ColorOutput "Checking prerequisites..." $Blue
    
    if (-not (Get-Command pulumi -ErrorAction SilentlyContinue)) {
        throw "Pulumi CLI not found. Please install Pulumi: https://www.pulumi.com/docs/get-started/install/"
    }
    
    if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
        throw "Azure CLI not found. Please install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    }
    
    if (-not (Test-AzureLogin)) {
        Write-ColorOutput "Not logged into Azure. Please run 'az login'" $Red
        exit 1
    }
    
    if (-not (Test-PulumiLogin)) {
        Write-ColorOutput "Not logged into Pulumi. Please run 'pulumi login'" $Red
        exit 1
    }
    
    Write-ColorOutput "✓ Prerequisites check passed" $Green
    Write-Host ""
    
    # Select stack
    Write-ColorOutput "Selecting stack: $Environment" $Blue
    try {
        pulumi stack select $Environment
    }
    catch {
        Write-ColorOutput "Stack $Environment not found. Creating..." $Yellow
        pulumi stack init $Environment
        pulumi stack select $Environment
    }
    
    # Set secrets if requested
    if ($SetSecrets) {
        Set-RequiredSecrets -Environment $Environment
        Write-Host ""
    }
    
    # Validate required secrets exist
    Write-ColorOutput "Validating configuration..." $Blue
    $requiredSecrets = @("postgresAdminPassword", "openAiApiKey", "googleClientId", "googleClientSecret", "jwtSecret")
    $missingSecrets = @()
    
    foreach ($secret in $requiredSecrets) {
        try {
            $value = pulumi config get $secret 2>$null
            if ([string]::IsNullOrEmpty($value)) {
                $missingSecrets += $secret
            }
        }
        catch {
            $missingSecrets += $secret
        }
    }
    
    if ($missingSecrets.Count -gt 0) {
        Write-ColorOutput "Missing required secrets: $($missingSecrets -join ', ')" $Red
        Write-ColorOutput "Run with -SetSecrets flag to configure them." $Yellow
        exit 1
    }
    
    Write-ColorOutput "✓ Configuration validation passed" $Green
    Write-Host ""
    
    # Execute action
    Invoke-PulumiAction -Action $Action -Environment $Environment
    
    if ($Action -eq "deploy") {
        Write-Host ""
        Write-ColorOutput "🎉 Deployment completed successfully!" $Green
        Write-ColorOutput "View outputs with: ./deploy.ps1 -Environment $Environment -Action outputs" $Blue
        
        # Show key outputs
        Write-Host ""
        Write-ColorOutput "Key outputs:" $Blue
        try {
            $containerUrl = pulumi stack output containerAppUrl
            $acrServer = pulumi stack output acrLoginServer
            $rgName = pulumi stack output resourceGroupName
            
            Write-ColorOutput "Container App URL: $containerUrl" $Green
            Write-ColorOutput "ACR Login Server: $acrServer" $Green  
            Write-ColorOutput "Resource Group: $rgName" $Green
        }
        catch {
            Write-ColorOutput "Could not retrieve outputs. Run 'pulumi stack output' manually." $Yellow
        }
    }
}
catch {
    Write-ColorOutput "❌ Error: $($_.Exception.Message)" $Red
    exit 1
} 