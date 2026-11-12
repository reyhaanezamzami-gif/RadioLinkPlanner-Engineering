using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Providers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class OfflineTerrainProviderTests
{
    [Fact]
    public async Task GetElevationAsync_ReturnsResultFromDemReader()
    {
        var expected =
            new TerrainElevationResult(
                1234.5,
                "SRTM");

        var provider =
            new OfflineTerrainProvider(
                new TestDemReader(expected));

        TerrainElevationResult result =
            await provider.GetElevationAsync(
                new GeoCoordinate(
                    35.6892,
                    51.3890));

        Assert.Equal(
            expected,
            result);
    }

    [Fact]
    public async Task GetElevationAsync_ThrowsWhenCancelled()
    {
        var provider =
            new OfflineTerrainProvider(
                new TestDemReader(
                    new TerrainElevationResult(
                        1000,
                        "Test")));

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<
            OperationCanceledException>(
            () =>
                provider.GetElevationAsync(
                    new GeoCoordinate(
                        35.6892,
                        51.3890),
                    cancellationTokenSource.Token));
    }

    private sealed class TestDemReader
        : IDemReader
    {
        private readonly TerrainElevationResult _result;

        public TestDemReader(
            TerrainElevationResult result)
        {
            _result = result;
        }

        public TerrainElevationResult ReadElevation(
            GeoCoordinate coordinate)
        {
            return _result;
        }
    }
}
