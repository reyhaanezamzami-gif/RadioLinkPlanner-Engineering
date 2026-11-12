using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface ITerrainElevationProvider
{
    string Name { get; }

    Task<TerrainElevationResult> GetElevationAsync(
        GeoCoordinate coordinate,
        CancellationToken cancellationToken = default);
}
