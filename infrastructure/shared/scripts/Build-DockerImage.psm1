# Build-DockerImage.psm1
# Docker build and push functions for MudakAI infrastructure

$ErrorActionPreference = "Stop"

<#
.SYNOPSIS
    Calculates a hash of source files to detect changes.
.PARAMETER SourcePath
    Path to the source directory
.PARAMETER IncludePatterns
    File patterns to include in hash calculation
.PARAMETER ExcludePatterns
    Directory patterns to exclude from hash calculation
#>
function Get-SourceHash {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourcePath,

        [Parameter(Mandatory = $false)]
        [string[]]$IncludePatterns = @("*.cs", "*.csproj", "Dockerfile"),

        [Parameter(Mandatory = $false)]
        [string[]]$ExcludePatterns = @("bin", "obj", "node_modules", ".git")
    )

    $excludeRegex = ($ExcludePatterns | ForEach-Object { [regex]::Escape($_) }) -join "|"
    $excludeRegex = "\\($excludeRegex)\\"

    $files = Get-ChildItem -Path $SourcePath -Recurse -File -Include $IncludePatterns |
        Where-Object { $_.FullName -notmatch $excludeRegex }

    if ($files.Count -eq 0) {
        return ""
    }

    $content = ($files | Get-FileHash -Algorithm SHA256 | Sort-Object Path | ForEach-Object { $_.Hash }) -join ""
    $bytes = [System.Security.Cryptography.SHA256]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes($content))
    return ($bytes | ForEach-Object { $_.ToString("x2") }) -join ""
}

<#
.SYNOPSIS
    Checks if source files have changed since last build.
.PARAMETER SourcePath
    Path to the source directory
.PARAMETER HashFile
    Path to store/read the hash file
.PARAMETER IncludePatterns
    File patterns to include in hash calculation
#>
function Test-SourceChanged {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourcePath,

        [Parameter(Mandatory = $false)]
        [string]$HashFile = ".image-hash",

        [Parameter(Mandatory = $false)]
        [string[]]$IncludePatterns = @("*.cs", "*.csproj", "Dockerfile")
    )

    $currentHash = Get-SourceHash -SourcePath $SourcePath -IncludePatterns $IncludePatterns
    $previousHash = if (Test-Path $HashFile) { (Get-Content $HashFile -Raw).Trim() } else { "" }

    return @{
        Changed      = $currentHash -ne $previousHash
        CurrentHash  = $currentHash
        PreviousHash = $previousHash
    }
}

<#
.SYNOPSIS
    Saves the source hash to a file.
.PARAMETER Hash
    The hash value to save
.PARAMETER HashFile
    Path to the hash file
#>
function Save-SourceHash {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Hash,

        [Parameter(Mandatory = $false)]
        [string]$HashFile = ".image-hash"
    )

    $Hash | Set-Content $HashFile
}

<#
.SYNOPSIS
    Builds and pushes a multi-arch Docker image to a private K3s registry.
.PARAMETER ImageName
    Name of the Docker image
.PARAMETER SourcePath
    Path to the source directory
.PARAMETER DockerfilePath
    Path to the Dockerfile (relative to SourcePath)
.PARAMETER RemoteHost
    SSH remote host (e.g., "user@host")
.PARAMETER RegistryService
    Name of the registry service in Kubernetes
.PARAMETER RegistryNamespace
    Namespace where the registry is deployed
.PARAMETER RegistryPassword
    Password for the registry (defaults to TF_VAR_registry_password)
.PARAMETER Platforms
    Target platforms for multi-arch build
.PARAMETER Force
    Force rebuild even if no changes detected
#>
function Build-AndPushImage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ImageName,

        [Parameter(Mandatory = $true)]
        [string]$SourcePath,

        [Parameter(Mandatory = $false)]
        [string]$DockerfilePath = "Dockerfile",

        [Parameter(Mandatory = $false)]
        [string]$RemoteHost = "uaspro@t3",

        [Parameter(Mandatory = $false)]
        [string]$RegistryService = "registry-docker-registry",

        [Parameter(Mandatory = $false)]
        [string]$RegistryNamespace = "container-registry",

        [Parameter(Mandatory = $false)]
        [string]$RegistryPassword = $env:TF_VAR_registry_password,

        [Parameter(Mandatory = $false)]
        [string]$Platforms = "linux/amd64,linux/arm64",

        [Parameter(Mandatory = $false)]
        [switch]$Force,

        [Parameter(Mandatory = $false)]
        [string[]]$IncludePatterns = @("*.cs", "*.csproj", "Dockerfile")
    )

    # Check for registry password
    if (-not $RegistryPassword) {
        Write-Host "ERROR: Registry password not set!" -ForegroundColor Red
        Write-Host "Set TF_VAR_registry_password environment variable." -ForegroundColor Yellow
        exit 1
    }

    # Check for source changes
    $changeCheck = Test-SourceChanged -SourcePath $SourcePath -IncludePatterns $IncludePatterns

    if (-not $Force -and -not $changeCheck.Changed) {
        Write-Host "No source changes detected, skipping build..." -ForegroundColor Green
        return $false
    }

    $tarFile = "${ImageName}.tar"
    $fullDockerfilePath = Join-Path $SourcePath $DockerfilePath

    try {
        # Build multi-arch image
        Write-Host "Building multi-arch image ($Platforms)..." -ForegroundColor Cyan
        docker buildx build --platform $Platforms -t "${ImageName}:latest" -f $fullDockerfilePath -o "type=oci,dest=$tarFile" $SourcePath
        if ($LASTEXITCODE -ne 0) { throw "Docker build failed" }

        # Copy to remote
        Write-Host "Copying image to remote ($RemoteHost)..." -ForegroundColor Cyan
        scp $tarFile "${RemoteHost}:/tmp/${tarFile}"
        if ($LASTEXITCODE -ne 0) { throw "SCP failed" }

        # Push to registry
        Write-Host "Pushing to registry..." -ForegroundColor Cyan
        $remoteCmd = @"
set -e
REGISTRY_IP=`$(sudo kubectl get svc $RegistryService -n $RegistryNamespace -o jsonpath='{.spec.clusterIP}'):5000
sudo k3s ctr images import --all-platforms /tmp/$tarFile
sudo k3s ctr images tag docker.io/library/${ImageName}:latest `$REGISTRY_IP/${ImageName}:latest
sudo k3s ctr images push --plain-http --user admin:$RegistryPassword `$REGISTRY_IP/${ImageName}:latest
rm -f /tmp/$tarFile
sudo k3s ctr images rm docker.io/library/${ImageName}:latest `$REGISTRY_IP/${ImageName}:latest 2>/dev/null || true
"@
        ssh -t $RemoteHost $remoteCmd
        if ($LASTEXITCODE -ne 0) { throw "Registry push failed" }

        # Save hash
        Save-SourceHash -Hash $changeCheck.CurrentHash

        Write-Host "Image pushed successfully!" -ForegroundColor Green
        return $true
    }
    finally {
        # Cleanup local tar file
        Remove-Item $tarFile -ErrorAction SilentlyContinue
    }
}

# Export module functions
Export-ModuleMember -Function @(
    'Get-SourceHash',
    'Test-SourceChanged',
    'Save-SourceHash',
    'Build-AndPushImage'
)
