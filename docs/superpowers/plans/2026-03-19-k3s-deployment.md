# MudakAI k3s Deployment Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deploy all 4 MudakAI services to a home lab k3s cluster using Terraform + Helm + PowerShell, mirroring the Pi project patterns.

**Architecture:** Self-contained infrastructure directory with shared modules (PowerShell, Terraform, Helm base chart) copied from Pi and adapted. Per-service Terraform + Helm configurations. Dapr installed via Terraform-managed Helm release. Single `mudakai` namespace.

**Tech Stack:** PowerShell, Terraform (azurerm backend), Helm 3, Kubernetes, Docker buildx, Dapr

**Spec:** `docs/superpowers/specs/2026-03-19-k3s-deployment-design.md`

---

## Chunk 1: Shared Infrastructure

### Task 1: Shared PowerShell Modules

**Files:**
- Create: `infrastructure/shared/scripts/Deploy-Common.psm1`
- Create: `infrastructure/shared/scripts/Build-DockerImage.psm1`

These are direct copies from `C:\Dev\Pi\pi-shared-infrastructure\scripts\`. No modifications needed — the modules are parameterized and work as-is.

- [ ] **Step 1: Create Deploy-Common.psm1**

Copy from Pi reference. This module provides: `Set-KubernetesContext`, `Update-HelmDependencies`, `Invoke-TerraformDeploy`, `Assert-EnvironmentVariables`, `Invoke-StandardDeploy`.

```powershell
# infrastructure/shared/scripts/Deploy-Common.psm1
# Exact copy of C:\Dev\Pi\pi-shared-infrastructure\scripts\Deploy-Common.psm1
```

- [ ] **Step 2: Create Build-DockerImage.psm1**

Copy from Pi reference. This module provides: `Get-SourceHash`, `Test-SourceChanged`, `Save-SourceHash`, `Build-AndPushImage`.

```powershell
# infrastructure/shared/scripts/Build-DockerImage.psm1
# Exact copy of C:\Dev\Pi\pi-shared-infrastructure\scripts\Build-DockerImage.psm1
```

- [ ] **Step 3: Commit**

```bash
git add infrastructure/shared/scripts/
git commit -m "feat: add shared PowerShell deployment modules"
```

---

### Task 2: Shared Terraform Module

**Files:**
- Create: `infrastructure/shared/modules/k8s-helm-deployment/main.tf`
- Create: `infrastructure/shared/modules/k8s-helm-deployment/variables.tf`

Direct copies from `C:\Dev\Pi\pi-shared-infrastructure\modules\k8s-helm-deployment\`.

- [ ] **Step 1: Create main.tf**

```hcl
# infrastructure/shared/modules/k8s-helm-deployment/main.tf
# Exact copy of C:\Dev\Pi\pi-shared-infrastructure\modules\k8s-helm-deployment\main.tf
```

- [ ] **Step 2: Create variables.tf**

```hcl
# infrastructure/shared/modules/k8s-helm-deployment/variables.tf
# Exact copy of C:\Dev\Pi\pi-shared-infrastructure\modules\k8s-helm-deployment\variables.tf
```

- [ ] **Step 3: Commit**

```bash
git add infrastructure/shared/modules/
git commit -m "feat: add shared Terraform k8s-helm-deployment module"
```

---

### Task 3: Shared Helm Base Chart

**Files:**
- Create: `infrastructure/shared/helm/mudakai-base-app/Chart.yaml`
- Create: `infrastructure/shared/helm/mudakai-base-app/values.yaml`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/_helpers.tpl`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/_deployment.tpl`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/_service.tpl`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/_configmap.tpl`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/_secret.tpl`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/_pvc.tpl`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/_registry-secret.tpl`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/deployment.yaml`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/service.yaml`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/configmap.yaml`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/secret.yaml`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/pvc.yaml`
- Create: `infrastructure/shared/helm/mudakai-base-app/templates/registry-secret.yaml`

Copy from `C:\Dev\Pi\pi-shared-infrastructure\helm\pi-base-app\` with all internal template names renamed from `pi-base-app` to `mudakai-base-app`.

- [ ] **Step 1: Create Chart.yaml**

```yaml
apiVersion: v2
name: mudakai-base-app
description: Base Helm chart for MudakAI applications - shared dependency
type: application
version: 1.0.0
appVersion: "1.0.0"
```

- [ ] **Step 2: Create values.yaml**

Copy from Pi's `pi-base-app/values.yaml`. Add Dapr annotation defaults. The values.yaml should include:

```yaml
# Default values for mudakai-base-app

