# .NET 8.0 Upgrade Plan for MudakAI.Chat.WebService

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Overview
This plan outlines the migration of **MudakAI.Chat.WebService** from .NET 6.0 to .NET 8.0, along with necessary updates to its dependent projects to ensure compatibility and leverage .NET 8.0 improvements.

### Scope
- **Primary Target**: MudakAI.Chat.WebService (net6.0 → net8.0)
- **Affected Projects**: 7 projects in solution
- **Already on .NET 8.0**: 3 projects (MudakAI.Connectors.Discord, MudakAI.TextToSpeech.Functions, MudakAI.Voice.Functions)
- **Remaining on .NET Standard 2.1**: 3 class libraries (compatible with .NET 8.0)

### Key Changes Required
1. **Target Framework Update**: Change MudakAI.Chat.WebService from `net6.0` to `net8.0`
2. **Package Updates**: 
   - Replace deprecated `MediatR.Extensions.Microsoft.DependencyInjection` with integrated MediatR functionality
   - Upgrade `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` from 1.19.4 to 1.21.0+
   - Update `Microsoft.Extensions.Azure` in dependent libraries (1.6.3/1.7.0 → 1.13.1)
   - Update `Microsoft.Extensions.DependencyInjection.Abstractions` (8.0.1 → 8.0.2)
3. **Azure Functions Enhancements**: Enable V2 model and Application Insights for Functions projects

### Complexity Assessment
- **Overall Complexity**: ⭐⭐ Low-Medium (2/5)
- **Estimated Effort**: 2-3 hours
- **Risk Level**: Low - Most changes are straightforward package updates

### Migration Strategy
**All-at-Once Coordinated Update** - Since most projects are already on .NET 8.0 and the solution is relatively small, we'll upgrade all affected components in a single coordinated effort.

## Migration Strategy

### Recommended Approach: All-at-Once Coordinated Update

Given the solution's characteristics, an all-at-once strategy is recommended:

**Rationale:**
- ✅ **Small Solution Size**: Only 7 projects, 3 already on .NET 8.0
- ✅ **Low Complexity**: Primarily package updates with no breaking API changes
- ✅ **Minimal Dependencies**: Clean dependency tree with no circular dependencies
- ✅ **Already Partially Migrated**: Most infrastructure already on .NET 8.0

### Migration Order

The upgrade will follow project dependency order from foundation to application layers:

#### Phase 1: Foundation Libraries (Level 0)
These libraries have no project dependencies and are used by higher-level projects:

1. **MudakAI.Connectors.Azure.Table** (netstandard2.1 - stays)
   - Update: `Microsoft.Extensions.Azure` 1.7.0 → 1.13.1

2. **MudakAI.Connectors.Azure.Blob** (netstandard2.1 - stays)
   - Update: `Microsoft.Extensions.Azure` 1.6.3 → 1.13.1

3. **MudakAI.Connectors.OpenAI** (netstandard2.1 - stays)
   - Update: `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.1 → 8.0.2

4. **MudakAI.Connectors.Discord** (net8.0 - already compatible)
   - No changes needed

#### Phase 2: Application Layer (Level 1)
These projects depend on foundation libraries:

5. **MudakAI.Chat.WebService** (net6.0 → net8.0) ⚡ **PRIMARY TARGET**
   - Update target framework: `net6.0` → `net8.0`
   - Remove: `MediatR.Extensions.Microsoft.DependencyInjection` 11.1.0
   - Add: `MediatR` package (if not already present)
   - Update: `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` 1.19.4 → 1.21.0+
   - Verify: `Dapr.AspNetCore` and `FluentValidation` compatibility

6. **MudakAI.TextToSpeech.Functions** (net8.0 - enhancement)
   - Enable Azure Functions V2 model
   - Enable Application Insights

7. **MudakAI.Voice.Functions** (net8.0 - enhancement)
   - Enable Azure Functions V2 model
   - Enable Application Insights

### Why Keep .NET Standard 2.1 Libraries?

The connector libraries (Azure.Table, Azure.Blob, OpenAI) will remain on .NET Standard 2.1 because:
- ✅ .NET Standard 2.1 is fully compatible with .NET 8.0
- ✅ Maintains broader compatibility if needed for other consumers
- ✅ No .NET 8.0-specific features required in these simple connector libraries
- ✅ Reduces testing surface area

### Rollback Strategy

If issues arise:
1. **Git Rollback**: All changes are in branch `upgrade-to-NET8`, can revert to `master`
2. **Per-Project Rollback**: Each project file change is committed separately for granular rollback
3. **Package Rollback**: NuGet packages can be downgraded if compatibility issues arise

## Detailed Dependency Analysis

### Project Dependency Graph

```
Level 0 (Foundation - No Dependencies):
├── MudakAI.Connectors.Azure.Blob (netstandard2.1)
│   └── Used by: MudakAI.Voice.Functions, MudakAI.TextToSpeech.Functions
├── MudakAI.Connectors.Azure.Table (netstandard2.1)
│   └── Used by: MudakAI.Chat.WebService
├── MudakAI.Connectors.Discord (net8.0) ✅
│   └── Used by: MudakAI.Chat.WebService, MudakAI.Voice.Functions
└── MudakAI.Connectors.OpenAI (netstandard2.1)
    └── Used by: MudakAI.Chat.WebService, MudakAI.TextToSpeech.Functions

Level 1 (Application Layer):
├── MudakAI.Chat.WebService (net6.0 → net8.0) ⚡ PRIMARY TARGET
│   ├── Dependencies: Connectors.Discord, Connectors.OpenAI, Connectors.Azure.Table
│   └── Issues: 3 (2 mandatory, 1 optional)
├── MudakAI.TextToSpeech.Functions (net8.0) ✅
│   ├── Dependencies: Connectors.OpenAI, Connectors.Azure.Blob
│   └── Issues: 1 (V2 model enhancement)
└── MudakAI.Voice.Functions (net8.0) ✅
    ├── Dependencies: Connectors.Discord, Connectors.Azure.Blob
    └── Issues: 1 (V2 model enhancement)

