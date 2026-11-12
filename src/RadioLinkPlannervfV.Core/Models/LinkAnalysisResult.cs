using RadioLinkPlannervfV.Core.Enums;

namespace RadioLinkPlannervfV.Core.Models;

public sealed record LinkAnalysisResult
(
    double DistanceMeters,
    double FreeSpacePathLossDb,
    double FirstFresnelZoneRadiusMeters,
    double RequiredClearanceMeters,
    double MinimumAntennaHeightASLMeters,
    double RecommendedAntennaHeightASiteAMeters,
    double RecommendedAntennaHeightBSiteBMeters,
    double MaximumTerrainElevationMeters,
    double MaximumTerrainClearanceDeficitMeters,
    LinkStatus Status,
    HeightOptimizationResult? HeightOptimization = null,
    LinkObstruction? WorstTerrainObstruction = null,
    double SiteAGroundElevationMeters = 0,
    double SiteBGroundElevationMeters = 0,
    IReadOnlyList<TerrainProfileAnalysisPoint>? TerrainProfileAnalysis = null
);