replicaCount: 1

image:
  repository: ""
  pullPolicy: Always
  tag: "latest"

imagePullSecrets: []
nameOverride: ""
fullnameOverride: ""

global:
  registry: "registry-docker-registry.container-registry.svc.cluster.local:5000"

serviceAccount:
  create: false
  name: ""

podAnnotations: {}
podLabels: {}

# Dapr sidecar configuration
dapr:
  enabled: false
  appId: ""
  appPort: "8080"

service:
  enabled: true
  type: ClusterIP
  port: 80
  targetPort: 8080
  annotations: {}

ingress:
  enabled: false

resources:
  limits:
    cpu: 200m
    memory: 128Mi
  requests:
    cpu: 50m
    memory: 32Mi

autoscaling:
  enabled: false

nodeSelector: {}
tolerations: []
affinity: {}

env: []
envFromSecret: {}

configMap:
  enabled: false
  mountPath: ""
  subPath: ""
  data: {}

secret:
  enabled: false
  data: {}

persistence:
  enabled: false
  accessMode: ReadWriteOnce
  size: 1Gi
  mountPath: /data
  storageClass: ""

startupProbe:
  enabled: false
livenessProbe:
  enabled: false
readinessProbe:
  enabled: false

deploymentTimestamp: ""
```

- [ ] **Step 3: Create _helpers.tpl**

Copy from Pi, rename all `pi-base-app` references to `mudakai-base-app`:

```
{{- define "mudakai-base-app.name" -}}
...
{{- define "mudakai-base-app.fullname" -}}
...
{{- define "mudakai-base-app.chart" -}}
...
{{- define "mudakai-base-app.labels" -}}
...
{{- define "mudakai-base-app.selectorLabels" -}}
...
{{- define "mudakai-base-app.serviceAccountName" -}}
...
{{- define "mudakai-base-app.registryUrl" -}}
...
{{- define "mudakai-base-app.image" -}}
...
```

- [ ] **Step 4: Create _deployment.tpl**

Copy from Pi, rename all `pi-base-app` to `mudakai-base-app`. Add Dapr annotation injection:

In the `template.metadata.annotations` section, add Dapr annotations:

```yaml
        {{- if and .Values.dapr .Values.dapr.enabled }}
        dapr.io/enabled: "true"
        dapr.io/app-id: {{ .Values.dapr.appId | quote }}
        dapr.io/app-port: {{ .Values.dapr.appPort | quote }}
        {{- end }}
```

This goes in the pod template annotations block alongside `deploymentTimestamp` and `podAnnotations`.

- [ ] **Step 5: Create remaining template files**

Copy from Pi with `pi-base-app` → `mudakai-base-app` rename:
- `_service.tpl`
- `_configmap.tpl`
- `_secret.tpl`
- `_pvc.tpl`
- `_registry-secret.tpl`

- [ ] **Step 6: Create manifest yaml files**

These are the thin wrappers that invoke the named templates:

`deployment.yaml`:
```yaml
{{ include "mudakai-base-app.deployment" . }}
```

`service.yaml`:
```yaml
{{ include "mudakai-base-app.service" . }}
```

`configmap.yaml`:
```yaml
{{ include "mudakai-base-app.configmap" . }}
```

`secret.yaml`:
```yaml
{{ include "mudakai-base-app.secret" . }}
```

`pvc.yaml`:
```yaml
{{ include "mudakai-base-app.pvc" . }}
```

`registry-secret.yaml`:
```yaml
{{ include "mudakai-base-app.registry-secret" . }}
```

Note: Do NOT copy rollout.yaml, canary-service.yaml, traefikservice.yaml, analysistemplate.yaml, hpa.yaml, pdb.yaml, serviceaccount.yaml — MudakAI does not need Argo Rollouts or these features.

- [ ] **Step 7: Validate Helm chart**

Run: `helm lint infrastructure/shared/helm/mudakai-base-app/`
Expected: No errors

- [ ] **Step 8: Commit**

```bash
git add infrastructure/shared/helm/
git commit -m "feat: add mudakai-base-app Helm library chart"
```

---

## Chunk 2: Dapr

### Task 4: Dapr Infrastructure

**Files:**
- Create: `infrastructure/dapr/deploy.ps1`
- Create: `infrastructure/dapr/main.tf`
- Create: `infrastructure/dapr/providers.tf`
- Create: `infrastructure/dapr/variables.tf`

- [ ] **Step 1: Create providers.tf**

```hcl
terraform {
  required_version = ">= 1.0"

  backend "azurerm" {
    resource_group_name  = "ay-dev-001"
    storage_account_name = "aydevsa"
    container_name       = "tfstate"
    key                  = "mudakai/dapr.tfstate"
  }

  required_providers {
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.11"
    }
  }
}

