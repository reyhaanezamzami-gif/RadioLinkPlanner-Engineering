using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Calculators;

namespace RadioLinkPlannervfV.Terrain.Generators;

public sealed class TerrainProfileGenerator
    : ITerrainProfileGenerator
{
    private readonly ITerrainElevationProvider _elevationProvider;
    private readonly IDistanceCalculator _distanceCalculator;

    public TerrainProfileGenerator(
        ITerrainElevationProvider elevationProvider,
        IDistanceCalculator distanceCalculator)
    {
        _elevationProvider =
            elevationProvider ??
            throw new ArgumentNullException(
                nameof(elevationProvider));

        _distanceCalculator =
            distanceCalculator ??
            throw new ArgumentNullException(
                nameof(distanceCalculator));
    }

    public async Task<TerrainProfileResult> GenerateAsync(
        GeoCoordinate start,
        GeoCoordinate end,
        int sampleCount,
        CancellationToken cancellationToken = default)
    {
        if (sampleCount < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleCount),
                "Sample count must be at least 2.");
        }

        double totalDistanceMeters =
            _distanceCalculator.CalculateDistanceMeters(
                start,
                end);

        var coordinates =
            new List<GeoCoordinate>(
                sampleCount);

        for (int index = 0;
             index < sampleCount;
             index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double fraction =
                (double)index /
                (sampleCount - 1);

            coordinates.Add(
                GreatCircleInterpolator.Interpolate(
                    start,
                    end,
                    fraction));
        }

        IReadOnlyList<TerrainElevationResult> elevations;

        if (_elevationProvider
            is ITerrainElevationBatchProvider batchProvider)
        {
            elevations =
                await batchProvider.GetElevationsAsync(
                    coordinates,
                    cancellationToken);
        }
        else
        {
            var results =
                new List<TerrainElevationResult>(
                    sampleCount);

            foreach (GeoCoordinate coordinate in coordinates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                results.Add(
                    await _elevationProvider.GetElevationAsync(
                        coordinate,
                        cancellationToken));
            }

            elevations =
                results;
        }

        if (elevations.Count != sampleCount)
        {
            throw new InvalidDataException(
                $"Terrain provider returned {elevations.Count} results for {sampleCount} samples.");
        }

        var terrainPoints =
            new List<TerrainPoint>(
                sampleCount);

        for (int index = 0;
             index < sampleCount;
             index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double fraction =
                (double)index /
                (sampleCount - 1);

            terrainPoints.Add(
                new TerrainPoint(
                    coordinates[index],
                    totalDistanceMeters * fraction,
                    elevations[index].ElevationMeters));
        }

        string source =
            elevations[0].Source;

        if (elevations.Any(
                elevation =>
                    !string.Equals(
                        elevation.Source,
                        source,
                        StringComparison.Ordinal)))
        {
            source = "Mixed";
        }

        return new TerrainProfileResult(
            terrainPoints,
            source);
    }
}