Standalone Applications:
├── MudakAI.TextToSpeech.API (net8.0) ✅ - No changes needed
└── docker-compose.dcproj - No changes needed
```

### Cross-Project Package Dependencies

| Package | Projects Using | Versions | Action Required |
|---------|---------------|----------|-----------------|
| **Microsoft.Extensions.Azure** | Azure.Table, Azure.Blob | 1.6.3, 1.7.0 → 1.13.1 | Update (deprecated versions) |
| **MediatR.Extensions.Microsoft.DependencyInjection** | Chat.WebService | 11.1.0 | Remove (deprecated) |
| **Microsoft.VisualStudio.Azure.Containers.Tools.Targets** | Chat.WebService | 1.19.4 → 1.21.0+ | Update (incompatible) |
| **Microsoft.Extensions.DependencyInjection.Abstractions** | Connectors.OpenAI | 8.0.1 → 8.0.2 | Update (recommended) |
| **Dapr.AspNetCore** | Chat.WebService | 1.11.0 | Verify compatibility |
| **FluentValidation** | Chat.WebService | 11.7.1 | Verify compatibility |

### Package Compatibility Matrix

| Package | Current | Target | .NET 8.0 Compatible? | Action |
|---------|---------|--------|---------------------|---------|
| Dapr.AspNetCore | 1.11.0 | - | ✅ Yes | No change |
| FluentValidation | 11.7.1 | - | ✅ Yes | No change |
| MediatR (implicit) | 11.1.0 | - | ✅ Yes | Ensure explicit reference |
| MediatR.Extensions.* | 11.1.0 | Remove | ⚠️ Deprecated | Remove package |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.19.4 | 1.21.0+ | ❌ No | Must upgrade |
| Microsoft.Extensions.Azure | 1.6.3/1.7.0 | 1.13.1 | ⚠️ Deprecated | Should upgrade |
| Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.1 | 8.0.2 | ⚠️ Minor update | Recommended |
| Azure.Data.Tables | 12.8.1 | - | ✅ Yes | No change |
| Discord.Net | 3.19.1 | - | ✅ Yes | No change |
| Microsoft.SemanticKernel.Connectors.OpenAI | 1.17.1 | - | ✅ Yes | No change |
| Azure Functions packages | Various | - | ✅ Yes | No change |

## Project-by-Project Plans

### 1. MudakAI.Connectors.Azure.Table

**Current State**: netstandard2.1  
**Target State**: netstandard2.1 (no framework change)  
**Complexity**: ⭐ Very Low  
**Estimated Time**: 10 minutes

#### Changes Required
- ✅ Update `Microsoft.Extensions.Azure` from 1.7.0 to 1.13.1

#### Steps
1. Edit `MudakAI.Connectors.Azure.Table.csproj`
2. Update package reference:
   ```xml
   <PackageReference Include="Microsoft.Extensions.Azure" Version="1.13.1" />
   ```
3. Restore packages: `dotnet restore`
4. Build project: `dotnet build`
5. Commit changes

#### Validation
- ✅ Project builds successfully
- ✅ No new warnings introduced
- ✅ Dependent projects (Chat.WebService) still reference correctly

---

### 2. MudakAI.Connectors.Azure.Blob

**Current State**: netstandard2.1  
**Target State**: netstandard2.1 (no framework change)  
**Complexity**: ⭐ Very Low  
**Estimated Time**: 10 minutes

#### Changes Required
- ✅ Update `Microsoft.Extensions.Azure` from 1.6.3 to 1.13.1

#### Steps
1. Edit `MudakAI.Connectors.Azure.Blob.csproj`
2. Update package reference:
   ```xml
   <PackageReference Include="Microsoft.Extensions.Azure" Version="1.13.1" />
   ```
3. Restore packages: `dotnet restore`
4. Build project: `dotnet build`
5. Commit changes

#### Validation
- ✅ Project builds successfully
- ✅ No new warnings introduced
- ✅ Dependent projects (Voice.Functions, TextToSpeech.Functions) still reference correctly

---

### 3. MudakAI.Connectors.OpenAI

**Current State**: netstandard2.1  
**Target State**: netstandard2.1 (no framework change)  
**Complexity**: ⭐ Very Low  
**Estimated Time**: 10 minutes

#### Changes Required
- ✅ Update `Microsoft.Extensions.DependencyInjection.Abstractions` from 8.0.1 to 8.0.2

#### Steps
1. Edit `MudakAI.Connectors.OpenAI.csproj`
2. Update package reference:
   ```xml
   <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.2" />
   ```
3. Restore packages: `dotnet restore`
4. Build project: `dotnet build`
5. Commit changes

#### Validation
- ✅ Project builds successfully
- ✅ No new warnings introduced
- ✅ Semantic Kernel integration still works (verify no API changes)

---

### 4. MudakAI.Connectors.Discord

**Current State**: net8.0  
**Target State**: net8.0 (already migrated)  
**Complexity**: N/A  
**Estimated Time**: N/A

#### Changes Required
- ✅ None - already on .NET 8.0

---

### 5. MudakAI.Chat.WebService ⚡ PRIMARY TARGET

**Current State**: net6.0  
**Target State**: net8.0  
**Complexity**: ⭐⭐ Low-Medium  
**Estimated Time**: 45-60 minutes

#### Changes Required
1. ✅ Update target framework: `net6.0` → `net8.0`
2. ✅ Remove deprecated `MediatR.Extensions.Microsoft.DependencyInjection` package
3. ✅ Ensure `MediatR` package is explicitly referenced
4. ✅ Update `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` from 1.19.4 to 1.21.0+
5. ⚠️ Review and update DI registration for MediatR (API changed)

#### Steps

**Step 1: Update Project File**
1. Edit `MudakAI.Chat.WebService.csproj`
2. Change target framework:
   ```xml
   <TargetFramework>net8.0</TargetFramework>
   ```
3. Remove deprecated package:
   ```xml
   <!-- REMOVE THIS LINE -->
   <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />
   ```
4. Add/verify MediatR package (if not already present):
   ```xml
   <PackageReference Include="MediatR" Version="12.0.0" />
   ```
5. Update container tools:
   ```xml
   <PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" Version="1.21.0" />
   ```

**Step 2: Update MediatR Registration**
1. Open `Program.cs` (or `Startup.cs` if using .NET 6 style)
2. Update MediatR service registration:
   ```csharp
   // OLD (.NET 6 with MediatR.Extensions):
   // services.AddMediatR(typeof(Program).Assembly);

   // NEW (.NET 8 with MediatR 12+):
   services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
   ```

**Step 3: Verify Other .NET 8 Enhancements**
1. Review `Program.cs` for potential .NET 8 improvements:
   - Consider using top-level statements if not already
   - Review minimal API patterns
   - Check logging configuration for new .NET 8 patterns

**Step 4: Build and Test**
1. Restore packages: `dotnet restore`
2. Build: `dotnet build`
3. Fix any compilation errors
4. Run application locally
5. Test key endpoints
6. Commit changes

#### Validation
- ✅ Project builds without errors
- ✅ All MediatR commands/queries still work
- ✅ Application starts successfully
- ✅ Docker container builds (verify with `DockerDefaultTargetOS`)
- ✅ Dapr integration still works
- ✅ FluentValidation still works
- ✅ No new runtime exceptions

#### Breaking Changes to Watch
- **MediatR 12+ API Changes**: Registration syntax changed from `AddMediatR(assembly)` to `AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly))`
- **ASP.NET Core 8.0**: Generally backwards compatible from .NET 6, but review:
  - Authentication/authorization changes
  - Minimal API enhancements
  - New rate limiting features (optional to adopt)

---

### 6. MudakAI.TextToSpeech.Functions

**Current State**: net8.0  
**Target State**: net8.0 (enhancement)  
**Complexity**: ⭐ Low  
**Estimated Time**: 20 minutes

#### Changes Required
- ✅ Enable Azure Functions V2 model
- ✅ Enable Application Insights

#### Steps
1. Edit `MudakAI.TextToSpeech.Functions.csproj`
2. Add properties to enable V2 model:
   ```xml
   <PropertyGroup>
     <AzureFunctionsVersion>v4</AzureFunctionsVersion>
     <OutputType>Exe</OutputType>
   </PropertyGroup>
   ```
3. Verify `Program.cs` uses isolated worker model (should already be present based on assessment)
4. Add Application Insights configuration in `host.json`:
   ```json
   {
     "version": "2.0",
     "logging": {
       "applicationInsights": {
         "samplingSettings": {
           "isEnabled": true,
           "maxTelemetryItemsPerSecond": 20
         }
       }
     }
   }
   ```
5. Add Application Insights package if not present:
   ```xml
   <PackageReference Include="Microsoft.Azure.Functions.Worker.ApplicationInsights" Version="1.0.0" />
   ```
6. Build and test

#### Validation
- ✅ Function app builds successfully
- ✅ Functions can be triggered locally
- ✅ Application Insights telemetry is captured (if configured)

---

### 7. MudakAI.Voice.Functions

**Current State**: net8.0  
**Target State**: net8.0 (enhancement)  
**Complexity**: ⭐ Low  
**Estimated Time**: 20 minutes

#### Changes Required
- ✅ Enable Azure Functions V2 model
- ✅ Enable Application Insights

#### Steps
*Same as MudakAI.TextToSpeech.Functions above*

1. Edit `MudakAI.Voice.Functions.csproj`
2. Add V2 model properties
3. Verify isolated worker model in `Program.cs`
4. Configure Application Insights in `host.json`
5. Add Application Insights package
6. Build and test

#### Validation
- ✅ Function app builds successfully
- ✅ Functions can be triggered locally
- ✅ Application Insights telemetry is captured (if configured)

---

### 8. MudakAI.TextToSpeech.API

**Current State**: net8.0  
**Target State**: net8.0  
**Complexity**: N/A  
**Estimated Time**: N/A

#### Changes Required
- ✅ None - already on .NET 8.0 and no issues detected

## Package Update Reference

### Critical Package Updates (Mandatory)

#### 1. Microsoft.VisualStudio.Azure.Containers.Tools.Targets
- **Project**: MudakAI.Chat.WebService
- **Current**: 1.19.4
- **Target**: 1.21.0 or later
- **Severity**: ❌ Incompatible with .NET 8.0
- **Reason**: Version 1.19.4 does not support .NET 8.0 Docker container builds
- **Migration Guide**: Direct package version update, no code changes required

#### 2. MediatR.Extensions.Microsoft.DependencyInjection
- **Project**: MudakAI.Chat.WebService
- **Current**: 11.1.0
- **Target**: Remove package
- **Severity**: ⚠️ Deprecated
- **Reason**: Functionality integrated into MediatR 12.0+
- **Migration Guide**:
  ```csharp
  // OLD Registration (with MediatR.Extensions.Microsoft.DependencyInjection):
  services.AddMediatR(typeof(Program).Assembly);

  // NEW Registration (with MediatR 12.0+):
  services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
  ```
- **Documentation**: https://github.com/jbogard/MediatR/releases/tag/v12.0.0

### Important Package Updates (Recommended)

#### 3. Microsoft.Extensions.Azure
- **Projects**: 
  - MudakAI.Connectors.Azure.Table (1.7.0 → 1.13.1)
  - MudakAI.Connectors.Azure.Blob (1.6.3 → 1.13.1)
- **Severity**: ⚠️ Deprecated versions
- **Reason**: Old versions depend on deprecated Azure.Identity versions
- **Migration Guide**: Direct package version update
- **Breaking Changes**: None expected (minor version updates)

#### 4. Microsoft.Extensions.DependencyInjection.Abstractions
- **Project**: MudakAI.Connectors.OpenAI
- **Current**: 8.0.1
- **Target**: 8.0.2
- **Severity**: ℹ️ Recommended patch update
- **Reason**: Bug fixes and improvements
- **Migration Guide**: Direct package version update, no code changes

### Azure Functions Enhancement Packages

#### 5. Microsoft.Azure.Functions.Worker.ApplicationInsights
- **Projects**: 
  - MudakAI.TextToSpeech.Functions
  - MudakAI.Voice.Functions
- **Current**: Not installed
- **Target**: 1.0.0 or later
- **Severity**: ✨ Enhancement (optional but recommended)
- **Reason**: Enable Application Insights telemetry in isolated worker model
- **Migration Guide**: Add package and configure in `host.json`

### Packages Verified Compatible (No Changes)

| Package | Version | Projects | Status |
|---------|---------|----------|--------|
| Dapr.AspNetCore | 1.11.0, 1.13.1 | Chat.WebService, Voice.Functions | ✅ Compatible |
| FluentValidation | 11.7.1, 11.9.2 | Chat.WebService, Voice.Functions | ✅ Compatible |
| MediatR | 11.1.0 | Chat.WebService (transitive) | ✅ Compatible (upgrade to 12.0+ recommended) |
| Discord.Net | 3.19.1 | Connectors.Discord | ✅ Compatible |
| Azure.Data.Tables | 12.8.1 | Connectors.Azure.Table | ✅ Compatible |
| Azure.Storage.Blobs | 12.16.0, 12.19.1 | Connectors.Azure.Blob | ✅ Compatible |
| Microsoft.SemanticKernel.Connectors.OpenAI | 1.17.1 | Connectors.OpenAI | ✅ Compatible |
| Microsoft.Azure.Functions.Worker | 1.21.0 | Functions projects | ✅ Compatible |
| Azure.Messaging.ServiceBus | 7.18.1 | Functions projects | ✅ Compatible |

### Package Update Commands

```bash
# MudakAI.Connectors.Azure.Table
dotnet add MudakAI.Connectors.Azure.Table/MudakAI.Connectors.Azure.Table.csproj package Microsoft.Extensions.Azure --version 1.13.1