provider "helm" {
  kubernetes {
    config_path    = "~/.kube/config"
    config_context = var.kubernetes_context
  }
}
```

- [ ] **Step 2: Create variables.tf**

```hcl
variable "kubernetes_context" {
  description = "Kubernetes context to use"
  type        = string
  default     = "t3-k3s"
}

variable "dapr_version" {
  description = "Dapr Helm chart version"
  type        = string
  default     = "1.14"
}
```

- [ ] **Step 3: Create main.tf**

```hcl
resource "helm_release" "dapr" {
  name             = "dapr"
  namespace        = "dapr-system"
  create_namespace = true
  repository       = "https://dapr.github.io/helm-charts/"
  chart            = "dapr"
  version          = var.dapr_version
  wait             = true
  timeout          = 300

  set {
    name  = "global.ha.enabled"
    value = "false"
  }
}
```

- [ ] **Step 4: Create deploy.ps1**

```powershell
param(
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/../shared/scripts/Deploy-Common.psm1" -Force

Write-Host "=== Dapr Deployment ===" -ForegroundColor Cyan

Set-KubernetesContext -Context "t3-k3s"
Invoke-TerraformDeploy -Namespace "dapr-system" -AutoApprove:$AutoApprove

Write-Host "`nDone!" -ForegroundColor Green
```

- [ ] **Step 5: Validate Terraform**

Run: `cd infrastructure/dapr && terraform init && terraform validate`
Expected: "Success! The configuration is valid."

- [ ] **Step 6: Commit**

```bash
git add infrastructure/dapr/
git commit -m "feat: add Dapr Terraform deployment"
```

---

## Chunk 3: .NET Service Deployments

### Task 6: TextToSpeech.API Service (tts-api)

**Files:**
- Create: `infrastructure/tts-api/deploy.ps1`
- Create: `infrastructure/tts-api/main.tf`
- Create: `infrastructure/tts-api/providers.tf`
- Create: `infrastructure/tts-api/variables.tf`
- Create: `infrastructure/tts-api/helm/Chart.yaml`
- Create: `infrastructure/tts-api/helm/values.yaml`

- [ ] **Step 1: Create providers.tf**

Use the Azure Storage backend and Kubernetes/Helm providers in `infrastructure/tts-api/providers.tf`, with key: `"mudakai/tts-api.tfstate"`.

- [ ] **Step 2: Create variables.tf**

```hcl
variable "kubernetes_context" {
  description = "Kubernetes context to use"
  type        = string
  default     = "t3-k3s"
}

variable "namespace" {
  description = "Kubernetes namespace"
  type        = string
  default     = "mudakai"
}

variable "app_name" {
  description = "Application name"
  type        = string
  default     = "tts-api"
}

variable "registry_password" {
  description = "Private container registry password"
  type        = string
  sensitive   = true
}

variable "azure_service_bus_connection_string" {
  description = "Azure Service Bus connection string"
  type        = string
  sensitive   = true
}
```

- [ ] **Step 3: Create main.tf**

```hcl
locals {
  registry_dockerconfig = base64encode(jsonencode({
    auths = {
      "registry-docker-registry.container-registry.svc.cluster.local:5000" = {
        username = "admin"
        password = var.registry_password
        auth     = base64encode("admin:${var.registry_password}")
      }
    }
  }))
}

module "tts_api" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace       = var.namespace
  app_name        = var.app_name
  helm_chart_path = "${path.module}/helm"

  helm_set_values = {
    "app.deploymentTimestamp"        = timestamp()
    "app.imagePullSecrets[0].name"   = "registry-credentials"
    "app.imagePullSecrets[0].create" = "true"
  }

  helm_set_sensitive_values = {
    "app.imagePullSecrets[0].dockerconfigjson"                                  = local.registry_dockerconfig
    "app.secret.data.ServiceBusConnectionString"                                = var.azure_service_bus_connection_string
    "app.envFromSecret.ConnectionStrings__ServiceBus.secretName"                = var.app_name
    "app.envFromSecret.ConnectionStrings__ServiceBus.key"                       = "ServiceBusConnectionString"
  }
}
```

- [ ] **Step 4: Create helm/Chart.yaml**

```yaml
apiVersion: v2
name: tts-api
description: MudakAI Text-to-Speech orchestration API
type: application
version: 1.0.0
appVersion: "1.0.0"

