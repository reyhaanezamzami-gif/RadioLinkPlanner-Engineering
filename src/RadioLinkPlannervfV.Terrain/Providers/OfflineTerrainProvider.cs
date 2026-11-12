using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Providers;

public sealed class OfflineTerrainProvider
    : ITerrainElevationProvider
{
    private readonly IDemReader _demReader;


    public OfflineTerrainProvider(
        IDemReader demReader)
    {
        _demReader =
            demReader ??
            throw new ArgumentNullException(
                nameof(demReader));
    }


    public string Name =>
        "Offline-SRTM";


    public Task<TerrainElevationResult> GetElevationAsync(
        GeoCoordinate coordinate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();


        try
        {
            TerrainElevationResult result =
                _demReader.ReadElevation(
                    coordinate);


            if (double.IsNaN(result.ElevationMeters) ||
                double.IsInfinity(result.ElevationMeters))
            {
                throw new InvalidDataException(
                    "Offline DEM returned invalid elevation.");
            }


            return Task.FromResult(result);
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"Offline DEM tile not found for coordinate {coordinate.Latitude},{coordinate.Longitude}.",
                ex);
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidOperationException(
                $"Offline DEM data invalid for coordinate {coordinate.Latitude},{coordinate.Longitude}.",
                ex);
        }
    }
}