# MudakAI.Connectors.Azure.Blob
dotnet add MudakAI.Connectors.Azure.Blob/MudakAI.Connectors.Azure.Blob.csproj package Microsoft.Extensions.Azure --version 1.13.1

# MudakAI.Connectors.OpenAI
dotnet add MudakAI.Connectors.OpenAI/MudakAI.Connectors.OpenAI.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.2

# MudakAI.Chat.WebService
dotnet remove MudakAI.Chat.WebService/MudakAI.Chat.WebService.csproj package MediatR.Extensions.Microsoft.DependencyInjection
dotnet add MudakAI.Chat.WebService/MudakAI.Chat.WebService.csproj package MediatR --version 12.0.0
dotnet add MudakAI.Chat.WebService/MudakAI.Chat.WebService.csproj package Microsoft.VisualStudio.Azure.Containers.Tools.Targets --version 1.21.0

# Azure Functions (optional Application Insights)
dotnet add MudakAI.TextToSpeech.Functions/MudakAI.TextToSpeech.Functions.csproj package Microsoft.Azure.Functions.Worker.ApplicationInsights --version 1.0.0
dotnet add MudakAI.Voice.Functions/MudakAI.Voice.Functions.csproj package Microsoft.Azure.Functions.Worker.ApplicationInsights --version 1.0.0
```

## Breaking Changes Catalog

### .NET 8.0 Framework Changes (from .NET 6.0)

#### ASP.NET Core Breaking Changes

**Low Impact - Unlikely to Affect MudakAI.Chat.WebService:**

1. **Authentication/Authorization**
   - Changes to `AuthenticationProperties` serialization (only affects custom authentication schemes)
   - **Impact**: None expected - using standard Dapr authentication

2. **Minimal APIs**
   - Enhanced parameter binding (backwards compatible)
   - **Impact**: None - only adds new features

3. **Rate Limiting**
   - New built-in rate limiting middleware
   - **Impact**: None - opt-in feature

4. **Request Decompression**
   - New middleware for automatic request decompression
   - **Impact**: None - opt-in feature

5. **HTTP/3**
   - Improved HTTP/3 support
   - **Impact**: None - backwards compatible

**Mitigation**: None required - all changes are additive or affect unused features

#### MediatR 12.0 Breaking Changes

**Medium Impact - Requires Code Changes:**

1. **Service Registration API Change**
   - **What Changed**: Extension method signature changed
   - **Old Code**:
     ```csharp
     services.AddMediatR(typeof(Program).Assembly);
     ```
   - **New Code**:
     ```csharp
     services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
     ```
   - **Impact**: Build error until updated
   - **Files Affected**: `Program.cs` or `Startup.cs`
   - **Mitigation**: Update registration call (1-line change)

2. **Pipeline Behavior Registration**
   - **What Changed**: Pipeline behaviors now registered via configuration
   - **Old Code**:
     ```csharp
     services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MyBehavior<,>));
     ```
   - **New Code**:
     ```csharp
     services.AddMediatR(cfg => {
         cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
         cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MyBehavior<,>));
     });
     ```
   - **Impact**: Only if using custom pipeline behaviors
   - **Mitigation**: Update behavior registration if present

**Reference**: https://github.com/jbogard/MediatR/releases/tag/v12.0.0

### Package-Specific Breaking Changes

#### Microsoft.Extensions.Azure 1.13.1

**No Breaking Changes:**
- Version 1.6.3 → 1.13.1: Only dependency updates and bug fixes
- Version 1.7.0 → 1.13.1: Only dependency updates and bug fixes
- **Impact**: None expected
- **Mitigation**: None required

#### Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.21.0

**No Code Breaking Changes:**
- Docker tooling support update
- **Impact**: Only affects Docker container build process
- **Mitigation**: None required - transparent update

### Runtime Behavior Changes

#### .NET 8.0 Performance Improvements

**Positive Changes (No Action Required):**
- Improved JSON serialization performance
- Faster Regex compilation
- Better garbage collection
- Enhanced LINQ performance

**Potential Compatibility Issues:**
- **Change**: Regex compilation cache improvements may affect memory usage patterns
- **Impact**: Minimal - only affects apps with heavy Regex use
- **Mitigation**: Monitor memory usage after upgrade

#### ASP.NET Core Kestrel Changes

**Configuration Changes:**
- New default limits and timeouts (backwards compatible)
- **Impact**: None expected
- **Mitigation**: None required - defaults are sensible

### Known Risks by Project

| Project | Breaking Change Risk | Description | Mitigation |
|---------|---------------------|-------------|------------|
| MudakAI.Chat.WebService | 🟡 Medium | MediatR API changes required | Update registration in Program.cs |
| MudakAI.Connectors.Azure.Table | 🟢 Low | Package update only | Standard testing |
| MudakAI.Connectors.Azure.Blob | 🟢 Low | Package update only | Standard testing |
| MudakAI.Connectors.OpenAI | 🟢 Low | Patch version update | Standard testing |
| MudakAI.Connectors.Discord | 🟢 None | Already on .NET 8 | No changes |
| MudakAI.TextToSpeech.Functions | 🟢 Low | Opt-in enhancements | Optional updates |
| MudakAI.Voice.Functions | 🟢 Low | Opt-in enhancements | Optional updates |

### Breaking Change Decision Tree

```
Is your project using custom MediatR pipeline behaviors?
├─ YES → Review pipeline behavior registration (medium effort)
└─ NO  → Simple registration update (low effort)

