using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Generators;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class TerrainProfileGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ReturnsRequestedNumberOfPoints()
    {
        var generator =
            new TerrainProfileGenerator(
                new TestTerrainElevationProvider(),
                new HaversineDistanceCalculator());

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        TerrainProfileResult result =
            await generator.GenerateAsync(
                start,
                end,
                5);

        Assert.Equal(5, result.Points.Count);
    }

    [Fact]
    public async Task GenerateAsync_FirstAndLastPointsMatchEndpoints()
    {
        var generator =
            new TerrainProfileGenerator(
                new TestTerrainElevationProvider(),
                new HaversineDistanceCalculator());

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        TerrainProfileResult result =
            await generator.GenerateAsync(
                start,
                end,
                5);

        Assert.Equal(
            start.Latitude,
            result.Points[0].Coordinate.Latitude,
            8);

        Assert.Equal(
            start.Longitude,
            result.Points[0].Coordinate.Longitude,
            8);

        Assert.Equal(
            end.Latitude,
            result.Points[^1].Coordinate.Latitude,
            8);

        Assert.Equal(
            end.Longitude,
            result.Points[^1].Coordinate.Longitude,
            8);
    }

    [Fact]
    public async Task GenerateAsync_FirstDistanceShouldBeZero()
    {
        var generator =
            new TerrainProfileGenerator(
                new TestTerrainElevationProvider(),
                new HaversineDistanceCalculator());

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        TerrainProfileResult result =
            await generator.GenerateAsync(
                start,
                end,
                5);

        Assert.Equal(
            0.0,
            result.Points[0].DistanceFromStartMeters,
            10);
    }

    [Fact]
    public async Task GenerateAsync_LastDistanceShouldEqualTotalDistance()
    {
        var distanceCalculator =
            new HaversineDistanceCalculator();

        var generator =
            new TerrainProfileGenerator(
                new TestTerrainElevationProvider(),
                distanceCalculator);

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        double expectedDistance =
            distanceCalculator.CalculateDistanceMeters(
                start,
                end);

        TerrainProfileResult result =
            await generator.GenerateAsync(
                start,
                end,
                5);

        Assert.Equal(
            expectedDistance,
            result.Points[^1].DistanceFromStartMeters,
            8);
    }

    [Fact]
    public async Task GenerateAsync_DistancesShouldBeStrictlyIncreasing()
    {
        var generator =
            new TerrainProfileGenerator(
                new TestTerrainElevationProvider(),
                new HaversineDistanceCalculator());

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        TerrainProfileResult result =
            await generator.GenerateAsync(
                start,
                end,
                10);

        for (int index = 1; index < result.Points.Count; index++)
        {
            Assert.True(
                result.Points[index].DistanceFromStartMeters >
                result.Points[index - 1].DistanceFromStartMeters);
        }
    }

    [Fact]
    public async Task GenerateAsync_ShouldUseElevationReturnedByProvider()
    {
        var provider =
            new RecordingTerrainElevationProvider();

        var generator =
            new TerrainProfileGenerator(
                provider,
                new HaversineDistanceCalculator());

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        TerrainProfileResult result =
            await generator.GenerateAsync(
                start,
                end,
                5);

        Assert.Equal(5, result.Points.Count);
        Assert.Equal(5, provider.CallCount);

        for (int index = 0; index < result.Points.Count; index++)
        {
            Assert.Equal(
                1000.0 + index,
                result.Points[index].ElevationMeters,
                10);
        }
    }

    [Fact]
    public async Task GenerateAsync_ShouldCallProviderOncePerSample()
    {
        var provider =
            new RecordingTerrainElevationProvider();

        var generator =
            new TerrainProfileGenerator(
                provider,
                new HaversineDistanceCalculator());

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        const int sampleCount = 7;

        await generator.GenerateAsync(
            start,
            end,
            sampleCount);

        Assert.Equal(
            sampleCount,
            provider.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_SampleCountLessThanTwo_ShouldThrow()
    {
        var generator =
            new TerrainProfileGenerator(
                new TestTerrainElevationProvider(),
                new HaversineDistanceCalculator());

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () =>
                generator.GenerateAsync(
                    start,
                    end,
                    1));
    }

    [Fact]
    public async Task GenerateAsync_CancelledToken_ShouldThrow()
    {
        var generator =
            new TerrainProfileGenerator(
                new TestTerrainElevationProvider(),
                new HaversineDistanceCalculator());

        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () =>
                generator.GenerateAsync(
                    start,
                    end,
                    5,
                    cancellationTokenSource.Token));
    }

    private sealed class TestTerrainElevationProvider
        : ITerrainElevationProvider
    {
        public string Name => "Test";

        public Task<TerrainElevationResult> GetElevationAsync(
            GeoCoordinate coordinate,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new TerrainElevationResult(
                    1000.0,
                    "Test"));
        }
    }

    private sealed class RecordingTerrainElevationProvider
        : ITerrainElevationProvider
    {
        public string Name => "Recording";

        public int CallCount { get; private set; }

        public Task<TerrainElevationResult> GetElevationAsync(
            GeoCoordinate coordinate,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double elevation =
                1000.0 + CallCount;

            CallCount++;

            return Task.FromResult(
                new TerrainElevationResult(
                    elevation,
                    "Recording"));
        }
    }
}