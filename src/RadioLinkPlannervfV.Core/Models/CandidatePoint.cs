namespace RadioLinkPlannervfV.Core.Models;

public sealed record CandidatePoint
(
    GeoCoordinate Coordinate,
    double GroundElevationMeters,
    double AntennaHeightMeters
);