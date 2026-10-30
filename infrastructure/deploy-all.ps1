# MudakAI - Deploy All Services to k3s
# Orchestrates deployment in dependency order

param(
    [switch]$Force,
    [switch]$SkipBuild,
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/shared/scripts/Deploy-Common.psm1" -Force

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MudakAI Full Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Validate all required environment variables upfront
Assert-EnvironmentVariables -Variables @(
    "TF_VAR_registry_password",
    "TF_VAR_discord_bot_token",
    "TF_VAR_openai_api_key",
    "TF_VAR_azure_storage_connection_string",
    "TF_VAR_azure_service_bus_connection_string"
)

$deployArgs = @{}
if ($Force) { $deployArgs["Force"] = $true }
if ($SkipBuild) { $deployArgs["SkipBuild"] = $true }
if ($AutoApprove) { $deployArgs["AutoApprove"] = $true }

# Set Kubernetes context
Set-KubernetesContext -Context "t3-k3s"

# Create mudakai namespace (idempotent)
Write-Host "`nEnsuring mudakai namespace exists..." -ForegroundColor Cyan
kubectl create namespace mudakai --dry-run=client -o yaml | kubectl apply -f -

# Create registry pull secret (idempotent)
Ensure-RegistrySecret -Namespace "mudakai" -Password $env:TF_VAR_registry_password

# 1. Dapr (for service-to-service invocation)
Write-Host "`n>>> Step 1/5: Dapr <<<" -ForegroundColor Magenta
& "$PSScriptRoot/dapr/deploy.ps1" @deployArgs

# 2. TTS API (.NET orchestration)
Write-Host "`n>>> Step 2/5: TTS API <<<" -ForegroundColor Magenta
& "$PSScriptRoot/tts-api/deploy.ps1" @deployArgs

# 3. TTS Functions
Write-Host "`n>>> Step 3/5: TTS Functions <<<" -ForegroundColor Magenta
& "$PSScriptRoot/tts-functions/deploy.ps1" @deployArgs

# 4. Voice Functions (amd64 only)
Write-Host "`n>>> Step 4/5: Voice Functions <<<" -ForegroundColor Magenta
& "$PSScriptRoot/voice-functions/deploy.ps1" @deployArgs

# 5. Chat WebService (depends on all others)
Write-Host "`n>>> Step 5/5: Chat WebService <<<" -ForegroundColor Magenta
& "$PSScriptRoot/chat-webservice/deploy.ps1" @deployArgs

# Final status
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nVerifying pods:" -ForegroundColor Cyan
kubectl get pods -n mudakai
kubectl get pods -n dapr-system