Is your project using custom authentication schemes?
├─ YES → Review authentication changes (review .NET 8 docs)
└─ NO  → No action required

Is your project using Regex heavily?
├─ YES → Monitor memory usage after upgrade
└─ NO  → No action required
```

## Risk Management

### Risk Assessment Matrix

| Risk Category | Likelihood | Impact | Severity | Mitigation Strategy |
|---------------|-----------|--------|----------|---------------------|
| **MediatR API Breaking Changes** | High | Medium | 🟡 Medium | Clear migration guide; automated tests |
| **Docker Container Build Failures** | Medium | Medium | 🟡 Medium | Update container tools first; test locally |
| **Azure Functions Runtime Issues** | Low | Medium | 🟢 Low | V2 model changes are optional; gradual rollout |
| **Package Dependency Conflicts** | Low | Low | 🟢 Low | Test in isolation; rollback available |
| **Runtime Performance Regressions** | Very Low | Low | 🟢 Low | Performance monitoring; benchmarks |
| **Azure Service Integration Issues** | Very Low | High | 🟢 Low | Dapr/Azure SDKs are stable; extensive testing |

### Risk Mitigation Strategies

#### 1. Pre-Migration Validation

**Action Items:**
- ✅ Verify .NET 8.0 SDK installed: `dotnet --list-sdks`
- ✅ Review all package compatibility before changes
- ✅ Create backup branch: `upgrade-to-NET8` (already done)
- ✅ Document current application behavior for comparison
- ✅ Prepare rollback plan

**Validation Command:**
```bash
dotnet --list-sdks | findstr "8.0"
```

#### 2. Incremental Testing Strategy

**Phase 1: Foundation Libraries**
1. Update and test Azure connector libraries individually
2. Run unit tests for each library
3. Verify no breaking changes in public APIs

**Phase 2: Main Application**
1. Update MudakAI.Chat.WebService
2. Build and resolve compilation errors
3. Run application locally
4. Test critical user flows

**Phase 3: Azure Functions**
1. Update Functions projects
2. Test locally with Azure Functions Core Tools
3. Verify bindings and triggers work

#### 3. Automated Testing Coverage

**Required Tests Before Deployment:**
- ✅ Unit tests for all projects (if available)
- ✅ Integration tests for MediatR command/query handlers
- ✅ API endpoint smoke tests
- ✅ Docker container build verification
- ✅ Dapr sidecar integration test

**Test Commands:**
```bash
# Build all projects
dotnet build MudakAI.sln

# Run all tests (if test projects exist)
dotnet test MudakAI.sln

# Test Docker build
docker build -t mudakai-chat-test -f MudakAI.Chat.WebService/Dockerfile .
```

#### 4. Deployment Safety Measures

**Git Safety:**
- ✅ All changes in dedicated branch `upgrade-to-NET8`
- ✅ Commit each project update separately for granular rollback
- ✅ Tag pre-upgrade state: `git tag pre-net8-upgrade`

**Deployment Strategy:**
- 🔄 Deploy to non-production environment first
- 🔄 Smoke test all endpoints
- 🔄 Monitor logs for errors/warnings
- 🔄 Gradual production rollout if applicable

**Rollback Plan:**
```bash
# If issues found, rollback to master
git checkout master

# Or cherry-pick successful changes
git cherry-pick <commit-sha>
```

#### 5. Monitoring and Validation Post-Deployment

**Immediate Post-Upgrade Checks:**
- ✅ Application starts without errors
- ✅ All services register correctly
- ✅ Database connections work
- ✅ Azure service connections (Storage, Service Bus) work
- ✅ Discord integration functions
- ✅ OpenAI/Semantic Kernel integration works

**Ongoing Monitoring (First 24-48 Hours):**
- 📊 Response time metrics (should improve or stay same)
- 📊 Error rates (should not increase)
- 📊 Memory usage patterns
- 📊 CPU utilization
- 📊 Exception logs

### Contingency Plans

#### Scenario 1: MediatR Registration Fails

**Symptoms:** Application fails to start, DI registration errors  
**Resolution:**
1. Review `Program.cs` registration syntax
2. Ensure MediatR 12.0+ package is installed
3. Verify assembly scanning is correct
4. Check for custom pipeline behaviors

**Fallback:** Temporarily add `MediatR.Extensions.Microsoft.DependencyInjection` back (deprecated but functional)

#### Scenario 2: Docker Container Build Fails

**Symptoms:** Docker build errors referencing .NET SDK  
**Resolution:**
1. Verify Dockerfile uses .NET 8 SDK image: `mcr.microsoft.com/dotnet/sdk:8.0`
2. Update runtime image: `mcr.microsoft.com/dotnet/aspnet:8.0`
3. Rebuild with `--no-cache` flag

**Fallback:** Revert `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` to 1.19.4 and target framework to net6.0

#### Scenario 3: Azure Functions Don't Trigger

**Symptoms:** Functions deploy but don't respond to triggers  
**Resolution:**
1. Verify `host.json` configuration
2. Check Azure Functions runtime version (v4)
3. Verify isolated worker model configuration
4. Review Application Insights settings

**Fallback:** Remove Application Insights package, revert to basic configuration

#### Scenario 4: Package Dependency Conflicts

**Symptoms:** NuGet restore errors, version conflicts  
**Resolution:**
1. Run `dotnet restore --force`
2. Clear NuGet cache: `dotnet nuget locals all --clear`
3. Check for transitive dependency conflicts
4. Use `dotnet list package --vulnerable` to check for issues

**Fallback:** Revert package updates one at a time to isolate conflict

### Success Criteria Checkpoints

Before considering migration complete, verify:

- [ ] All projects build successfully
- [ ] No new compiler warnings introduced
- [ ] All automated tests pass
- [ ] Application starts and responds to requests
- [ ] MediatR handlers execute successfully
- [ ] Dapr integration works (service invocation, pub/sub)
- [ ] Azure Storage operations work
- [ ] Discord bot responds to messages
- [ ] OpenAI/Semantic Kernel integration works
- [ ] Docker containers build and run
- [ ] Azure Functions trigger and execute
- [ ] No errors in application logs
- [ ] Performance metrics are acceptable

### Escalation Path

If critical issues arise:

1. **Minor Issues** (warnings, non-critical errors)
   - Document and create follow-up tasks
   - Continue with migration

2. **Major Issues** (build failures, critical runtime errors)
   - Stop migration process
   - Investigate root cause
   - Apply fix or rollback specific project

3. **Blocking Issues** (incompatible APIs, data loss risk)
   - Halt migration immediately
   - Rollback entire branch
   - Reassess migration strategy
   - Consult .NET 8 migration documentation

## Testing & Validation Strategy

### Testing Pyramid Approach

```
                    ╱╲
                   ╱  ╲
                  ╱ E2E ╲           End-to-End Tests (Manual)
                 ╱──────╲         - Full Discord bot interaction
                ╱        ╲        - Complete request/response flows
               ╱          ╲       - Docker container deployment
              ╱────────────╲      
             ╱              ╲     Integration Tests
            ╱  Integration  ╲    - MediatR handler execution
           ╱────────────────╲   - Azure service connections
          ╱                  ╲  - Dapr sidecar communication
         ╱                    ╲
        ╱    Unit Tests        ╲  Unit Tests
       ╱──────────────────────╲ - Individual class/method validation
      ╱________________________╲ - Package compatibility verification
