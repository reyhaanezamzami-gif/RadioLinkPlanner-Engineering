# RadioLink Planner — Engineering Components

A C#/.NET 8 engineering project focused on radio link planning, RF calculations, and terrain analysis.

This repository presents a selected public portion of a larger radio link planning application. It is intended to demonstrate selected engineering models, calculation components, terrain-processing functionality, and automated tests.

> **Scope notice:** This repository is not the complete application or the full original source code.

## Engineering Scope

The published source is organized into the following components:

| Component | Responsibility |
|---|---|
| Core | Domain models, interfaces, value objects, and shared abstractions |
| RF | Radio-frequency calculation components |
| Terrain | Terrain processing, elevation providers, and terrain-based analysis |
| Tests | Automated tests for selected RF and terrain components |

### Core

Contains shared domain models, interfaces, and value objects used by the selected engineering components.

### RF

Contains selected radio-frequency calculation implementations, including:

- Fresnel zone calculations
- Link budget calculations

### Terrain

Contains selected terrain-processing and analysis implementations, including:

- Terrain profile generation
- SRTM DEM reading
- Terrain elevation providers
- Earth curvature calculations
- Terrain-based link analysis
- Antenna height optimization

### Tests

Contains selected automated tests covering terrain processing, RF-related calculations, and integration or diagnostic scenarios.

Some integration and diagnostic tests may require external elevation datasets, local DEM files, or network connectivity.

## Technology Stack

- C#
- .NET 8
- xUnit

## Repository Structure

```text
src/
├── RadioLinkPlannervfV.Core/
├── RadioLinkPlannervfV.RF/
└── RadioLinkPlannervfV.Terrain/

tests/
└── RadioLinkPlannervfV.Terrain.Tests/
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Network connectivity for restoring NuGet dependencies

### Restore Dependencies

```powershell
dotnet restore tests/RadioLinkPlannervfV.Terrain.Tests/RadioLinkPlannervfV.Terrain.Tests.csproj
```

### Build

```powershell
dotnet build tests/RadioLinkPlannervfV.Terrain.Tests/RadioLinkPlannervfV.Terrain.Tests.csproj --no-restore
```

### Run Tests

```powershell
dotnet test tests/RadioLinkPlannervfV.Terrain.Tests/RadioLinkPlannervfV.Terrain.Tests.csproj --no-restore
```

**Note:** A successful dependency restore is required before using `--no-restore`. Tests requiring external terrain data or network services may need additional environment setup.

## Scope and Limitations

This repository contains a limited selection of engineering components for public technical presentation.

It does not include:

- The complete application
- The full original source code
- Private company materials
- Proprietary terrain datasets
- All internal implementation components

The presence of selected source files does not imply that every application workflow can be built or executed independently from this repository.

## Copyright and Usage

Copyright and all applicable rights are reserved by the respective rights holder.

No license is granted to copy, modify, distribute, sublicense, or use this source code for commercial or other purposes without prior written permission from the rights holder.

Public visibility does not constitute permission to reuse the code.

For permission requests, contact the repository owner through GitHub.

---

**Repository:** RadioLinkPlanner-Engineering
**Framework:** .NET 8
**Language:** C#
