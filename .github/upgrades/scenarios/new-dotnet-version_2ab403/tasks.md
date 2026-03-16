# Pattern-Sc2Bot .NET 8 Upgrade Tasks

## Overview

This document tracks the upgrade of the `Bot.sln` solution to target `net8.0`. The work will convert the project file to SDK-style, apply package updates, restore and build the solution, and run automated tests.

**Progress**: 0/3 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

---

## Tasks

### [ ] TASK-001: Verify prerequisites
**References**: Plan §1 Executive Summary, Plan §4 Project-by-Project Plans §Prerequisites

- [ ] (1) Verify required .NET 8 SDK is installed on the machine per Plan §Prerequisites (run `dotnet --list-sdks`)  
- [ ] (2) Installed runtime/SDK version meets minimum requirements (**Verify**)  
- [ ] (3) Check for `global.json` or other lock/config files and verify compatibility with target SDK per Plan §Prerequisites  
- [ ] (4) Configuration files compatible with target SDK (**Verify**)

### [ ] TASK-002: Atomic framework and package upgrade
**References**: Plan §4 Project-by-Project Plans, Plan §Package Update Matrix, Plan §6 Breaking Changes Catalog, Plan §10 Source Control Strategy

- [ ] (1) Convert `Bot\Bot.csproj` to SDK-style (`<Project Sdk="Microsoft.NET.Sdk">`) and set `<TargetFramework>net8.0</TargetFramework>`; migrate `PackageReference` entries or migrate from `packages.config` if required (see Plan §Project-by-Project Plans)  
- [ ] (2) Update NuGet package references per Plan §Package Update Matrix (e.g., `Newtonsoft.Json` → 13.0.4; leave `RoyT.AStar` as-is unless CI indicates otherwise)  
- [ ] (3) Restore dependencies (e.g., `dotnet restore`) and ensure all packages restore successfully (**Verify**)  
- [ ] (4) Build the solution and fix all compilation errors introduced by the framework/package changes (reference Plan §6 Breaking Changes Catalog for common remediation patterns)  
- [ ] (5) Solution builds with 0 errors (**Verify**)  
- [ ] (6) Commit changes with message: "TASK-002: Atomic framework and package upgrade to net8.0"

### [ ] TASK-003: Run full test suite and validate upgrade
**References**: Plan §7 Testing Strategy, Plan §6 Breaking Changes Catalog, Plan §4 Project-by-Project Plans

- [ ] (1) Discover and run all unit/integration test projects (`dotnet test`) per Plan §Testing Strategy  
- [ ] (2) Fix any test failures caused by the upgrade (reference Plan §6 Breaking Changes Catalog for typical fixes)  
- [ ] (3) Re-run tests after fixes  
- [ ] (4) All tests pass with 0 failures (**Verify**)  
- [ ] (5) Commit test fixes with message: "TASK-003: Complete testing and validation"