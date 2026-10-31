param(
    [switch]$Force,
    [switch]$SkipBuild,
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/../../pi-shared-infrastructure/library/scripts/Deploy-Common.psm1" -Force
Import-Module "$PSScriptRoot/../../pi-shared-infrastructure/library/scripts/Build-DockerImage.psm1" -Force

Assert-EnvironmentVariables -Variables @(
    "TF_VAR_registry_password",
    "TF_VAR_discord_bot_token",
    "TF_VAR_openai_api_key",
    "TF_VAR_azure_storage_connection_string",
    "TF_VAR_azure_service_bus_connection_string"
)

Set-KubernetesContext -Context "t3-k3s"

$services = @(
    @{ Name = "tts-api"; Image = "mudakai-tts-api"; Project = "MudakAI.TextToSpeech.API"; Platforms = "linux/amd64,linux/arm64" },
    @{ Name = "tts-functions"; Image = "mudakai-tts-functions"; Project = "MudakAI.TextToSpeech.Functions"; Platforms = "linux/amd64" },
    @{ Name = "voice-functions"; Image = "mudakai-voice-functions"; Project = "MudakAI.Voice.Functions"; Platforms = "linux/amd64" },
    @{ Name = "chat-webservice"; Image = "mudakai-chat"; Project = "MudakAI.Chat.WebService"; Platforms = "linux/amd64,linux/arm64" }
)
$rebuiltServices = @()

foreach ($service in $services) {
    Update-HelmDependencies -HelmPath "$PSScriptRoot/helm/$($service.Name)"

    if (-not $SkipBuild) {
        # The shared builder stores its source hash in the working directory.
        $hashPath = Join-Path $PSScriptRoot ".image-hashes/$($service.Name)"
        New-Item -ItemType Directory -Path $hashPath -Force | Out-Null
        Push-Location $hashPath
        try {
            $buildOutput = @(Build-AndPushImage `
                -ImageName $service.Image `
                -SourcePath "$PSScriptRoot/../src/MudakAI.Net" `
                -DockerfilePath "$($service.Project)/Dockerfile" `
                -Platforms $service.Platforms `
                -IncludePatterns @("*.cs", "*.csproj", "*.json", "*.props", "*.targets", "Dockerfile") `
                -Force:$Force)
            if ($buildOutput -contains $true) {
                $rebuiltServices += $service.Name
            }
        }
        finally {
            Pop-Location
        }
    }
}

Push-Location $PSScriptRoot
try {
    Invoke-TerraformDeploy -Namespace "mudakai" -AutoApprove:$AutoApprove

    # Images retain their existing latest tags. Restart only when a new image
    # was published (or explicitly requested), so unchanged plans stay empty.
    foreach ($service in $services) {
        if ($Force -or $service.Name -in $rebuiltServices) {
            kubectl rollout restart "deployment/$($service.Name)" -n mudakai
            if ($LASTEXITCODE -ne 0) { throw "Failed to restart $($service.Name)" }
        }
        kubectl rollout status "deployment/$($service.Name)" -n mudakai --timeout=300s
        if ($LASTEXITCODE -ne 0) { throw "Rollout failed for $($service.Name)" }
    }

    kubectl rollout status deployment -n dapr-system --timeout=300s
    if ($LASTEXITCODE -ne 0) { throw "Dapr rollout failed" }
}
finally {
    Pop-Location
}
