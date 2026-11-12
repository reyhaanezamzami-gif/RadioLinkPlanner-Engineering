# Architecture — RadioLinkPlanner-Engineering



## 1. Overview



RadioLinkPlanner-Engineering is a selected public release of engineering components from the RadioLinkPlannervfV project.



The repository focuses on three primary areas:



- Domain models and shared contracts

- Radio-frequency calculations

- Terrain processing and analysis



The published code is organized into separate .NET 8 projects with explicit project references.



This repository does not represent the complete architecture of the original application.



## 2. Technology



| Technology | Purpose |

|---|---|

| C# | Primary programming language |

| .NET 8 | Application framework |

| xUnit | Automated testing |

| Nullable Reference Types | Compile-time nullability analysis |

| Implicit Usings | Common namespace imports |



All published projects target `net8.0`.



## 3. Solution Structure



```text

RadioLinkPlanner-Engineering/

│

├── README.md

├── ARCHITECTURE.md

├── .gitignore

│

├── src/

│   ├── RadioLinkPlannervfV.Core/

│   │   ├── Enums/

│   │   ├── Interfaces/

│   │   ├── Models/

│   │   ├── Services/

│   │   └── ValueObjects/

│   │

│   ├── RadioLinkPlannervfV.RF/

│   │   └── Calculators/

│   │

│   └── RadioLinkPlannervfV.Terrain/

│       ├── Calculators/

│       ├── Exceptions/

│       ├── Generators/

│       ├── Models/

│       ├── Optimizers/

│       ├── Providers/

│       ├── Readers/

│       └── Services/

│

└── tests/

&#x20;   └── RadioLinkPlannervfV.Terrain.Tests/

```



## 4. Project Responsibilities



### 4.1 RadioLinkPlannervfV.Core



Core contains the shared domain models and abstractions used by the published engineering projects.



Its source structure includes:



| Directory | Responsibility |

|---|---|

| `Enums` | Domain enumeration types |

| `Interfaces` | Contracts for calculations, terrain access, analysis, and persistence |

| `Models` | Domain entities, requests, results, and configuration models |

| `Services` | Shared domain-level services |

| `ValueObjects` | Strongly typed domain values such as distance and frequency |



Examples of published domain concepts include:



- Geographic coordinates

- Sites and radio links

- Radio parameters

- Propagation settings

- Terrain elevation

- Terrain profiles

- Link analysis results

- Link budget models

- Multi-site analysis

- Network analysis

- Project and report contracts



\*\*Dependency rule:\*\*



Core has no project references in its published project file.



This keeps the published domain layer independent of the RF and Terrain implementation projects.



### 4.2 RadioLinkPlannervfV.RF



The RF project contains radio-frequency calculation implementations.



Published source components include:



- `FresnelCalculator`

- `LinkBudgetCalculator`



Its responsibility is to provide RF calculation functionality using the shared contracts and models defined in Core.



\*\*Project dependency:\*\*



```text

RadioLinkPlannervfV.RF

&#x20;         |

&#x20;         v

RadioLinkPlannervfV.Core

```



### 4.3 RadioLinkPlannervfV.Terrain



The Terrain project contains terrain-related data processing and analysis components.



Its published source is organized into:



| Directory | Responsibility |

|---|---|

| `Calculators` | Geographic distance and interpolation calculations |

| `Exceptions` | Terrain-related exception types |

| `Generators` | Terrain profile generation |

| `Models` | Terrain configuration and provider options |

| `Optimizers` | Antenna height optimization |

| `Providers` | Offline, online, and fallback elevation providers |

| `Readers` | SRTM file and elevation data reading |

| `Services` | Terrain-related engineering analysis services |



Published implementations include:



- Great-circle interpolation

- Haversine distance calculation

- Terrain profile generation

- SRTM coordinate mapping

- SRTM DEM reading

- SRTM tile metadata handling

- Offline terrain elevation provider

- Open-Meteo terrain elevation provider

- Fallback terrain elevation provider

