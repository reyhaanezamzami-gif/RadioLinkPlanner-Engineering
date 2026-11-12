using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Generators;

namespace RadioLinkPlannervfV.Terrain.Tests;

public sealed class TerrainProfileGeneratorBatchTests
{
    [Fact]
    public async Task GenerateAsync_WhenProviderSupportsBatch_UsesSingleBatchCall()
    {
        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        var provider =
            new BatchRecordingTerrainElevationProvider();

        var generator =
            new TerrainProfileGenerator(
                provider,
                new HaversineDistanceCalculator());

        TerrainProfileResult results =
            await generator.GenerateAsync(
                start,
                end,
                5,
                CancellationToken.None);

        Assert.Equal(5, results.Points.Count);

        Assert.Equal(
            1,
            provider.BatchCallCount);

        Assert.Equal(
            0,
            provider.SingleCallCount);

        Assert.Equal(
            1000,
            results.Points[0].ElevationMeters);

        Assert.Equal(
            1004,
            results.Points[4].ElevationMeters);
    }

    private sealed class BatchRecordingTerrainElevationProvider
        : ITerrainElevationProvider,
          ITerrainElevationBatchProvider
    {
        public string Name => "BatchRecording";

        public int BatchCallCount { get; private set; }

        public int SingleCallCount { get; private set; }

        public Task<TerrainElevationResult> GetElevationAsync(
            GeoCoordinate coordinate,
            CancellationToken cancellationToken = default)
        {
            SingleCallCount++;

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                new TerrainElevationResult(
                    9999,
                    "Single"));
        }

        public Task<IReadOnlyList<TerrainElevationResult>>
            GetElevationsAsync(
                IReadOnlyList<GeoCoordinate> coordinates,
                CancellationToken cancellationToken = default)
        {
            BatchCallCount++;

            cancellationToken.ThrowIfCancellationRequested();

            var results =
                new List<TerrainElevationResult>(
                    coordinates.Count);

            for (
                int index = 0;
                index < coordinates.Count;
                index++)
            {
                results.Add(
                    new TerrainElevationResult(
                        1000 + index,
                        "Batch"));
            }

            return Task.FromResult<
                IReadOnlyList<TerrainElevationResult>>(
                results);
        }
    }
}