dependencies:
  - name: mudakai-base-app
    version: "1.0.0"
    repository: "file://../../shared/helm/mudakai-base-app"
    alias: app
```

- [ ] **Step 5: Create helm/values.yaml**

```yaml
app:
  nameOverride: "tts-api"

  replicaCount: 1

  image:
    repository: mudakai-tts-api
    pullPolicy: Always
    tag: "latest"

  imagePullSecrets:
    - name: registry-credentials
      create: false
      dockerconfigjson: ""

  dapr:
    enabled: true
    appId: "mudakaitexttospeech-api"
    appPort: "8080"

  service:
    enabled: true
    type: ClusterIP
    port: 80
    targetPort: 8080

  secret:
    enabled: true
    data:
      ServiceBusConnectionString: ""

  env:
    - name: ASPNETCORE_ENVIRONMENT
      value: "Production"
    - name: ASPNETCORE_URLS
      value: "http://+:8080"

  envFromSecret: {}

  configMap:
    enabled: true
    mountPath: /app/appsettings.json
    subPath: appsettings.json
    data:
      appsettings.json: |
        {
          "TextToSpeechQueueName": "text-to-speech",
          "Logging": {
            "LogLevel": {
              "Default": "Information",
              "Microsoft.AspNetCore": "Warning"
            }
          },
          "AllowedHosts": "*"
        }

  resources:
    limits:
      cpu: 200m
      memory: 128Mi
    requests:
      cpu: 50m
      memory: 64Mi
```

- [ ] **Step 6: Create deploy.ps1**

```powershell
param(
    [switch]$Force,
    [switch]$SkipBuild,
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/../shared/scripts/Deploy-Common.psm1" -Force
Import-Module "$PSScriptRoot/../shared/scripts/Build-DockerImage.psm1" -Force

Write-Host "=== TTS API Deployment ===" -ForegroundColor Cyan

Assert-EnvironmentVariables -Variables @(
    "TF_VAR_registry_password",
    "TF_VAR_azure_service_bus_connection_string"
)
Set-KubernetesContext -Context "t3-k3s"

if (-not $SkipBuild) {
    Build-AndPushImage `
        -ImageName "mudakai-tts-api" `
        -SourcePath "$PSScriptRoot/../../src/MudakAI.Net" `
        -DockerfilePath "MudakAI.TextToSpeech.API/Dockerfile" `
        -Force:$Force
}

Update-HelmDependencies -HelmPath "$PSScriptRoot/helm"
Invoke-TerraformDeploy -Namespace "mudakai" -AutoApprove:$AutoApprove

Write-Host "`nDone!" -ForegroundColor Green
```

- [ ] **Step 7: Commit**

```bash
git add infrastructure/tts-api/
git commit -m "feat: add tts-api k3s deployment"
```

---

### Task 7: TextToSpeech.Functions Service (tts-functions)

**Files:**
- Create: `infrastructure/tts-functions/deploy.ps1`
- Create: `infrastructure/tts-functions/main.tf`
- Create: `infrastructure/tts-functions/providers.tf`
- Create: `infrastructure/tts-functions/variables.tf`
- Create: `infrastructure/tts-functions/helm/Chart.yaml`
- Create: `infrastructure/tts-functions/helm/values.yaml`

- [ ] **Step 1: Create providers.tf**

Same pattern, key: `"mudakai/tts-functions.tfstate"`

- [ ] **Step 2: Create variables.tf**

```hcl
variable "kubernetes_context" {
  description = "Kubernetes context to use"
  type        = string
  default     = "t3-k3s"
}

variable "namespace" {
  description = "Kubernetes namespace"
  type        = string
  default     = "mudakai"
}

variable "app_name" {
  description = "Application name"
  type        = string
  default     = "tts-functions"
}

variable "registry_password" {
  description = "Private container registry password"
  type        = string
  sensitive   = true
}

variable "azure_storage_connection_string" {
  description = "Azure Storage connection string"
  type        = string
  sensitive   = true
}

variable "azure_service_bus_connection_string" {
  description = "Azure Service Bus connection string"
  type        = string
  sensitive   = true
}

variable "openai_api_key" {
  description = "OpenAI API key"
  type        = string
  sensitive   = true
}
```

- [ ] **Step 3: Create main.tf**

```hcl
locals {
  registry_dockerconfig = base64encode(jsonencode({
    auths = {
      "registry-docker-registry.container-registry.svc.cluster.local:5000" = {
        username = "admin"
        password = var.registry_password
        auth     = base64encode("admin:${var.registry_password}")
      }
    }
  }))
}

module "tts_functions" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace       = var.namespace
  app_name        = var.app_name
  helm_chart_path = "${path.module}/helm"

  helm_set_values = {
    "app.deploymentTimestamp"        = timestamp()
    "app.imagePullSecrets[0].name"   = "registry-credentials"
    "app.imagePullSecrets[0].create" = "true"
  }

  helm_set_sensitive_values = {
    "app.imagePullSecrets[0].dockerconfigjson"                          = local.registry_dockerconfig
    "app.secret.data.AzureWebJobsStorage"                              = var.azure_storage_connection_string
    "app.secret.data.BlobStorage"                                      = var.azure_storage_connection_string
    "app.secret.data.ServiceBusListen"                                 = var.azure_service_bus_connection_string
    "app.secret.data.ServiceBusSend"                                   = var.azure_service_bus_connection_string
    "app.secret.data.OpenAIApiKey"                                     = var.openai_api_key
    "app.envFromSecret.AzureWebJobsStorage.secretName"                 = var.app_name
    "app.envFromSecret.AzureWebJobsStorage.key"                        = "AzureWebJobsStorage"
    "app.envFromSecret.BlobStorage.secretName"                         = var.app_name
    "app.envFromSecret.BlobStorage.key"                                = "BlobStorage"
    "app.envFromSecret.ServiceBusListen.secretName"                    = var.app_name
    "app.envFromSecret.ServiceBusListen.key"                           = "ServiceBusListen"
    "app.envFromSecret.ServiceBusSend.secretName"                      = var.app_name
    "app.envFromSecret.ServiceBusSend.key"                             = "ServiceBusSend"
    "app.envFromSecret.OpenAI__OpenAIApiKey.secretName"                = var.app_name
    "app.envFromSecret.OpenAI__OpenAIApiKey.key"                       = "OpenAIApiKey"
  }
}
```

- [ ] **Step 4: Create helm/Chart.yaml**

```yaml
apiVersion: v2
name: tts-functions
description: MudakAI Text-to-Speech Azure Functions
type: application
version: 1.0.0
appVersion: "1.0.0"

