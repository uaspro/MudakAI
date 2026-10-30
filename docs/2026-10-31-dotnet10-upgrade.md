# .NET 10 LTS upgrade

The applications and Discord connector target .NET 10. The other connectors keep
their existing .NET Standard 2.1 targets. The SDK is pinned to 10.0.403, and the
deployment images use the .NET 10 LTS image tags (10.0.14 when verified).

All direct NuGet references were checked against the latest stable versions on
2026-10-31. MediatR is intentionally aligned to 12.5.0 in both projects, as
approved, to retain the Apache license instead of introducing the licensing
requirements in 13+. Third-party packages generally do not have an LTS channel.

Compatibility changes:

- Use the stable OpenAI audio client for speech. The updated Semantic Kernel
  speech connector rejects the existing `ash` and `coral` voices. The bot keeps
  its voice mapping, configured model, and WAV response format.
- Use typed chat execution settings to retain the configured response token
  limit with the updated Semantic Kernel/OpenAI SDKs.
- The speech worker uses OpenAI directly and does not require Dapr.
- Install voice dependencies from the Ubuntu 24.04 LTS Functions image, replacing
  the previous Debian package mixing. Keep libdave pinned to 1.1.1.

Validation:

```powershell
dotnet test --project tests/MudakAI.Tests/MudakAI.Tests.csproj -c Release
dotnet list tests/MudakAI.Tests/MudakAI.Tests.csproj package --include-transitive --vulnerable
```

The 10 tests cover the HTTP-to-Service-Bus contract, malformed JSON, required
configuration, Swagger, playback message compatibility, chat settings/history,
and OpenAI voice selection/WAV bytes. They use fake external services and do not
post to Discord or production queues.

Both Functions projects generate identical trigger metadata to the .NET 8 build.
All four deployment images build successfully; the web images retain amd64 and
arm64 support, and the Functions images use amd64. Native voice libraries resolve
without missing dependencies, and FFmpeg starts successfully.

Existing Dapr invocation methods remain usable but now emit deprecation warnings.
The existing nullable-return warning in TableStorageService is unchanged.

Deployment verification on `t3-k3s`, namespace `mudakai`:

- All four Terraform/Helm deployments completed; all pods are ready with zero
  restarts and report .NET 10.0.14.
- Running image digests match the built images published with release tags and
  `latest`. The TTS API is running on the ARM64 Raspberry Pi node.
- Chat connected to Discord and registered both guilds. The voice connection
  timer succeeded, and the TTS worker initialized its Service Bus trigger.
- Malformed speech requests return HTTP 400 both directly and through Dapr.
- NuGet reports no known vulnerabilities, including generated Function extension
  dependencies. Live Discord audio playback was not exercised.
