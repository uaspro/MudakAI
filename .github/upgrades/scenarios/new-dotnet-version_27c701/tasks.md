# MudakAI.Chat.WebService .NET 8.0 Upgrade Tasks

## Overview

This document tracks the execution of the MudakAI.Chat.WebService upgrade from .NET 6.0 to .NET 8.0. All affected components will be upgraded simultaneously in a single atomic operation, followed by testing and validation.

**Progress**: 0/3 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

---

## Tasks

### [▶] TASK-001: Verify prerequisites
**References**: Plan §Migration Strategy - Phase 0

- [ ] (1) Verify .NET 8.0 SDK installed per Plan §Prerequisites
- [ ] (2) .NET 8.0 SDK version confirmed (**Verify**)

---

### [ ] TASK-002: Atomic framework and package upgrade
**References**: Plan §Implementation Timeline - Phase 1 & Phase 2, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [ ] (1) Update Microsoft.Extensions.Azure to 1.13.1 in MudakAI.Connectors.Azure.Table and MudakAI.Connectors.Azure.Blob
- [ ] (2) Update Microsoft.Extensions.DependencyInjection.Abstractions to 8.0.2 in MudakAI.Connectors.OpenAI
- [ ] (3) Update target framework to net8.0 in MudakAI.Chat.WebService
- [ ] (4) Remove MediatR.Extensions.Microsoft.DependencyInjection package from MudakAI.Chat.WebService
- [ ] (5) Add MediatR package version 12.0.0 to MudakAI.Chat.WebService (if not already present)
- [ ] (6) Update Microsoft.VisualStudio.Azure.Containers.Tools.Targets to 1.21.0 in MudakAI.Chat.WebService
- [ ] (7) Update MediatR registration in MudakAI.Chat.WebService/Program.cs per Plan §Breaking Changes Catalog (change to cfg.RegisterServicesFromAssembly pattern)
- [ ] (8) Enable Azure Functions V2 model for MudakAI.TextToSpeech.Functions and MudakAI.Voice.Functions per Plan §Project-by-Project Plans sections 6 & 7
- [ ] (9) Add Application Insights configuration to Functions projects per Plan §Package Update Reference section 5 (optional)
- [ ] (10) Restore all dependencies
- [ ] (11) All dependencies restored successfully (**Verify**)
- [ ] (12) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog
- [ ] (13) Solution builds with 0 errors (**Verify**)
- [ ] (14) Commit changes with message: "TASK-002: Upgrade to .NET 8.0 - framework, packages, and configuration updates"

---

### [ ] TASK-003: Run test suite and validate upgrade
**References**: Plan §Testing & Validation Strategy, Plan §Success Criteria

- [ ] (1) Run all test projects per Plan §Testing & Validation Strategy (if test projects exist)
- [ ] (2) Fix any test failures referencing Plan §Breaking Changes Catalog for common issues
- [ ] (3) Re-run tests after fixes
- [ ] (4) All tests pass with 0 failures (**Verify**)
- [ ] (5) Commit test fixes with message: "TASK-003: Complete .NET 8.0 upgrade testing and validation"

---
