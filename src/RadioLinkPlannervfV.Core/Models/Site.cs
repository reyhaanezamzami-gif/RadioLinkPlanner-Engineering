namespace RadioLinkPlannervfV.Core.Models;

public sealed record Site
(
    string Name,
    GeoCoordinate Coordinate,
    double GroundElevationMeters,
    double AntennaHeightMeters
);
