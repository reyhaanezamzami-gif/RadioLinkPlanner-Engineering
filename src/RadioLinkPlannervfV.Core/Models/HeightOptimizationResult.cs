namespace RadioLinkPlannervfV.Core.Models;

public sealed record HeightOptimizationResult
(
    double OptimizedAntennaHeightASiteAMeters,
    double OptimizedAntennaHeightBSiteBMeters,
    double TotalAdditionalHeightMeters,
    bool IsClear
);