# Deploy-Common.psm1
# Common deployment functions for MudakAI infrastructure

$ErrorActionPreference = "Stop"

<#
.SYNOPSIS
    Sets the Kubernetes context for deployments.
.PARAMETER Context
    The kubectl context to use (e.g., "t3-k3s", "raspberry-pi-k3s")
#>
function Set-KubernetesContext {
    param(
        [Parameter(Mandatory = $false)]
        [string]$Context = "t3-k3s"
    )

    Write-Host "Setting Kubernetes context to '$Context'..." -ForegroundColor Cyan
    kubectl config use-context $Context
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to set Kubernetes context"
    }
}

<#
.SYNOPSIS
    Updates Helm dependencies if Chart.yaml exists in the helm folder.
.PARAMETER HelmPath
    Path to the Helm chart directory
#>
function Update-HelmDependencies {
    param(
        [Parameter(Mandatory = $false)]
        [string]$HelmPath = ".\helm"
    )

    $chartFile = Join-Path $HelmPath "Chart.yaml"
    if (Test-Path $chartFile) {
        $chartContent = Get-Content $chartFile -Raw
        if ($chartContent -match "dependencies:") {
            Write-Host "Updating Helm dependencies..." -ForegroundColor Cyan
            Push-Location $HelmPath
            try {
                helm dependency update
                if ($LASTEXITCODE -ne 0) {
                    throw "Failed to update Helm dependencies"
                }
            }
            finally {
                Pop-Location
            }
        }
    }
}

<#
.SYNOPSIS
    Runs Terraform init, plan, and apply with interactive confirmation.
.PARAMETER AutoApprove
    Skip confirmation prompt and auto-apply changes
.PARAMETER PlanOnly
    Only run plan, skip apply
#>
function Invoke-TerraformDeploy {
    param(
        [Parameter(Mandatory = $false)]
        [switch]$AutoApprove,

        [Parameter(Mandatory = $false)]
        [switch]$PlanOnly,

        [Parameter(Mandatory = $false)]
        [string]$Namespace
    )

    # Initialize Terraform if needed
    if (-not (Test-Path ".terraform")) {
        Write-Host "Initializing Terraform..." -ForegroundColor Cyan
        terraform init
        if ($LASTEXITCODE -ne 0) {
            throw "Terraform init failed"
        }
    }

    Write-Host "Planning Terraform changes..." -ForegroundColor Cyan
    terraform plan -out=tfplan -detailed-exitcode
    $planExitCode = $LASTEXITCODE

    # Exit codes: 0 = no changes, 1 = error, 2 = changes present
    if ($planExitCode -eq 1) {
        Write-Host "Terraform plan failed!" -ForegroundColor Red
        exit 1
    }

    if ($planExitCode -eq 0) {
        Write-Host "No infrastructure changes detected." -ForegroundColor Green
        return
    }

    if ($PlanOnly) {
        Write-Host "Plan complete (apply skipped)." -ForegroundColor Yellow
        Remove-Item -Path "tfplan" -ErrorAction SilentlyContinue
        return
    }

    # Changes present (exit code 2)
    if (-not $AutoApprove) {
        Write-Host ""
        $confirm = Read-Host "Do you want to apply these changes? (yes/no)"
        if ($confirm -ne "yes") {
            Write-Host "Deployment cancelled." -ForegroundColor Yellow
            Remove-Item -Path "tfplan" -ErrorAction SilentlyContinue
            exit 0
        }
    }

    Write-Host "Applying Terraform infrastructure..." -ForegroundColor Cyan
    terraform apply -auto-approve tfplan

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Terraform apply failed!" -ForegroundColor Red
        Remove-Item -Path "tfplan" -ErrorAction SilentlyContinue
        exit 1
    }

    Remove-Item -Path "tfplan" -ErrorAction SilentlyContinue

    # Show pods if namespace provided
    if ($Namespace) {
        Write-Host "`nVerifying deployment..." -ForegroundColor Cyan
        kubectl get pods -n $Namespace
    }

    Write-Host "`nDeployment complete!" -ForegroundColor Green
}

<#
.SYNOPSIS
    Checks if required environment variables are set.
.PARAMETER Variables
    Array of environment variable names to check
#>
function Assert-EnvironmentVariables {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Variables
    )

    $missing = @()
    foreach ($var in $Variables) {
        if (-not (Get-Item "env:$var" -ErrorAction SilentlyContinue)) {
            $missing += $var
        }
    }

    if ($missing.Count -gt 0) {
        Write-Host "ERROR: Missing required environment variables:" -ForegroundColor Red
        foreach ($var in $missing) {
            Write-Host "  - $var" -ForegroundColor Yellow
        }
        exit 1
    }
}

<#
.SYNOPSIS
    Full deployment pipeline for Kubernetes/Helm projects.
.PARAMETER KubeContext
    Kubernetes context to use
.PARAMETER Namespace
    Namespace to verify after deployment
.PARAMETER RequiredEnvVars
    Environment variables that must be set
.PARAMETER PreDeployScript
    Script block to run before Terraform deployment
.PARAMETER PostDeployScript
    Script block to run after Terraform deployment
#>
function Invoke-StandardDeploy {
    param(
        [Parameter(Mandatory = $false)]
        [string]$KubeContext = "t3-k3s",

        [Parameter(Mandatory = $false)]
        [string]$Namespace,

        [Parameter(Mandatory = $false)]
        [string[]]$RequiredEnvVars = @(),

        [Parameter(Mandatory = $false)]
        [scriptblock]$PreDeployScript,

        [Parameter(Mandatory = $false)]
        [scriptblock]$PostDeployScript,

        [Parameter(Mandatory = $false)]
        [switch]$AutoApprove
    )

    # Check required environment variables
    if ($RequiredEnvVars.Count -gt 0) {
        Assert-EnvironmentVariables -Variables $RequiredEnvVars
    }

    # Set Kubernetes context
    Set-KubernetesContext -Context $KubeContext

    # Update Helm dependencies if needed
    Update-HelmDependencies

    # Run pre-deploy script if provided
    if ($PreDeployScript) {
        Write-Host "Running pre-deploy tasks..." -ForegroundColor Cyan
        & $PreDeployScript
    }

    # Run Terraform deployment
    Invoke-TerraformDeploy -Namespace $Namespace -AutoApprove:$AutoApprove

    # Run post-deploy script if provided
    if ($PostDeployScript) {
        Write-Host "Running post-deploy tasks..." -ForegroundColor Cyan
        & $PostDeployScript
    }
}

# Export module functions
Export-ModuleMember -Function @(
    'Set-KubernetesContext',
    'Update-HelmDependencies',
    'Invoke-TerraformDeploy',
    'Assert-EnvironmentVariables',
    'Invoke-StandardDeploy'
)
