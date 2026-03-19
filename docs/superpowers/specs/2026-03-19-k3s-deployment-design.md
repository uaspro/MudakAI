# MudakAI k3s Deployment Design

## Overview

Deploy all 5 MudakAI services to a home lab k3s cluster, mirroring the deployment patterns from the Pi project (`C:\Dev\Pi`). Uses Terraform + Helm for infrastructure-as-code, multi-arch Docker builds, Dapr for service mesh, and the existing private container registry.

## Services

All services deploy into a single `mudakai` namespace to simplify Dapr service discovery, secrets, and registry pull secrets.

| Service | Image | Port | Node Target | Dapr App ID | Build Arch |
|---|---|---|---|---|---|
| Chat.WebService | mudakai-chat | 8080 | any | mudakaichat-webservice | amd64 + arm64 |
| TextToSpeech.API | mudakai-tts-api | 8080 | any | mudakaitexttospeech-api | amd64 + arm64 |
| TextToSpeech.Functions | mudakai-tts-functions | 80 | any | mudakaitexttospeech-functions | amd64 + arm64 |
| Voice.Functions | mudakai-voice-functions | 80 | amd64 only | mudakaivoice-functions | amd64 only |
| text-to-speech-api (Python) | mudakai-tts-python | 3100 | arm64 only | tts-api | amd64 + arm64 |

Dapr App IDs match the values hardcoded in source code (`TextToSpeechApiService.cs`, `SpeechGenerationApiService.cs`, `docker-compose.yml`).

Voice.Functions is amd64-only because its Dockerfile has hardcoded amd64 native libraries (libopus, libsodium, libdave).

All services: 1 replica, Dapr sidecar enabled.

## Directory Structure

```
C:\Dev\MudakAI\
├── src/                              # Existing source code (unchanged)
│   ├── MudakAI.Net/
│   └── text-to-speech-api/
├── infrastructure/
│   ├── deploy-all.ps1                # Root orchestrator
│   ├── shared/
│   │   ├── scripts/
│   │   │   ├── Deploy-Common.psm1    # Copied from Pi, adapted
│   │   │   └── Build-DockerImage.psm1
│   │   ├── modules/
│   │   │   └── k8s-helm-deployment/  # Terraform module
│   │   └── helm/
│   │       └── mudakai-base-app/     # Helm library chart
│   ├── dapr/
│   │   ├── main.tf, providers.tf, variables.tf
│   │   └── deploy.ps1
│   ├── chat-webservice/
│   │   ├── deploy.ps1
│   │   ├── main.tf, providers.tf, variables.tf
│   │   └── helm/ (Chart.yaml, values.yaml)
│   ├── tts-api/
│   │   ├── deploy.ps1
│   │   ├── main.tf, providers.tf, variables.tf
│   │   └── helm/
│   ├── tts-functions/
│   │   ├── deploy.ps1
│   │   ├── main.tf, providers.tf, variables.tf
│   │   └── helm/
│   ├── voice-functions/
│   │   ├── deploy.ps1
│   │   ├── main.tf, providers.tf, variables.tf
│   │   └── helm/
│   └── tts-python-api/
│       ├── deploy.ps1
│       ├── main.tf, providers.tf, variables.tf
│       └── helm/
```

## Terraform State

- Backend: Azure Storage Account `aydevsa`, resource group `ay-dev-001`, container `tfstate`
- Keys: `mudakai/dapr.tfstate`, `mudakai/chat-webservice.tfstate`, `mudakai/tts-api.tfstate`, `mudakai/tts-functions.tfstate`, `mudakai/voice-functions.tfstate`, `mudakai/tts-python-api.tfstate`

## Shared Infrastructure

Copied from `C:\Dev\Pi\pi-shared-infrastructure` and adapted:

### PowerShell Modules

**Deploy-Common.psm1** — provides:
- `Set-KubernetesContext` — switches kubectl context to `t3-k3s`
- `Update-HelmDependencies` — updates Helm chart deps
- `Invoke-TerraformDeploy` — orchestrates init/plan/apply with confirmation
- `Assert-EnvironmentVariables` — validates required env vars
- `Invoke-StandardDeploy` — complete deployment pipeline

**Build-DockerImage.psm1** — provides:
- `Get-SourceHash` — SHA256 of source files
- `Test-SourceChanged` — checks if rebuild needed
- `Build-AndPushImage` — multi-arch build → SCP to t3 → import to k3s containerd → push to registry
- Python TTS service must pass custom `IncludePatterns` (`@("*.py", "requirements.txt", "*.yaml", "Dockerfile")`) since defaults only cover .NET files

### Terraform Module (k8s-helm-deployment)

Creates namespace, deploys Helm release with values from Terraform variables. Supports `helm_set_values` and `helm_set_sensitive_values`.

### Helm Library Chart (mudakai-base-app)

Based on `pi-base-app`. Provides templates for:
- Deployment (with Dapr annotations, nodeSelector, resource limits)
- Service (ClusterIP)
- ConfigMap (for appsettings.json)
- Secret (for sensitive config)
- PVC (if persistence needed)
- Registry pull secret

## Deployment Flow

### deploy-all.ps1

Validates all required env vars, then deploys in order:

