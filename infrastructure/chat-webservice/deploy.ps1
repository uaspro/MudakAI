param(
    [switch]$Force,
    [switch]$SkipBuild,
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/../shared/scripts/Deploy-Common.psm1" -Force
Import-Module "$PSScriptRoot/../shared/scripts/Build-DockerImage.psm1" -Force

Write-Host "=== Chat WebService Deployment ===" -ForegroundColor Cyan

Assert-EnvironmentVariables -Variables @(
    "TF_VAR_registry_password",
    "TF_VAR_discord_bot_token",
    "TF_VAR_openai_api_key",
    "TF_VAR_azure_storage_connection_string"
)
Set-KubernetesContext -Context "t3-k3s"

if (-not $SkipBuild) {
    Build-AndPushImage `
        -ImageName "mudakai-chat" `
        -SourcePath "$PSScriptRoot/../../src/MudakAI.Net" `
        -DockerfilePath "MudakAI.Chat.WebService/Dockerfile" `
        -Force:$Force
}

Update-HelmDependencies -HelmPath "$PSScriptRoot/helm"
Invoke-TerraformDeploy -Namespace "mudakai" -AutoApprove:$AutoApprove

Write-Host "`nDone!" -ForegroundColor Green
