using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Exceptions;
using RadioLinkPlannervfV.Terrain.Providers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public sealed class FallbackTerrainElevationProviderTests
{
    [Fact]
    public async Task GetElevationAsync_FirstProviderSucceeds_ReturnsFirstProviderResult()
    {
        var coordinate =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var expected =
            new TerrainElevationResult(
                1200,
                "First");

        var firstProvider =
            new StubTerrainElevationProvider(
                "First",
                expected);

        var secondProvider =
            new StubTerrainElevationProvider(
                "Second",
                new TerrainElevationResult(
                    1300,
                    "Second"));

        var provider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    firstProvider,
                    secondProvider
                });

        var result =
            await provider.GetElevationAsync(
                coordinate,
                CancellationToken.None);

        Assert.Equal(
            expected,
            result);

        Assert.Equal(
            1,
            firstProvider.CallCount);

        Assert.Equal(
            0,
            secondProvider.CallCount);
    }

    [Fact]
    public async Task GetElevationAsync_FirstProviderFails_SecondProviderSucceeds_ReturnsSecondProviderResult()
    {
        var coordinate =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var firstProvider =
            new StubTerrainElevationProvider(
                "First",
                new FileNotFoundException(
                    "First provider failed."));

        var expected =
            new TerrainElevationResult(
                1250,
                "Second");

        var secondProvider =
            new StubTerrainElevationProvider(
                "Second",
                expected);

        var provider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    firstProvider,
                    secondProvider
                });

        var result =
            await provider.GetElevationAsync(
                coordinate,
                CancellationToken.None);

        Assert.Equal(
            expected,
            result);

        Assert.Equal(
            1,
            firstProvider.CallCount);

        Assert.Equal(
            1,
            secondProvider.CallCount);
    }

    [Fact]
    public async Task GetElevationAsync_AllProvidersFail_ThrowsControlledException()
    {
        var coordinate =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var firstProvider =
            new StubTerrainElevationProvider(
                "First",
                new FileNotFoundException(
                    "First provider failed."));

        var secondProvider =
            new StubTerrainElevationProvider(
                "Second",
                new InvalidDataException(
                    "Second provider failed."));

        var provider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    firstProvider,
                    secondProvider
                });

        var exception =
            await Assert.ThrowsAsync<TerrainDataUnavailableException>(
                () => provider.GetElevationAsync(
                    coordinate,
                    CancellationToken.None));

        Assert.Contains(
            "35.6892",
            exception.Message);

        Assert.Contains(
            "51.389",
            exception.Message);

        Assert.NotNull(
            exception.InnerException);

        var aggregateException =
            Assert.IsType<AggregateException>(
                exception.InnerException);

        Assert.Equal(
            2,
            aggregateException.InnerExceptions.Count);

        Assert.Equal(
            1,
            firstProvider.CallCount);

        Assert.Equal(
            1,
            secondProvider.CallCount);
    }

    [Fact]
    public void Constructor_WithNoProviders_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new FallbackTerrainElevationProvider(
                    Array.Empty<ITerrainElevationProvider>()));
    }

    [Fact]
    public void Constructor_WithNullProviders_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new FallbackTerrainElevationProvider(
                    null!));
    }

    private sealed class StubTerrainElevationProvider
        : ITerrainElevationProvider
    {
        private readonly TerrainElevationResult? _result;
        private readonly Exception? _exception;

        public StubTerrainElevationProvider(
            string name,
            TerrainElevationResult result)
        {
            Name = name;
            _result = result;
        }

        public StubTerrainElevationProvider(
            string name,
            Exception exception)
        {
            Name = name;
            _exception = exception;
        }

        public string Name { get; }

        public int CallCount { get; private set; }

        public Task<TerrainElevationResult> GetElevationAsync(
            GeoCoordinate coordinate,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            cancellationToken.ThrowIfCancellationRequested();

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(
                _result!);
        }
    }
}