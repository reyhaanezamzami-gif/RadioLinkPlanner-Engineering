namespace RadioLinkPlannervfV.Core.Models;

public sealed record TerrainProfileResult
(
    IReadOnlyList<TerrainPoint> Points,
    string Source
);