```

### Phase 1: Pre-Migration Testing

#### Baseline Behavior Documentation
**Purpose:** Establish known-good behavior to compare against post-migration

**Actions:**
1. Document current application startup behavior
2. Test and record response times for key endpoints
3. Capture sample logs for normal operation
4. Test Discord bot command responses
5. Verify OpenAI integration responses

**Tools:**
```bash
# Test current build
dotnet build MudakAI.sln -c Release

# Run current version
dotnet run --project MudakAI.Chat.WebService/MudakAI.Chat.WebService.csproj
```

### Phase 2: Component-Level Testing (During Migration)

#### For Each Updated Project:

**Build Verification:**
```bash
# Clean build test
dotnet clean <project>.csproj
dotnet build <project>.csproj --configuration Release

# Check for warnings
dotnet build <project>.csproj /warnaserror /nowarn:CS1591
```

**Package Validation:**
```bash
# List package dependencies
dotnet list <project>.csproj package

# Check for vulnerable packages
dotnet list <project>.csproj package --vulnerable

# Check for deprecated packages
dotnet list <project>.csproj package --deprecated
```

**Unit Tests (if available):**
```bash
# Run tests for specific project
dotnet test <test-project>.csproj --logger "console;verbosity=detailed"
```

### Phase 3: Integration Testing

#### MudakAI.Chat.WebService Integration Tests

**Test 1: Application Startup**
```bash
# Start application
dotnet run --project MudakAI.Chat.WebService/MudakAI.Chat.WebService.csproj

# Expected: No startup errors, application listening on configured port
# Check logs for DI registration success
```

**Test 2: MediatR Handler Execution**
- **Purpose:** Verify MediatR 12.0 registration works
- **Method:** Trigger each command/query handler
- **Test Cases:**
  - `AnnouncementCommand` - Send announcement via bot
  - `SetPersonaCommand` - Change bot persona
  - `SpeakCommand` - Trigger text-to-speech
  - `GetPersonaQuery` - Retrieve current persona
  - `DiscordMessageReceived` - Process incoming Discord message

**Test 3: Dapr Integration**
- Verify Dapr sidecar connection
- Test pub/sub message publishing
- Test service invocation to other services

**Test 4: Azure Storage Integration**
- Test Azure Table operations (read/write persona data)
- Verify connection string configuration
- Check for authentication issues

**Test 5: Discord Integration**
- Send test message to Discord bot
- Verify bot responds appropriately
- Check command processing pipeline

**Test 6: OpenAI/Semantic Kernel**
- Trigger AI response generation
- Verify API key and endpoint configuration
- Check response quality and latency

#### Azure Functions Integration Tests

**MudakAI.TextToSpeech.Functions:**
```bash
# Run locally with Azure Functions Core Tools
cd MudakAI.TextToSpeech.Functions
func start

# Test trigger (Service Bus, Storage, Timer)
# Verify Application Insights telemetry (if enabled)
```

**MudakAI.Voice.Functions:**
```bash
# Run locally
cd MudakAI.Voice.Functions
func start

# Test Discord connector function
# Test playback listener function
```

### Phase 4: System-Level Testing

#### Docker Container Testing

**Build Container:**
```bash
# Build Docker image
docker build -t mudakai-chat:net8 -f MudakAI.Chat.WebService/Dockerfile .

# Run container
docker run -p 8080:80 -e ASPNETCORE_ENVIRONMENT=Development mudakai-chat:net8

# Verify container starts and responds
curl http://localhost:8080/health
```

**Docker Compose Testing:**
```bash
# Test full stack with docker-compose
docker-compose up --build

# Verify all services start
# Test inter-service communication
```

#### Performance Testing

**Baseline Metrics to Compare:**
- Application startup time
- First request response time
- Average response time under load
- Memory consumption at idle
- Memory consumption under load
- CPU utilization patterns

**Simple Load Test:**
```bash
# Using Apache Bench (if available)
ab -n 1000 -c 10 http://localhost:8080/api/persona

# Or PowerShell:
1..100 | ForEach-Object { 
    Measure-Command { Invoke-WebRequest -Uri "http://localhost:8080/api/persona" }
}
```

### Phase 5: Regression Testing

#### Critical User Flows

**Flow 1: Bot Response to User Message**
1. User sends message in Discord
2. Discord webhook triggers DiscordMessageReceived notification
3. MediatR processes notification
4. OpenAI generates response via Semantic Kernel
5. Response sent back to Discord
6. User sees bot reply

**Flow 2: Voice/TTS Pipeline**
1. Text input received
2. Service Bus message published
3. TextToSpeech function triggered
4. Audio generated and stored in Blob storage
5. Playback function retrieves and plays audio

**Flow 3: Persona Management**
1. User requests persona change via SetPersonaCommand
2. MediatR handles command
3. Persona stored in Azure Table
4. GetPersonaQuery retrieves current persona
5. Bot behavior reflects new persona

### Phase 6: Post-Deployment Validation

#### Deployment Checklist

**Immediate Checks (First 5 Minutes):**
- [ ] Application started successfully
- [ ] No errors in startup logs
- [ ] Health check endpoint returns 200 OK
- [ ] Dapr sidecar connected
- [ ] Azure Storage accessible
- [ ] Discord bot online

**Short-term Monitoring (First Hour):**
- [ ] No unexpected exceptions in logs
- [ ] Memory usage stable
- [ ] CPU usage normal
- [ ] Response times acceptable
- [ ] Message processing working

**Medium-term Monitoring (First 24 Hours):**
- [ ] No memory leaks detected
- [ ] Error rates normal
- [ ] Performance metrics meet baseline
- [ ] User-reported issues = 0

### Testing Tools and Commands

#### Useful Commands

```bash
# Full solution build
dotnet build MudakAI.sln -c Release

# Run all tests
dotnet test MudakAI.sln --logger "console;verbosity=normal"

# Check package vulnerabilities across solution
dotnet list package --vulnerable --include-transitive

# Analyze solution for issues
dotnet format --verify-no-changes

# Check for deprecated packages
dotnet list package --deprecated

# List SDK versions
dotnet --list-sdks

# List runtime versions
dotnet --list-runtimes
```

#### Health Check Endpoints

Add or verify these endpoints in MudakAI.Chat.WebService:

```csharp
// Minimal health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", framework = ".NET 8.0" }));

