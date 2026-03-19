param(
    [switch]$Force,
    [switch]$SkipBuild,
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/../shared/scripts/Deploy-Common.psm1" -Force
Import-Module "$PSScriptRoot/../shared/scripts/Build-DockerImage.psm1" -Force

Write-Host "=== TTS Functions Deployment ===" -ForegroundColor Cyan

Assert-EnvironmentVariables -Variables @(
    "TF_VAR_registry_password",
    "TF_VAR_azure_storage_connection_string",
    "TF_VAR_azure_service_bus_connection_string",
    "TF_VAR_openai_api_key"
)
Set-KubernetesContext -Context "t3-k3s"

if (-not $SkipBuild) {
    Build-AndPushImage `
        -ImageName "mudakai-tts-functions" `
        -SourcePath "$PSScriptRoot/../../src/MudakAI.Net" `
        -DockerfilePath "MudakAI.TextToSpeech.Functions/Dockerfile" `
        -Force:$Force
}

Update-HelmDependencies -HelmPath "$PSScriptRoot/helm"

Push-Location $PSScriptRoot
try {
    Invoke-TerraformDeploy -Namespace "mudakai" -AutoApprove:$AutoApprove
} finally {
    Pop-Location
}

Write-Host "`nDone!" -ForegroundColor Green
