param(
    [switch]$Force,
    [switch]$SkipBuild,
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/../shared/scripts/Deploy-Common.psm1" -Force
Import-Module "$PSScriptRoot/../shared/scripts/Build-DockerImage.psm1" -Force

Write-Host "=== TTS Python API Deployment ===" -ForegroundColor Cyan

Assert-EnvironmentVariables -Variables @("TF_VAR_registry_password")
Set-KubernetesContext -Context "t3-k3s"

if (-not $SkipBuild) {
    Build-AndPushImage `
        -ImageName "mudakai-tts-python" `
        -SourcePath "$PSScriptRoot/../../src/text-to-speech-api" `
        -Force:$Force `
        -IncludePatterns @("*.py", "requirements.txt", "*.yaml", "Dockerfile")
}

Update-HelmDependencies -HelmPath "$PSScriptRoot/helm"
Invoke-TerraformDeploy -Namespace "mudakai" -AutoApprove:$AutoApprove

Write-Host "`nDone!" -ForegroundColor Green
