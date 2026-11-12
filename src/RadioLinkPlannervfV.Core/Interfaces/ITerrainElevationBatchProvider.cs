using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface ITerrainElevationBatchProvider
{
    Task<IReadOnlyList<TerrainElevationResult>> GetElevationsAsync(
        IReadOnlyList<GeoCoordinate> coordinates,
        CancellationToken cancellationToken = default);
}