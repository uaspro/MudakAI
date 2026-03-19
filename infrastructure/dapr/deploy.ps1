param(
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/../shared/scripts/Deploy-Common.psm1" -Force

Write-Host "=== Dapr Deployment ===" -ForegroundColor Cyan

Set-KubernetesContext -Context "t3-k3s"
Invoke-TerraformDeploy -Namespace "dapr-system" -AutoApprove:$AutoApprove

Write-Host "`nDone!" -ForegroundColor Green
