namespace RadioLinkPlannervfV.Core.Models;

public sealed record LinkObstruction
(
    GeoCoordinate Coordinate,
    double DistanceFromSiteAMeters,
    double TerrainElevationMeters,
    double TerrainElevationWithCurvatureMeters,
    double LineOfSightElevationMeters,
    double ObstructionAmountMeters
);