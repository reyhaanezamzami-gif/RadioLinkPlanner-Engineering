# Radio Link Planner — Engineering Components

A selected public release of the engineering components from **RadioLinkPlannervfV**, a radio link planning application developed using C# and .NET 8.

This repository contains selected domain models, RF calculation components, terrain-processing components, and automated tests. It does not contain the complete application or its full source code.

## Repository Scope

The public repository contains the following projects:

| Project | Description |
|---|---|
| `RadioLinkPlannervfV.Core` | Domain models, interfaces, enums, and value objects |
| `RadioLinkPlannervfV.RF` | Radio-frequency calculation components |
| `RadioLinkPlannervfV.Terrain` | Terrain data access, elevation profiles, terrain analysis, and antenna height optimization |
| `RadioLinkPlannervfV.Terrain.Tests` | Automated tests for terrain and related engineering components |

### Published Source File Count

The published source contains **69 C# files**:

| Project | C# Files |
|---|---:|
| `RadioLinkPlannervfV.Core` | 49 |
| `RadioLinkPlannervfV.RF` | 2 |
| `RadioLinkPlannervfV.Terrain` | 18 |
| **Total** | **69** |

The complete application includes additional components that are not part of this public repository, including its API, web interface, and infrastructure layer.

## Technology Stack

- C#
- .NET 8
- Nullable reference types
- Implicit global usings
- xUnit for automated testing

All published projects target `net8.0`.

## Engineering Components

### 1. Core

The Core project defines the domain contracts and data structures used by the engineering components.

Its published source includes:

- Geographic coordinates and distance value objects
- Site and link models
- Radio parameters and propagation settings
- Link analysis and obstruction results
- Terrain elevation and profile models
- Interfaces for terrain data access and profile generation
- Interfaces for RF calculations and link analysis
- Multi-site and network analysis models
- Project and report repository contracts

Core has no project references in its published project file.

### 2. RF

The RF project contains calculation components for radio-link analysis.

Published calculators:

- `FresnelCalculator`
- `LinkBudgetCalculator`

The project references Core and targets .NET 8.

### 3. Terrain

The Terrain project contains components for terrain data handling and terrain-related analysis.

**Terrain data:**

- SRTM coordinate mapping
- SRTM tile filename resolution
- SRTM tile metadata handling
- SRTM DEM reading
- Elevation sample reading

**Elevation providers:**

- Offline terrain provider
- Open-Meteo terrain elevation provider
- Fallback terrain elevation provider

**Terrain analysis:**

- Great-circle interpolation
- Haversine distance calculation
- Terrain profile generation
- Earth curvature calculation
- Link analysis
- Multi-site evaluation
- Antenna height optimization

The project references Core and targets .NET 8.

### 4. Terrain Tests

The repository includes an xUnit test project covering several areas:

- Distance and propagation-related calculations
- Fresnel and link budget calculations
- Terrain profile generation
- SRTM coordinate mapping and DEM reading
- Offline terrain provider behavior
- Open-Meteo provider behavior
- Fallback provider behavior
- Multi-site evaluation
- Antenna height optimization
- Terrain integration and diagnostic scenarios

The presence of a test does not establish that it currently passes. Test execution results should be verified in the target environment.

## Repository Structure

```text
RadioLinkPlanner-Engineering/
│
├── README.md
├── .gitignore
│
├── src/
│   ├── RadioLinkPlannervfV.Core/
│   ├── RadioLinkPlannervfV.RF/
│   └── RadioLinkPlannervfV.Terrain/
│
└── tests/
    └── RadioLinkPlannervfV.Terrain.Tests/
```

## Architecture and Dependencies

The published project references establish the following dependency structure:

```text
                 Core
                /    \
               /      \
              RF     Terrain
               \       /
                \     /
              Terrain.Tests
```

Project dependencies:

- Core has no project references.
- RF references Core.
- Terrain references Core.
- Terrain.Tests references Core, RF, and Terrain.

This repository represents selected engineering components, not the complete application architecture.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Internet access for restoring NuGet dependencies, unless the required packages are already available locally

### Restore Dependencies

Run from the repository root:

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

A successful restore is required before running build or test commands with `--no-restore`.

Some integration and diagnostic tests may require external elevation datasets, local DEM files, or network connectivity, depending on their configuration.

## Scope and Limitations

This public repository contains only the selected Core, RF, Terrain, and Terrain.Tests projects.

It does not include:

- The complete application solution
- The HTTP API
- The Blazor web application
- Infrastructure and persistence implementations
- The application guide
- The complete deployment configuration
- Private company or customer datasets

Consequently, cloning this repository does not provide a complete, runnable deployment of the original RadioLinkPlannervfV application.

Terrain data availability and provider configuration depend on the runtime environment and the data sources configured by the consuming application.

## Copyright and Usage

All rights are reserved by the respective rights holder.

This repository is published for public inspection and engineering reference.

No license is granted to copy, modify, redistribute, sublicense, or use this source code in other projects, whether commercially or otherwise, without prior written permission from the relevant rights holder.

The absence of a license file does not grant permission to reuse the code.

Before redistributing or reusing any portion of this repository, obtain the necessary authorization from the rights holder.

---

**Repository:** RadioLinkPlanner-Engineering
**Framework:** .NET 8
**Language:** C#
