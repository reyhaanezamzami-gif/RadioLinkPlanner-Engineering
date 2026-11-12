namespace RadioLinkPlannervfV.Core.Models;

public sealed record PropagationSettings
(
    double KFactor = 4.0 / 3.0,
    double EarthRadiusMeters = 6371000.0,
    double FresnelClearanceFactor = 0.6
);