1. **Dapr** — Terraform-managed Helm release of `dapr/dapr` into `dapr-system` namespace
2. **tts-python-api** — Python TTS with arm64 nodeSelector
3. **tts-api** — .NET TTS orchestration
4. **tts-functions** — TTS Azure Functions
5. **voice-functions** — Voice Azure Functions
6. **chat-webservice** — Main chat service (deployed last, depends on others)

Each service step: `Build-AndPushImage` → `Invoke-TerraformDeploy`

### Per-Service deploy.ps1

Each service has a standalone deploy.ps1 for independent deployment.

## Required Environment Variables

- `TF_VAR_registry_password` — private registry password
- `TF_VAR_discord_bot_token` — Discord bot authentication
- `TF_VAR_openai_api_key` — OpenAI API key for chat
- `TF_VAR_openai_tts_model_id` — OpenAI TTS model identifier
- `TF_VAR_azure_storage_connection_string` — Azure Storage (blobs, tables, queues)
- `TF_VAR_azure_service_bus_connection_string` — Azure Service Bus for async messaging

## Per-Service Configuration Matrix

### Chat.WebService
| Env Var / Config Key | Source | Type |
|---|---|---|
| `ConnectionStrings__TableStorage` | `TF_VAR_azure_storage_connection_string` | secret |
| `Discord__DiscordBotToken` | `TF_VAR_discord_bot_token` | secret |
| `OpenAI__OpenAIApiKey` | `TF_VAR_openai_api_key` | secret |
| `OpenAI__ModelId` | `"gpt-4.1-mini"` | configmap |
| `OpenAI__ResponseTokensLimit` | `500` | configmap |
| `MaxHistoryDepth` | `6` | configmap |
| `MaxHistoryAge` | `"00:01:00"` | configmap |

### TextToSpeech.API
| Env Var / Config Key | Source | Type |
|---|---|---|
| `ConnectionStrings__ServiceBus` | `TF_VAR_azure_service_bus_connection_string` | secret |
| `TextToSpeechQueueName` | `"text-to-speech"` | configmap |

### TextToSpeech.Functions
| Env Var / Config Key | Source | Type |
|---|---|---|
| `AzureWebJobsStorage` | `TF_VAR_azure_storage_connection_string` | secret |
| `FUNCTIONS_WORKER_RUNTIME` | `"dotnet-isolated"` | env |
| `BlobStorage` | `TF_VAR_azure_storage_connection_string` | secret |
| `ServiceBusListen` | `TF_VAR_azure_service_bus_connection_string` | secret |
| `ServiceBusSend` | `TF_VAR_azure_service_bus_connection_string` | secret |
| `ServiceBusPlaybackQueueName` | `"playback"` | configmap |
| `OpenAI__OpenAIApiKey` | `TF_VAR_openai_api_key` | secret |
| `OpenAI__TTSModelId` | `TF_VAR_openai_tts_model_id` | configmap |
| `BlobStorageAudioContainerName` | `"audio"` | configmap |

### Voice.Functions
| Env Var / Config Key | Source | Type |
|---|---|---|
| `AzureWebJobsStorage` | `TF_VAR_azure_storage_connection_string` | secret |
| `FUNCTIONS_WORKER_RUNTIME` | `"dotnet-isolated"` | env |
| `BlobStorage` | `TF_VAR_azure_storage_connection_string` | secret |
| `ServiceBus` | `TF_VAR_azure_service_bus_connection_string` | secret |
| `DiscordBotToken` | `TF_VAR_discord_bot_token` | secret |
| `BlobStorageAudioContainerName` | `"audio"` | configmap |
| `BlobStorageLocksContainerName` | `"locks"` | configmap |

### text-to-speech-api (Python)
No secrets or configuration required. Uses default config.yaml baked into the Docker image.

## Secrets Management

Secrets flow: shell env vars → Terraform variables → `helm_set_sensitive_values` → Kubernetes Secrets → pod env vars (via `envFromSecret` in Helm values).

## Docker Builds

Images built using `docker buildx` with existing Dockerfiles from `src/`. Azure Functions services keep their Azure Functions base images.

- **Multi-arch (amd64 + arm64):** Chat.WebService, TextToSpeech.API, TextToSpeech.Functions, text-to-speech-api
- **amd64-only:** Voice.Functions (hardcoded amd64 native libraries: libopus, libsodium, libdave)

.NET Dockerfiles use build context `src/MudakAI.Net/`. Python TTS uses build context `src/text-to-speech-api/`.

## Node Targeting

- **text-to-speech-api (Python):** `nodeSelector: { kubernetes.io/arch: arm64 }`
- **Voice.Functions:** `nodeSelector: { kubernetes.io/arch: amd64 }` (amd64 native deps)
- **All other services:** No nodeSelector (schedule on any available node)

## Dapr Configuration

- Installed via Terraform-managed Helm release (`dapr/dapr`) into `dapr-system` namespace
- Dapr is used only for service-to-service invocation — no Dapr component CRDs are needed
- Each service pod gets Dapr sidecar via annotations:
  - `dapr.io/enabled: "true"`
  - `dapr.io/app-id: "<service-dapr-id>"`
  - `dapr.io/app-port: "<service-port>"`

## Container Registry

Reuses existing private registry from Pi project:
- Endpoint: `registry-docker-registry.container-registry.svc.cluster.local:5000`
- Auth: admin / `TF_VAR_registry_password`
- Image pull secret created in `mudakai` namespace via Helm template
