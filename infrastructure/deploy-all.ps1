# Compatibility entry point; every deployment uses the same Terraform root.
param(
    [switch]$Force,
    [switch]$SkipBuild,
    [switch]$AutoApprove
)

& "$PSScriptRoot/deploy.ps1" @PSBoundParameters