dependencies:
  - name: mudakai-base-app
    version: "1.0.0"
    repository: "file://../../shared/helm/mudakai-base-app"
    alias: app
```

- [ ] **Step 5: Create helm/values.yaml**

```yaml
app:
  nameOverride: "tts-functions"

  replicaCount: 1

  image:
    repository: mudakai-tts-functions
    pullPolicy: Always
    tag: "latest"

  imagePullSecrets:
    - name: registry-credentials
      create: false
      dockerconfigjson: ""

  dapr:
    enabled: false

  podAnnotations:
    dapr.io/enabled: "false"

  service:
    enabled: true
    type: ClusterIP
    port: 80
    targetPort: 80

  secret:
    enabled: true
    data:
      AzureWebJobsStorage: ""
      BlobStorage: ""
      ServiceBusListen: ""
      ServiceBusSend: ""
      OpenAIApiKey: ""

  env:
    - name: FUNCTIONS_WORKER_RUNTIME
      value: "dotnet-isolated"
    - name: AzureWebJobsScriptRoot
      value: "/home/site/wwwroot"

  envFromSecret: {}

  configMap:
    enabled: true
    mountPath: /home/site/wwwroot/appsettings.json
    subPath: appsettings.json
    data:
      appsettings.json: |
        {
          "ServiceBusPlaybackQueueName": "playback",
          "BlobStorageAudioContainerName": "audio",
          "OpenAI": {
            "TTSModelId": "tts-1"
          }
        }

  resources:
    limits:
      cpu: 500m
      memory: 256Mi
    requests:
      cpu: 100m
      memory: 128Mi
```

- [ ] **Step 6: Create deploy.ps1**

```powershell
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
Invoke-TerraformDeploy -Namespace "mudakai" -AutoApprove:$AutoApprove

Write-Host "`nDone!" -ForegroundColor Green
```

- [ ] **Step 7: Commit**

```bash
git add infrastructure/tts-functions/
git commit -m "feat: add tts-functions k3s deployment"
```

---

### Task 8: Voice.Functions Service (voice-functions, amd64 only)

**Files:**
- Create: `infrastructure/voice-functions/deploy.ps1`
- Create: `infrastructure/voice-functions/main.tf`
- Create: `infrastructure/voice-functions/providers.tf`
- Create: `infrastructure/voice-functions/variables.tf`
- Create: `infrastructure/voice-functions/helm/Chart.yaml`
- Create: `infrastructure/voice-functions/helm/values.yaml`

- [ ] **Step 1: Create providers.tf**

Same pattern, key: `"mudakai/voice-functions.tfstate"`

- [ ] **Step 2: Create variables.tf**

```hcl
variable "kubernetes_context" {
  description = "Kubernetes context to use"
  type        = string
  default     = "t3-k3s"
}

