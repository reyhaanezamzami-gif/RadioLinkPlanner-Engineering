
using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Exceptions;

namespace RadioLinkPlannervfV.Terrain.Providers;

public sealed class FallbackTerrainElevationProvider
    : ITerrainElevationProvider,
      ITerrainElevationBatchProvider
{
    private readonly IReadOnlyList<ITerrainElevationProvider> _providers;


    public FallbackTerrainElevationProvider(
        IEnumerable<ITerrainElevationProvider> providers)
    {
        _providers =
            providers?.ToList()
            ?? throw new ArgumentNullException(
                nameof(providers));


        if (_providers.Count == 0)
        {
            throw new ArgumentException(
                "At least one terrain provider is required.",
                nameof(providers));
        }
    }


    public string Name =>
        "Fallback";


    public async Task<TerrainElevationResult> GetElevationAsync(
        GeoCoordinate coordinate,
        CancellationToken cancellationToken = default)
    {
        List<Exception> errors =
            new();


        foreach (ITerrainElevationProvider provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();


            try
            {
                TerrainElevationResult result =
                    await provider.GetElevationAsync(
                        coordinate,
                        cancellationToken);


                return result;
            }
            catch (Exception ex)
            {
                errors.Add(
                    new InvalidOperationException(
                        $"Terrain provider '{provider.Name}' failed.",
                        ex));
            }
        }


        throw new TerrainDataUnavailableException(
            coordinate,
            errors);
    }


    public async Task<IReadOnlyList<TerrainElevationResult>>
        GetElevationsAsync(
            IReadOnlyList<GeoCoordinate> coordinates,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            coordinates);


        if (coordinates.Count == 0)
        {
            return Array.Empty<TerrainElevationResult>();
        }


        List<Exception> errors =
            new();


        foreach (ITerrainElevationProvider provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();


            try
            {
                if (provider is ITerrainElevationBatchProvider batchProvider)
                {
                    IReadOnlyList<TerrainElevationResult> results =
                        await batchProvider.GetElevationsAsync(
                            coordinates,
                            cancellationToken);


                    if (results.Count != coordinates.Count)
                    {
                        throw new InvalidDataException(
                            $"Provider '{provider.Name}' returned invalid result count.");
                    }


                    return results;
                }


                var sequentialResults =
                    new List<TerrainElevationResult>(
                        coordinates.Count);


                foreach (GeoCoordinate coordinate in coordinates)
                {
                    cancellationToken.ThrowIfCancellationRequested();


                    TerrainElevationResult result =
                        await provider.GetElevationAsync(
                            coordinate,
                            cancellationToken);


                    sequentialResults.Add(result);
                }


                return sequentialResults;
            }
            catch (Exception ex)
            {
                errors.Add(
                    new InvalidOperationException(
                        $"Terrain provider '{provider.Name}' failed.",
                        ex));
            }
        }


        throw new TerrainDataUnavailableException(
            coordinates[0],
            errors);
    }
}

