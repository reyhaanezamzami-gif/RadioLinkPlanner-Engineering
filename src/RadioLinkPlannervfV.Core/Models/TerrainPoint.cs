namespace RadioLinkPlannervfV.Core.Models;

public sealed record TerrainPoint
(
    GeoCoordinate Coordinate,
    double DistanceFromStartMeters,
    double ElevationMeters
);