variable "namespace" {
  description = "Kubernetes namespace"
  type        = string
  default     = "mudakai"
}

variable "app_name" {
  description = "Application name"
  type        = string
  default     = "voice-functions"
}

variable "registry_password" {
  description = "Private container registry password"
  type        = string
  sensitive   = true
}

variable "azure_storage_connection_string" {
  description = "Azure Storage connection string"
  type        = string
  sensitive   = true
}

variable "azure_service_bus_connection_string" {
  description = "Azure Service Bus connection string"
  type        = string
  sensitive   = true
}

variable "discord_bot_token" {
  description = "Discord bot token"
  type        = string
  sensitive   = true
}
```

- [ ] **Step 3: Create main.tf**

```hcl
locals {
  registry_dockerconfig = base64encode(jsonencode({
    auths = {
      "registry-docker-registry.container-registry.svc.cluster.local:5000" = {
        username = "admin"
        password = var.registry_password
        auth     = base64encode("admin:${var.registry_password}")
      }
    }
  }))
}

module "voice_functions" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace       = var.namespace
  app_name        = var.app_name
  helm_chart_path = "${path.module}/helm"

  helm_set_values = {
    "app.deploymentTimestamp"        = timestamp()
    "app.imagePullSecrets[0].name"   = "registry-credentials"
    "app.imagePullSecrets[0].create" = "true"
  }

  helm_set_sensitive_values = {
    "app.imagePullSecrets[0].dockerconfigjson"                        = local.registry_dockerconfig
    "app.secret.data.AzureWebJobsStorage"                            = var.azure_storage_connection_string
    "app.secret.data.BlobStorage"                                    = var.azure_storage_connection_string
    "app.secret.data.ServiceBus"                                     = var.azure_service_bus_connection_string
    "app.secret.data.DiscordBotToken"                                = var.discord_bot_token
    "app.envFromSecret.AzureWebJobsStorage.secretName"               = var.app_name
    "app.envFromSecret.AzureWebJobsStorage.key"                      = "AzureWebJobsStorage"
    "app.envFromSecret.BlobStorage.secretName"                       = var.app_name
    "app.envFromSecret.BlobStorage.key"                              = "BlobStorage"
    "app.envFromSecret.ServiceBus.secretName"                        = var.app_name
    "app.envFromSecret.ServiceBus.key"                               = "ServiceBus"
    "app.envFromSecret.DiscordBotToken.secretName"                   = var.app_name
    "app.envFromSecret.DiscordBotToken.key"                          = "DiscordBotToken"
  }
}
```

- [ ] **Step 4: Create helm/Chart.yaml**

```yaml
apiVersion: v2
name: voice-functions
description: MudakAI Voice Azure Functions (Discord audio)
type: application
version: 1.0.0
appVersion: "1.0.0"

dependencies:
  - name: mudakai-base-app
    version: "1.0.0"
    repository: "file://../../shared/helm/mudakai-base-app"
    alias: app
```

- [ ] **Step 5: Create helm/values.yaml**

Note: `nodeSelector` set to `amd64` due to hardcoded native libraries in Dockerfile. Build is amd64-only (`Platforms` override in deploy.ps1).

```yaml
app:
  nameOverride: "voice-functions"

  replicaCount: 1

  image:
    repository: mudakai-voice-functions
    pullPolicy: Always
    tag: "latest"

  imagePullSecrets:
    - name: registry-credentials
      create: false
      dockerconfigjson: ""

  dapr:
    enabled: true
    appId: "mudakaivoice-functions"
    appPort: "80"

  service:
    enabled: true
    type: ClusterIP
    port: 80
    targetPort: 80

  secret:
    enabled: true
    data:
      AzureWebJobsStorage: ""
      BlobStorage: ""
      ServiceBus: ""
      DiscordBotToken: ""

  env:
    - name: FUNCTIONS_WORKER_RUNTIME
      value: "dotnet-isolated"
    - name: AzureWebJobsScriptRoot
      value: "/home/site/wwwroot"

  envFromSecret: {}

  configMap:
    enabled: true
    mountPath: /home/site/wwwroot/appsettings.json
    subPath: appsettings.json
    data:
      appsettings.json: |
        {
          "BlobStorageAudioContainerName": "audio",
          "BlobStorageLocksContainerName": "locks"
        }

  resources:
    limits:
      cpu: 500m
      memory: 512Mi
    requests:
      cpu: 100m
      memory: 128Mi

  nodeSelector:
    kubernetes.io/arch: amd64
