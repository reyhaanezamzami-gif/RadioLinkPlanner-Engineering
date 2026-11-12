namespace RadioLinkPlannervfV.Core.Models;

public sealed record TerrainProfileAnalysisPoint
(
    GeoCoordinate Coordinate,
    double DistanceFromStartMeters,
    double TerrainElevationMeters,
    double TerrainElevationWithCurvatureMeters,
    double LineOfSightElevationMeters,
    double FirstFresnelZoneRadiusMeters,
    double RequiredFresnelClearanceMeters,
    double ActualClearanceMeters,
    double ClearanceDeficitMeters
);