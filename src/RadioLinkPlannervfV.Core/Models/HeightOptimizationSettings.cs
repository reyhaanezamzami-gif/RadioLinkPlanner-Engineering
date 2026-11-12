namespace RadioLinkPlannervfV.Core.Models;

public sealed record HeightOptimizationSettings
(
    double MinimumAntennaHeightMeters = 5.0,
    double MaximumAntennaHeightMeters = 200.0,
    double OptimizationStepMeters = 1.0
);