```

- [ ] **Step 6: Create deploy.ps1**

Note: Platforms set to `linux/amd64` only (no arm64 due to native deps).

```powershell
param(
    [switch]$Force,
    [switch]$SkipBuild,
    [switch]$AutoApprove
)

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot/../shared/scripts/Deploy-Common.psm1" -Force
Import-Module "$PSScriptRoot/../shared/scripts/Build-DockerImage.psm1" -Force

Write-Host "=== Voice Functions Deployment ===" -ForegroundColor Cyan

Assert-EnvironmentVariables -Variables @(
    "TF_VAR_registry_password",
    "TF_VAR_azure_storage_connection_string",
    "TF_VAR_azure_service_bus_connection_string",
    "TF_VAR_discord_bot_token"
)
Set-KubernetesContext -Context "t3-k3s"

if (-not $SkipBuild) {
    Build-AndPushImage `
        -ImageName "mudakai-voice-functions" `
        -SourcePath "$PSScriptRoot/../../src/MudakAI.Net" `
        -DockerfilePath "MudakAI.Voice.Functions/Dockerfile" `
        -Platforms "linux/amd64" `
        -Force:$Force
}

Update-HelmDependencies -HelmPath "$PSScriptRoot/helm"
Invoke-TerraformDeploy -Namespace "mudakai" -AutoApprove:$AutoApprove

Write-Host "`nDone!" -ForegroundColor Green
```

- [ ] **Step 7: Commit**

```bash
git add infrastructure/voice-functions/
git commit -m "feat: add voice-functions k3s deployment (amd64 only)"
```

---

### Task 9: Chat.WebService (chat-webservice)

**Files:**
- Create: `infrastructure/chat-webservice/deploy.ps1`
- Create: `infrastructure/chat-webservice/main.tf`
- Create: `infrastructure/chat-webservice/providers.tf`
- Create: `infrastructure/chat-webservice/variables.tf`
- Create: `infrastructure/chat-webservice/helm/Chart.yaml`
- Create: `infrastructure/chat-webservice/helm/values.yaml`

- [ ] **Step 1: Create providers.tf**

Same pattern, key: `"mudakai/chat-webservice.tfstate"`

- [ ] **Step 2: Create variables.tf**

```hcl
variable "kubernetes_context" {
  description = "Kubernetes context to use"
  type        = string
  default     = "t3-k3s"
}

variable "namespace" {
  description = "Kubernetes namespace"
  type        = string
  default     = "mudakai"
}

variable "app_name" {
  description = "Application name"
  type        = string
  default     = "chat-webservice"
}

variable "registry_password" {
  description = "Private container registry password"
  type        = string
  sensitive   = true
}

variable "discord_bot_token" {
  description = "Discord bot token"
  type        = string
  sensitive   = true
}

variable "openai_api_key" {
  description = "OpenAI API key"
  type        = string
  sensitive   = true
}

variable "azure_storage_connection_string" {
  description = "Azure Storage connection string"
  type        = string
  sensitive   = true
}
```

- [ ] **Step 3: Create main.tf**

```hcl
locals {
  registry_dockerconfig = base64encode(jsonencode({
    auths = {
      "registry-docker-registry.container-registry.svc.cluster.local:5000" = {
        username = "admin"
        password = var.registry_password
        auth     = base64encode("admin:${var.registry_password}")
      }
    }
  }))
}

module "chat_webservice" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace       = var.namespace
  app_name        = var.app_name
  helm_chart_path = "${path.module}/helm"

  helm_set_values = {
    "app.deploymentTimestamp"        = timestamp()
    "app.imagePullSecrets[0].name"   = "registry-credentials"
    "app.imagePullSecrets[0].create" = "true"
  }

  helm_set_sensitive_values = {
    "app.imagePullSecrets[0].dockerconfigjson"                                    = local.registry_dockerconfig
    "app.secret.data.DiscordBotToken"                                             = var.discord_bot_token
    "app.secret.data.OpenAIApiKey"                                                = var.openai_api_key
    "app.secret.data.TableStorageConnectionString"                                = var.azure_storage_connection_string
    "app.envFromSecret.Discord__DiscordBotToken.secretName"                       = var.app_name
    "app.envFromSecret.Discord__DiscordBotToken.key"                              = "DiscordBotToken"
    "app.envFromSecret.OpenAI__OpenAIApiKey.secretName"                           = var.app_name
    "app.envFromSecret.OpenAI__OpenAIApiKey.key"                                  = "OpenAIApiKey"
    "app.envFromSecret.ConnectionStrings__TableStorage.secretName"                = var.app_name
    "app.envFromSecret.ConnectionStrings__TableStorage.key"                       = "TableStorageConnectionString"
  }
}
```

- [ ] **Step 4: Create helm/Chart.yaml**

```yaml
apiVersion: v2
name: chat-webservice
description: MudakAI Chat WebService (Discord bot + OpenAI)
type: application
version: 1.0.0
appVersion: "1.0.0"