// Detailed health check
app.MapHealthChecks("/health/detailed", new HealthCheckOptions {
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### Test Results Documentation

Create a test results log: `.github/upgrades/scenarios/new-dotnet-version_27c701/test-results.md`

**Template:**
```markdown
# .NET 8 Migration Test Results

## Build Tests
- [ ] All projects build successfully
- [ ] No new warnings introduced
- [ ] Package restore successful

## Unit Tests
- [ ] All unit tests pass
- [ ] No test failures
- [ ] Code coverage maintained

## Integration Tests
- [ ] MediatR handlers execute
- [ ] Dapr integration works
- [ ] Azure Storage accessible
- [ ] Discord bot responds
- [ ] OpenAI integration works

## Performance Tests
- Startup time: ___ ms (baseline: ___ ms)
- Memory usage: ___ MB (baseline: ___ MB)
- Average response time: ___ ms (baseline: ___ ms)

## Regression Tests
- [ ] Bot responds to messages
- [ ] Voice/TTS pipeline works
- [ ] Persona management works

## Issues Found
1. [Issue description] - Status: [Fixed/Pending/Deferred]
2. ...

## Sign-off
- Tested by: ___
- Date: ___
- Migration status: [Success/Partial/Failed]
```

## Complexity & Effort Assessment

### Overall Assessment

| Metric | Rating | Justification |
|--------|--------|---------------|
| **Technical Complexity** | ⭐⭐ Low-Medium (2/5) | Mostly package updates; one API change (MediatR) |
| **Risk Level** | 🟢 Low | Well-documented changes; stable .NET 8 release |
| **Estimated Effort** | 2-3 hours | Including testing and validation |
| **Team Skill Required** | Junior-Mid Level | Standard .NET upgrade tasks |

### Detailed Effort Breakdown

#### By Project

| Project | Complexity | Effort | Risk | Notes |
|---------|-----------|---------|------|-------|
| **MudakAI.Connectors.Azure.Table** | ⭐ Very Low | 10 min | 🟢 Low | Single package update |
| **MudakAI.Connectors.Azure.Blob** | ⭐ Very Low | 10 min | 🟢 Low | Single package update |
| **MudakAI.Connectors.OpenAI** | ⭐ Very Low | 10 min | 🟢 Low | Patch version update |
| **MudakAI.Connectors.Discord** | - None | 0 min | - None | Already .NET 8 |
| **MudakAI.Chat.WebService** | ⭐⭐ Low-Med | 45-60 min | 🟡 Medium | Framework change + MediatR API update |
| **MudakAI.TextToSpeech.Functions** | ⭐ Low | 20 min | 🟢 Low | Optional enhancements |
| **MudakAI.Voice.Functions** | ⭐ Low | 20 min | 🟢 Low | Optional enhancements |
| **Testing & Validation** | - | 45-60 min | - | Integration tests across all projects |

**Total Active Work Time:** 2-3 hours  
**Total Calendar Time (with testing):** 3-4 hours

#### By Task Type

| Task Type | Estimated Time | Percentage of Total Effort |
|-----------|----------------|---------------------------|
| **Project File Updates** | 30 min | 20% |
| **Package Updates** | 30 min | 20% |
| **Code Changes (MediatR)** | 15 min | 10% |
| **Build & Compile** | 15 min | 10% |
| **Integration Testing** | 45 min | 30% |
| **Documentation** | 15 min | 10% |

### Complexity Factors

#### Low Complexity Factors (Reducing Risk)
- ✅ Most projects already on .NET 8.0
- ✅ Small codebase (7 projects)
- ✅ Clear dependency hierarchy
- ✅ Well-documented breaking changes (MediatR)
- ✅ Mature .NET 8 release (LTS, production-ready)
- ✅ Compatible package versions available
- ✅ No database schema changes
- ✅ No API contract changes

#### Medium Complexity Factors (Requiring Attention)
- ⚠️ MediatR API change requires code update
- ⚠️ Docker container configuration changes
- ⚠️ Azure Functions V2 model (optional but recommended)
- ⚠️ Multiple Azure SDK packages to coordinate

#### Complexity Mitigation
- 📋 Detailed migration guide provided
- 📋 Step-by-step project-by-project plan
- 📋 Rollback strategy documented
- 📋 Testing checklist prepared

### Skills Required

| Skill Area | Level Required | Availability |
|------------|---------------|--------------|
| **.NET/C# Development** | Mid-level | ✅ Should have |
| **NuGet Package Management** | Basic | ✅ Common skill |
| **Git Version Control** | Basic | ✅ Common skill |
| **MediatR Framework** | Basic | ⚠️ May need quick reference |
| **Docker** | Basic | ⚠️ Optional (for container testing) |
| **Azure Functions** | Basic | ⚠️ Optional (for Functions updates) |

### Effort Optimization Strategies

#### Parallel Execution Opportunities

**Can Be Done in Parallel:**
1. Update all Level 0 libraries simultaneously (Azure.Table, Azure.Blob, OpenAI, Discord)
2. Test each library independently after update

**Must Be Sequential:**
1. Level 0 libraries → Level 1 applications (dependency order)
2. Build → Test → Deploy pipeline for each project

#### Time-Saving Tips

1. **Use CLI commands for package updates** (faster than manual .csproj editing)
   ```bash
   dotnet add package <name> --version <version>
   dotnet remove package <name>
   ```

2. **Batch build validation**
   ```bash
   dotnet build MudakAI.sln  # Build all at once
   ```

3. **Automated testing** (if test suite exists)
   ```bash
   dotnet test MudakAI.sln  # Run all tests
   ```

4. **Pre-download packages** (saves time during actual migration)
   ```bash
   dotnet restore MudakAI.sln
   ```

### Resource Requirements

#### Software Prerequisites
- ✅ .NET 8.0 SDK (required)
- ✅ Visual Studio 2022 17.8+ or VS Code (recommended)
- ⚠️ Docker Desktop (optional, for container testing)
- ⚠️ Azure Functions Core Tools (optional, for Functions testing)
- ⚠️ Git (required for version control)

#### Hardware Requirements
- **Minimal:** Standard development machine (4GB RAM, dual-core)
- **Recommended:** 8GB+ RAM, quad-core (for faster builds)

#### Network Requirements
- Internet connection for NuGet package downloads
- Azure access for testing Azure service connections
- Discord API access for bot testing

### Effort Estimation by Role

#### Developer
- **Initial Migration:** 1.5-2 hours
- **Testing & Fixes:** 0.5-1 hour
- **Total:** 2-3 hours

#### QA/Tester
- **Test Plan Execution:** 1 hour
- **Regression Testing:** 0.5 hour
- **Total:** 1.5 hours

#### DevOps (if separate)
- **CI/CD Pipeline Updates:** 0.5 hour
- **Deployment Verification:** 0.5 hour
- **Total:** 1 hour

**Total Team Effort:** 4.5-5.5 hours

### Confidence Level

| Aspect | Confidence | Reasoning |
|--------|-----------|-----------|
| **Estimation Accuracy** | 🟢 High (85%) | Well-understood changes; precedent from other migrations |
| **Success Probability** | 🟢 High (90%) | Low-risk changes; stable target framework |
| **On-Time Completion** | 🟢 High (90%) | Small scope; clear plan |
| **Zero-Defect Migration** | 🟡 Medium (70%) | MediatR change may need iteration |

### Contingency Buffer

**Recommended Buffer:** +50% (1-1.5 additional hours)

**Reasons for Buffer:**
- Unexpected dependency conflicts
- MediatR behavior differences requiring debugging
- Azure service connection issues during testing
- Docker configuration adjustments
- Team unfamiliarity with .NET 8 features

**Total Project Estimate with Buffer:** 3-4.5 hours

### Effort Reduction Opportunities

If timeline is critical, consider:

1. **Minimum Viable Migration:**
   - Only update MudakAI.Chat.WebService to .NET 8.0
   - Defer connector library package updates (optional)
   - Skip Azure Functions V2 model upgrade (optional)
   - **Reduced Effort:** 1-1.5 hours

2. **Automated Approach:**
   - Use `dotnet-upgrade-assistant` tool for automatic migration
   - Review and test generated changes
   - **Note:** May require more review time

3. **Staged Migration:**
   - Week 1: Update foundation libraries
   - Week 2: Update main application
   - Week 3: Update Azure Functions
   - **Benefit:** Reduced pressure, better testing coverage

## Source Control Strategy

### Branch Strategy

#### Branch Structure
```
master (main branch)
  └── upgrade-to-NET8 (feature branch - current) ✅ CREATED
       ├── foundation-libraries (optional sub-branch)
       ├── main-application (optional sub-branch)
       └── azure-functions (optional sub-branch)
```

**Current State:**
- ✅ Source branch: `master`
- ✅ Upgrade branch: `upgrade-to-NET8` (created)
- ✅ All pending changes committed

### Commit Strategy

#### Granular Commits (Recommended)

Each project update should be a separate commit for easy rollback:

**Commit Sequence:**
```
1. "Update Microsoft.Extensions.Azure in Connectors.Azure.Table to 1.13.1"
2. "Update Microsoft.Extensions.Azure in Connectors.Azure.Blob to 1.13.1"
3. "Update Microsoft.Extensions.DependencyInjection.Abstractions in Connectors.OpenAI to 8.0.2"
4. "Upgrade MudakAI.Chat.WebService to .NET 8.0"
5. "Remove MediatR.Extensions and update MediatR registration in Chat.WebService"
6. "Update Microsoft.VisualStudio.Azure.Containers.Tools.Targets in Chat.WebService"
7. "Enable Azure Functions V2 model for TextToSpeech.Functions"
8. "Enable Azure Functions V2 model for Voice.Functions"
9. "Add Application Insights to Functions projects (optional)"
```

**Commit Template:**
```
[Type] Brief description

- Detailed change 1
- Detailed change 2
- Detailed change 3

Refs: #issue-number (if applicable)
Testing: [Build passed / Tests passed / Manual verification]
```

**Commit Types:**
- `[Build]` - Build configuration changes (.csproj files)
- `[Package]` - NuGet package updates
- `[Code]` - Code changes (e.g., MediatR registration)
- `[Config]` - Configuration file changes (appsettings, host.json)
- `[Test]` - Test-related changes
- `[Docs]` - Documentation updates

#### Alternative: Squash Strategy

If you prefer a cleaner history:

1. Make all changes with frequent commits during development
2. Squash into logical commits before merging:
   - "Update foundation libraries to support .NET 8.0"
   - "Upgrade MudakAI.Chat.WebService to .NET 8.0"
   - "Enhance Azure Functions with V2 model and App Insights"

### Merge Strategy

#### Option 1: Pull Request (Recommended for Teams)

**Process:**
1. Complete all changes in `upgrade-to-NET8` branch
2. Create Pull Request: `upgrade-to-NET8` → `master`
3. Code review by team
4. Address feedback
5. Run CI/CD pipeline
6. Merge using "Squash and Merge" or "Merge Commit"

**PR Template:**
```markdown
## .NET 8.0 Upgrade

### Summary
Upgrades MudakAI.Chat.WebService and related projects to .NET 8.0 LTS

### Changes
- ✅ Updated target framework: net6.0 → net8.0
- ✅ Updated deprecated packages
- ✅ Updated MediatR to v12 and adjusted registration
- ✅ Enhanced Azure Functions with V2 model

### Testing
- [ ] All projects build successfully
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing complete
- [ ] Performance benchmarks acceptable

### Rollback Plan
Revert to `master` branch if issues arise

### Related Issues
Closes #[issue-number]
```

#### Option 2: Direct Merge (Solo Developer)

**Process:**
```bash
# After all changes complete and tested
git checkout master
git merge upgrade-to-NET8 --no-ff
git push origin master

# Keep upgrade branch for reference
git push origin upgrade-to-NET8
```

### Tagging Strategy

#### Pre-Migration Tag
```bash
# Tag the last known-good state before migration
git tag -a pre-net8-upgrade -m "Snapshot before .NET 8 migration"
git push origin pre-net8-upgrade
```

#### Post-Migration Tag
```bash
# Tag successful migration
git tag -a net8-migration-complete -m ".NET 8 migration completed successfully"
git push origin net8-migration-complete
```

#### Version Tag (if using semantic versioning)
```bash
# If this migration warrants a version bump
git tag -a v2.0.0 -m "Version 2.0.0 - .NET 8 upgrade"
git push origin v2.0.0
```

### Rollback Procedures

#### Scenario 1: Quick Rollback (Before Merge)

```bash
# Discard all changes in upgrade branch
git checkout master
git branch -D upgrade-to-NET8

# Or just switch back to working version
git checkout master
```

#### Scenario 2: Selective Rollback (Revert Specific Changes)

```bash
# Stay in upgrade branch, revert specific commit
git revert <commit-sha>

# Or reset to specific commit
git reset --hard <commit-sha>
```

#### Scenario 3: Post-Merge Rollback

```bash
# If already merged to master and deployed
git revert -m 1 <merge-commit-sha>
git push origin master

# Or hard reset (destructive - use with caution)
git reset --hard pre-net8-upgrade
git push origin master --force
```

### CI/CD Integration

#### Build Pipeline Updates

**If using GitHub Actions, update workflow:**

```yaml
# .github/workflows/build.yml
name: Build

on:
  push:
    branches: [ master, upgrade-to-NET8 ]
  pull_request:
    branches: [ master ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET 8
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'  # UPDATE THIS
    - name: Restore
      run: dotnet restore
    - name: Build
      run: dotnet build --configuration Release --no-restore
    - name: Test
      run: dotnet test --no-restore --verbosity normal
```

#### Azure Pipelines Updates

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
    - master
    - upgrade-to-NET8

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  dotnetVersion: '8.0.x'  # UPDATE THIS

steps:
- task: UseDotNet@2
  inputs:
    version: $(dotnetVersion)  # UPDATE THIS
- task: DotNetCoreCLI@2
  inputs:
    command: 'restore'
- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    arguments: '--configuration $(buildConfiguration)'
```

### Git Hooks (Optional Quality Gates)

#### Pre-Commit Hook
```bash
# .git/hooks/pre-commit
#!/bin/sh
# Build before committing
dotnet build MudakAI.sln
if [ $? -ne 0 ]; then
    echo "Build failed. Commit aborted."
    exit 1
fi
```

#### Pre-Push Hook
```bash
# .git/hooks/pre-push
#!/bin/sh
# Run tests before pushing
dotnet test MudakAI.sln
if [ $? -ne 0 ]; then
    echo "Tests failed. Push aborted."
    exit 1
fi
```

### Branch Protection Rules (GitHub)

**Recommended settings for `master` branch:**
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
  - ✅ Build check
  - ✅ Test check
- ✅ Require branches to be up to date before merging
- ⚠️ Do not allow force pushes
- ⚠️ Do not allow deletions

### File Change Tracking

**Expected File Changes Summary:**
```
Modified:
  MudakAI.Connectors.Azure.Table/MudakAI.Connectors.Azure.Table.csproj
  MudakAI.Connectors.Azure.Blob/MudakAI.Connectors.Azure.Blob.csproj
  MudakAI.Connectors.OpenAI/MudakAI.Connectors.OpenAI.csproj
  MudakAI.Chat.WebService/MudakAI.Chat.WebService.csproj
  MudakAI.Chat.WebService/Program.cs
  MudakAI.TextToSpeech.Functions/MudakAI.TextToSpeech.Functions.csproj
  MudakAI.TextToSpeech.Functions/host.json (optional)
  MudakAI.Voice.Functions/MudakAI.Voice.Functions.csproj
  MudakAI.Voice.Functions/host.json (optional)

Added (optional):
  .github/upgrades/scenarios/new-dotnet-version_27c701/test-results.md

Total Files Changed: 8-11
Total Lines Changed: ~50-100 (mostly XML/config)
```

### Documentation Updates

**Update the following documentation:**
- README.md - Update .NET version requirements
- CONTRIBUTING.md - Update build instructions
- deployment.md - Update deployment prerequisites
- docker-compose.yml - Update .NET version comments

**Example README.md update:**
```markdown
## Prerequisites

- .NET 8.0 SDK or later  <!-- UPDATED from .NET 6.0 -->
- Docker Desktop (for containerized development)
- Azure subscription (for Functions and Storage)
```

## Success Criteria

### Migration Completion Checklist

#### ✅ Build & Compilation Success
- [ ] All 7 projects build without errors
- [ ] No new compiler warnings introduced
- [ ] NuGet package restore successful for all projects
- [ ] No package version conflicts
- [ ] Docker container builds successfully

**Verification Command:**
```bash
dotnet build MudakAI.sln -c Release /warnaserror
```

#### ✅ Target Framework Updates
- [ ] MudakAI.Chat.WebService: `<TargetFramework>net8.0</TargetFramework>`
- [ ] MudakAI.Connectors.Azure.Table: remains `netstandard2.1`
- [ ] MudakAI.Connectors.Azure.Blob: remains `netstandard2.1`
- [ ] MudakAI.Connectors.OpenAI: remains `netstandard2.1`
- [ ] MudakAI.Connectors.Discord: confirmed `net8.0`
- [ ] MudakAI.TextToSpeech.Functions: confirmed `net8.0`
- [ ] MudakAI.Voice.Functions: confirmed `net8.0`

#### ✅ Package Updates Completed
- [ ] **MudakAI.Connectors.Azure.Table**: `Microsoft.Extensions.Azure` → 1.13.1
- [ ] **MudakAI.Connectors.Azure.Blob**: `Microsoft.Extensions.Azure` → 1.13.1
- [ ] **MudakAI.Connectors.OpenAI**: `Microsoft.Extensions.DependencyInjection.Abstractions` → 8.0.2
- [ ] **MudakAI.Chat.WebService**: `MediatR.Extensions.Microsoft.DependencyInjection` removed
- [ ] **MudakAI.Chat.WebService**: `MediatR` 12.0+ added (or verified present)
- [ ] **MudakAI.Chat.WebService**: `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` → 1.21.0+
- [ ] **Functions Projects** (optional): `Microsoft.Azure.Functions.Worker.ApplicationInsights` added

**Verification Command:**
```bash
dotnet list package | findstr -i "MediatR Extensions Azure Containers"
```

#### ✅ Code Changes Implemented
- [ ] MediatR registration updated in `MudakAI.Chat.WebService/Program.cs`
- [ ] Old registration syntax removed
- [ ] New registration syntax: `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly))`
- [ ] No compilation errors from MediatR changes

**Verification:** Build succeeds and application starts

#### ✅ Functional Validation
- [ ] Application starts without errors
- [ ] No exceptions during startup
- [ ] Health check endpoint responds (if configured)
- [ ] Dapr sidecar connects successfully
- [ ] All services registered in DI container

**Verification Command:**
```bash
dotnet run --project MudakAI.Chat.WebService/MudakAI.Chat.WebService.csproj
# Check logs for "Application started" and no errors
```

#### ✅ Integration Points Working

**MediatR Integration:**
- [ ] Commands execute successfully (AnnouncementCommand, SetPersonaCommand, SpeakCommand)
- [ ] Queries execute successfully (GetPersonaQuery)
- [ ] Notifications process successfully (DiscordMessageReceived)
- [ ] No runtime errors from MediatR pipeline

**Azure Services Integration:**
- [ ] Azure Table Storage: Read/write operations work
- [ ] Azure Blob Storage: Upload/download operations work
- [ ] Service Bus: Message publishing/receiving works (if used)
- [ ] No authentication errors

**Discord Integration:**
- [ ] Discord bot connects to Discord API
- [ ] Bot responds to messages
- [ ] Commands processed correctly
- [ ] No Discord SDK errors

**OpenAI/Semantic Kernel Integration:**
- [ ] AI responses generated successfully
- [ ] API calls to OpenAI work
- [ ] No Semantic Kernel errors
- [ ] Response quality unchanged

**Dapr Integration:**
- [ ] Pub/sub functionality works
- [ ] Service invocation works
- [ ] State management works (if used)

#### ✅ Performance Metrics

**Acceptable Performance Ranges (compared to .NET 6 baseline):**
- [ ] Startup time: ≤ baseline + 10%
- [ ] Memory usage (idle): ≤ baseline + 15%
- [ ] Memory usage (load): ≤ baseline + 15%
- [ ] Response time (average): ≤ baseline OR improved
- [ ] Throughput: ≥ baseline OR improved

**Expected .NET 8 Improvements:**
- ✨ Faster JSON serialization (System.Text.Json improvements)
- ✨ Better garbage collection efficiency
- ✨ Improved LINQ performance
- ✨ Lower memory allocations

#### ✅ Testing Completed
- [ ] Unit tests pass (if test suite exists)
- [ ] Integration tests pass
- [ ] Smoke tests completed
- [ ] Regression tests completed
- [ ] Docker container tests passed
- [ ] Azure Functions tests passed (if applicable)

**Verification Command:**
```bash
dotnet test MudakAI.sln --logger "console;verbosity=normal"
```

#### ✅ Docker Validation
- [ ] Dockerfile updated to use .NET 8 SDK: `mcr.microsoft.com/dotnet/sdk:8.0`
- [ ] Runtime image updated: `mcr.microsoft.com/dotnet/aspnet:8.0`
- [ ] Docker build succeeds
- [ ] Container starts successfully
- [ ] Application accessible in container
- [ ] No container runtime errors

**Verification Command:**
```bash
docker build -t mudakai-chat:net8 -f MudakAI.Chat.WebService/Dockerfile .
docker run -d -p 8080:80 mudakai-chat:net8
curl http://localhost:8080/health
```

#### ✅ Azure Functions Validation (Optional)
- [ ] V2 model configuration added
- [ ] Application Insights configuration added (optional)
- [ ] Functions trigger successfully
- [ ] Bindings work correctly
- [ ] No runtime errors

**Verification Command:**
```bash
cd MudakAI.TextToSpeech.Functions
func start  # Should start without errors
```

#### ✅ Source Control
- [ ] All changes committed to `upgrade-to-NET8` branch
- [ ] Commits are properly documented
- [ ] No merge conflicts with `master`
- [ ] Tag created: `pre-net8-upgrade` (on master before migration)
- [ ] Tag created: `net8-migration-complete` (after successful migration)

**Verification Command:**
```bash
git log --oneline upgrade-to-NET8
git tag
```

#### ✅ Documentation Updated
- [ ] README.md - .NET 8 requirements documented
- [ ] Build instructions updated
- [ ] Deployment guide updated (if exists)
- [ ] Dependencies documented
- [ ] Migration notes created

#### ✅ No Regressions
- [ ] No new runtime exceptions
- [ ] No new error logs
- [ ] No new warnings in logs
- [ ] No user-reported issues
- [ ] No performance degradation
- [ ] All existing features work

### Success Metrics

#### Quantitative Metrics

| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| **Build Success Rate** | 100% | ___ | ⬜ |
| **Test Pass Rate** | 100% | ___ | ⬜ |
| **Zero Critical Errors** | 0 | ___ | ⬜ |
| **Deployment Success** | 1st try | ___ | ⬜ |
| **Rollback Instances** | 0 | ___ | ⬜ |
| **Performance vs Baseline** | ≥100% | ___% | ⬜ |
| **Memory vs Baseline** | ≤115% | ___% | ⬜ |
| **Response Time vs Baseline** | ≤100% | ___% | ⬜ |

#### Qualitative Success Indicators

- ✅ **Team Confidence**: Team comfortable with .NET 8 environment
- ✅ **Code Quality**: No technical debt introduced
- ✅ **Maintainability**: Code remains easy to understand and modify
- ✅ **User Experience**: No negative user feedback
- ✅ **Operations**: Deployment and monitoring unchanged or improved

### Definition of Done

The migration is considered **COMPLETE** when:

1. ✅ **All mandatory criteria** above are met
2. ✅ **Application deployed** to at least one environment (dev/staging)
3. ✅ **Monitoring confirms** stability for 24-48 hours
4. ✅ **Team sign-off** from:
   - Developer(s)
   - QA/Tester (if separate role)
   - DevOps (if separate role)
   - Product Owner (if applicable)

### Final Sign-Off

```
Migration Completed: _____ (Date)
Tested By: ___________
Approved By: ___________
Deployed To Production: _____ (Date)

