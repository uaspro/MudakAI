# MudakAI deployment

MudakAI has one Terraform root, one Azure backend key
(`mudakai/terraform.tfstate`), and one deployment command. The root owns the
`mudakai` namespace, registry pull secret, Dapr, and all four application releases.
The existing Helm release names, Kubernetes resource names, Service selectors,
Dapr app IDs, queues, secrets, image repositories, and architecture constraints
are preserved.

```text
infrastructure/
|-- deploy.ps1                  # Build images, apply the whole root, wait for rollouts
|-- deploy-all.ps1              # Compatibility alias for deploy.ps1
|-- providers.tf                # Shared backend and provider configuration
|-- main.tf                     # Namespace, registry credentials, Dapr and applications
|-- variables.tf                # Deployment inputs
|-- .terraform.lock.hcl         # Provider versions used by the existing deployments
|-- helm/<service>/             # Application-specific values and chart dependencies
`-- tests/test_infrastructure.py
```

## Dependencies

Check out `pi-shared-infrastructure` alongside this repository. MudakAI consumes
its canonical `library/scripts`, `library/modules/k8s-helm-deployment`, and
`library/helm/pi-base-app` directly. There are no local copies. The application
charts pin `pi-base-app` to `1.1.0`; their lock files and the provider lock file
are committed. Dapr remains pinned to the deployed chart version, `1.14.5`.

Dapr settings use the shared chart's `podAnnotations` support. The speech worker
continues to disable Dapr. Secret references belong to the application values;
secret contents come from sensitive Terraform inputs.

## Deploy

Use PowerShell with Terraform, Helm, kubectl, an authenticated Azure CLI session,
and access to Kubernetes context `t3-k3s`. Image builds also need Docker buildx
and SSH/SCP access to `uaspro@t3`, as required by the shared image builder.

Set these existing environment inputs without committing their values:

- `TF_VAR_registry_password`
- `TF_VAR_discord_bot_token`
- `TF_VAR_openai_api_key`
- `TF_VAR_azure_storage_connection_string`
- `TF_VAR_azure_service_bus_connection_string`

```powershell
./infrastructure/deploy.ps1 -AutoApprove

# Apply infrastructure using the already published images:
./infrastructure/deploy.ps1 -SkipBuild -AutoApprove

# Force image rebuilds and application restarts:
./infrastructure/deploy.ps1 -Force -AutoApprove
```

All images are built before the single Terraform plan/apply. Applications wait
for Dapr and registry credentials; chat is applied after the other applications.
The web images remain amd64/arm64, and the Functions images remain amd64.
Each image has its own ignored source-hash directory. Rebuilt `latest` images
are explicitly rolled out; unchanged applies do not generate timestamps or
restart workloads. `-SkipBuild -Force` restarts the existing published images.
The script fails if any application or Dapr rollout fails.

## Validate

The rendering tests require Python with PyYAML (`python -m pip install PyYAML`).
They use no real credentials and do not contact Kubernetes.

```powershell
python -m unittest discover -s infrastructure/tests -v
terraform -chdir=infrastructure init -input=false
terraform -chdir=infrastructure fmt -check
terraform -chdir=infrastructure validate
terraform -chdir=infrastructure plan -input=false
```

Tests protect immutable selectors, service identities, image paths, named ports,
Dapr invocation IDs, secret references, Functions scheduling, configuration
mounts, and the speech/playback queue contract.

## State consolidation

The five former backend keys were `mudakai/dapr.tfstate`,
`mudakai/tts-api.tfstate`, `mudakai/tts-functions.tfstate`,
`mudakai/voice-functions.tfstate`, and `mudakai/chat-webservice.tfstate`.
Their resource addresses are unchanged in the unified root. The namespace and
registry secret were previously created by kubectl and are now Terraform-owned.

The production migration on 2026-10-31 used this sequence:

1. Pull and back up all five states and the live Helm manifests outside Git.
2. Copy the Dapr state to a local combined state. Use `terraform state mv`
   with local `-state` and `-state-out` files to move each application resource
   into it, preserving its address. Keep the original backups untouched.
3. Initialize the unified backend, verify it is empty, and push the combined
   state without `-force`.
4. Import `kubernetes_namespace.this` as `mudakai` and
   `kubernetes_secret.registry_credentials` as `mudakai/registry-credentials`.
5. Verify the unified plan has no resource creations, replacements, or deletions.
   Compare complete rendered runtime manifests and secret values to production,
   allowing only chart labels, checksums, and removal of rollout timestamps.
6. Remove the migrated resource bindings from the five old states with
   `terraform state rm`. This leaves the live resources intact and gives the
   unified root sole ownership. Remove the obsolete root configurations.
7. Apply the unified root, verify rollouts and application connectivity, and
   confirm a subsequent plan has no changes.

Do not run the historical per-service roots from an older checkout: they no
longer own resources. The older design and implementation documents under
`docs/superpowers` describe the superseded layout; this README describes the
current deployment.
