using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface ITerrainProfileGenerator
{
    Task<TerrainProfileResult> GenerateAsync(
        GeoCoordinate start,
        GeoCoordinate end,
        int sampleCount,
        CancellationToken cancellationToken = default);
}