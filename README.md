# RadioLink Planner

A radio link planning engineering project focused on terrain analysis and RF calculation components.

## Project Overview

RadioLink Planner is designed to support engineering workflows for radio link planning.

This repository contains a selected public portion of the project, including core models, RF calculation components, terrain processing, and selected automated tests.

**This is not the complete application or the full original source code.**

## Included Components

### Core
Contains domain models, interfaces, and shared business abstractions.

### RF
Contains selected radio-frequency calculation components, including:
- Fresnel zone calculations
- Link budget calculations

### Terrain
Contains selected terrain processing and analysis components, including:
- Terrain profile generation
- Elevation data providers
- SRTM DEM reading
- Earth curvature calculations
- Terrain-based link analysis
- Antenna height optimization

### Tests
Contains selected automated tests for terrain processing and RF-related calculations.

## Technology Stack

- C#
- .NET 8
- xUnit

## Project Structure

```text
src/
├── RadioLinkPlannervfV.Core/
├── RadioLinkPlannervfV.RF/
└── RadioLinkPlannervfV.Terrain/

tests/
└── RadioLinkPlannervfV.Terrain.Tests/
```

## Build

Build the test project and its referenced projects:

```powershell
dotnet build tests/RadioLinkPlannervfV.Terrain.Tests/RadioLinkPlannervfV.Terrain.Tests.csproj
```

## Tests

Run the available automated tests:

```powershell
dotnet test tests/RadioLinkPlannervfV.Terrain.Tests/RadioLinkPlannervfV.Terrain.Tests.csproj
```

Some terrain integration and diagnostic scenarios may require external elevation data or network access.

## Scope and Limitations

This repository is a limited public presentation of the project.

It does not include the complete application, private company materials, proprietary datasets, or all internal implementation components.

The published source code is provided for inspection and portfolio presentation only.

## Copyright and Usage

Copyright and all applicable rights are reserved by the respective rights holder.

No license is granted to copy, modify, distribute, sublicense, or use this source code for commercial or other purposes without prior written permission from the rights holder.

Public visibility does not constitute permission to reuse the code.

For permission requests, contact the repository owner through GitHub.