Notes:
_______________________________________
_______________________________________
_______________________________________

Status: [ ] Success  [ ] Success with Minor Issues  [ ] Requires Follow-up
```

### Post-Migration Monitoring (30 Days)

**Ongoing Validation:**
- Week 1: Daily log review, performance monitoring
- Week 2: Performance comparison vs .NET 6 baseline
- Week 3: Stability validation, error rate analysis
- Week 4: Final assessment, lessons learned documentation

**Success Indicators:**
- ✅ Zero critical incidents related to .NET 8 upgrade
- ✅ Performance metrics stable or improved
- ✅ No rollback required
- ✅ No emergency hotfixes needed

---

## Conclusion

This comprehensive plan provides:
- ✅ **Clear roadmap** for upgrading MudakAI.Chat.WebService to .NET 8.0
- ✅ **Detailed steps** for each affected project
- ✅ **Risk mitigation** strategies
- ✅ **Testing & validation** procedures
- ✅ **Success criteria** for completion
- ✅ **Rollback procedures** if needed

**Recommended Next Steps:**
1. Review and approve this plan
2. Schedule migration window (estimated 3-4 hours)
3. Ensure .NET 8 SDK is installed
4. Begin execution following the project order specified
5. Document any issues or deviations from plan

**Questions or Concerns?**
Address before starting execution to ensure smooth migration.

---

*Plan created: 2024*  
*Target .NET Version: 8.0 LTS*  
*Estimated Effort: 2-3 hours (active work), 3-4 hours (total with testing)*  
*Risk Level: Low 🟢*
