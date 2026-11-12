using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IDemReader
{
    TerrainElevationResult ReadElevation(
        GeoCoordinate coordinate);
}