- Earth curvature calculation

- Link analysis

- Multi-site evaluation

- Antenna height optimization



\*\*Project dependency:\*\*



```text

RadioLinkPlannervfV.Terrain

&#x20;            |

&#x20;            v

RadioLinkPlannervfV.Core

```



### 4.4 RadioLinkPlannervfV.Terrain.Tests



The published test project validates selected behavior across the Core, RF, and Terrain projects.



Its project references include:



- RadioLinkPlannervfV.Core

- RadioLinkPlannervfV.RF

- RadioLinkPlannervfV.Terrain



The test project uses xUnit.



Its published tests cover calculation behavior, terrain providers, SRTM data processing, profile generation, multi-site evaluation, and integration or diagnostic scenarios.



## 5. Dependency Architecture



The verified project-reference structure is:



```text

&#x20;                   Core

&#x20;                  /    \\

&#x20;                 /      \\

&#x20;                RF     Terrain

&#x20;                 \\       /

&#x20;                  \\     /

&#x20;                Terrain.Tests

```



The dependency rules are:



1\. Core does not reference other published projects.

2\. RF references Core.

3\. Terrain references Core.

4\. Terrain.Tests references Core, RF, and Terrain.

5\. Core does not depend on the implementation details of RF or Terrain.



No direct RF-to-Terrain project reference is declared in the published project files.



## 6. Architectural Boundaries



The published code separates shared domain contracts from engineering implementations.



### Core boundary



Core defines shared models, interfaces, and value objects.



It does not contain references to the published RF or Terrain projects.



### Engineering implementation boundary



RF and Terrain contain specialized engineering implementations.



Both projects reference Core to use shared domain abstractions.



### Testing boundary



Terrain.Tests references the published engineering projects to verify selected behavior.



This structure provides separation between domain definitions, engineering implementations, and automated tests.



## 7. Terrain Data Architecture



The published Terrain project includes multiple elevation data access implementations.



These include:



- Offline terrain elevation access

- Open-Meteo elevation access

- Fallback terrain elevation access

- SRTM DEM reading components



The repository also includes abstractions for terrain elevation access and batch terrain elevation operations in Core.



The presence of these components does not imply that all required DEM datasets are bundled with the repository.



Actual terrain data availability depends on the runtime environment and configured data sources.



## 8. Build and Test Boundaries



The public repository does not include the complete original solution.



Build and test operations should therefore target the published test project.



Restore dependencies:



```powershell

dotnet restore tests/RadioLinkPlannervfV.Terrain.Tests/RadioLinkPlannervfV.Terrain.Tests.csproj

```



Build:



```powershell

dotnet build tests/RadioLinkPlannervfV.Terrain.Tests/RadioLinkPlannervfV.Terrain.Tests.csproj --no-restore

```



Run tests:



```powershell

dotnet test tests/RadioLinkPlannervfV.Terrain.Tests/RadioLinkPlannervfV.Terrain.Tests.csproj --no-restore

```



A successful restore is required before using `--no-restore`.



## 9. Public Repository Limitations



The following components belong to the broader application but are not included in this public release:



- API

- Web application

- Infrastructure implementations

- Application guide

- Complete application solution

- Private company and customer datasets

- Full deployment configuration



Consequently, the dependency diagram in this document describes only the published projects.



It should not be interpreted as a complete representation of the original application's architecture.



## 10. Design Considerations



The published project structure demonstrates several architectural principles:



- Separation of shared domain abstractions from specialized implementations

- Explicit project dependencies

- Independent RF and Terrain implementation projects

- Centralized domain contracts

- Automated testing through a dedicated test project

- Use of .NET 8 across the published projects



These boundaries provide a foundation for maintaining and extending the published engineering components without introducing unnecessary dependencies into Core.



---



\*\*Repository:\*\* RadioLinkPlanner-Engineering  

\*\*Framework:\*\* .NET 8  

\*\*Architecture Scope:\*\* Published Engineering Components Only