dependencies:
  - name: mudakai-base-app
    version: "1.0.0"
    repository: "file://../../shared/helm/mudakai-base-app"
    alias: app
```

- [ ] **Step 5: Create helm/values.yaml**

```yaml
app:
  nameOverride: "chat-webservice"

  replicaCount: 1

  image:
    repository: mudakai-chat
    pullPolicy: Always
    tag: "latest"

  imagePullSecrets:
    - name: registry-credentials
      create: false
      dockerconfigjson: ""

  dapr:
    enabled: true
    appId: "mudakaichat-webservice"
    appPort: "8080"

  service:
    enabled: true
    type: ClusterIP
    port: 80
    targetPort: 8080

  secret:
    enabled: true
    data:
      DiscordBotToken: ""
      OpenAIApiKey: ""
      TableStorageConnectionString: ""

  env:
    - name: ASPNETCORE_ENVIRONMENT
      value: "Production"
    - name: ASPNETCORE_URLS
      value: "http://+:8080"

  envFromSecret: {}

  configMap:
    enabled: true
    mountPath: /app/appsettings.json
    subPath: appsettings.json
    data:
      appsettings.json: |
        {
          "OpenAI": {
            "ModelId": "gpt-4.1-mini",
            "ResponseTokensLimit": 500
          },
          "MaxHistoryDepth": 6,
          "MaxHistoryAge": "00:01:00",
          "Localization": {
            "DefaultCulture": "uk-UA",
            "SupportedCultures": ["uk-UA", "en-US"]
          },
          "Logging": {
            "LogLevel": {
              "Default": "Information",
              "Microsoft.AspNetCore": "Warning"
            }
          },
          "AllowedHosts": "*"
        }

  resources:
    limits:
      cpu: 500m
      memory: 256Mi
    requests:
      cpu: 100m
      memory: 128Mi
```

- [ ] **Step 6: Create deploy.ps1**

```powershell
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
```

- [ ] **Step 7: Commit**

```bash
git add infrastructure/chat-webservice/
git commit -m "feat: add chat-webservice k3s deployment"
```

---

## Chunk 4: Root Orchestrator and Validation

### Task 10: Root Deploy Script

**Files:**
- Create: `infrastructure/deploy-all.ps1`

- [ ] **Step 1: Create deploy-all.ps1**

```powershell
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
```

- [ ] **Step 2: Commit**

```bash
git add infrastructure/deploy-all.ps1
git commit -m "feat: add deploy-all.ps1 root orchestrator"
```

---

### Task 11: Validation

- [ ] **Step 1: Validate all Terraform configurations**

Run from each service directory:
```bash
cd infrastructure/dapr && terraform init && terraform validate
cd infrastructure/tts-api && terraform init && terraform validate
cd infrastructure/tts-functions && terraform init && terraform validate
cd infrastructure/voice-functions && terraform init && terraform validate
cd infrastructure/chat-webservice && terraform init && terraform validate
```
Expected: All return "Success! The configuration is valid."

- [ ] **Step 2: Validate all Helm charts**

```bash
helm lint infrastructure/shared/helm/mudakai-base-app/
```

For service charts, update dependencies first then lint:
```bash
cd infrastructure/tts-api/helm && helm dependency update && cd .. && helm lint helm/
cd infrastructure/tts-functions/helm && helm dependency update && cd .. && helm lint helm/
cd infrastructure/voice-functions/helm && helm dependency update && cd .. && helm lint helm/
cd infrastructure/chat-webservice/helm && helm dependency update && cd .. && helm lint helm/
```
Expected: All pass lint

- [ ] **Step 3: Dry-run Helm template rendering**

For each service chart, verify templates render correctly:
```bash
helm template test infrastructure/tts-functions/helm/
```
Expected: Valid Kubernetes YAML with correct Dapr annotations, nodeSelector, etc.

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "chore: validation cleanup"
```
