using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Exceptions;
using RadioLinkPlannervfV.Terrain.Providers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public sealed class FallbackTerrainElevationBatchProviderTests
{
    [Fact]
    public async Task GetElevationsAsync_FirstProviderSupportsBatch_ReturnsBatchResultsWithoutCallingSecondProvider()
    {
        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500),
                new GeoCoordinate(35.7100, 51.4600)
            };

        var firstProvider =
            new BatchStubTerrainElevationProvider(
                "First",
                new[]
                {
                    new TerrainElevationResult(1200, "First"),
                    new TerrainElevationResult(1250, "First"),
                    new TerrainElevationResult(1300, "First")
                });

        var secondProvider =
            new StubTerrainElevationProvider(
                "Second",
                new TerrainElevationResult(
                    1400,
                    "Second"));

        var provider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    firstProvider,
                    secondProvider
                });

        var batchProvider =
            Assert.IsAssignableFrom<
                ITerrainElevationBatchProvider>(
                provider);

        IReadOnlyList<TerrainElevationResult> results =
            await batchProvider.GetElevationsAsync(
                coordinates,
                CancellationToken.None);

        Assert.Equal(3, results.Count);

        Assert.Equal(
            1200,
            results[0].ElevationMeters);

        Assert.Equal(
            1250,
            results[1].ElevationMeters);

        Assert.Equal(
            1300,
            results[2].ElevationMeters);

        Assert.Equal(
            1,
            firstProvider.BatchCallCount);

        Assert.Equal(
            0,
            secondProvider.CallCount);
    }


    [Fact]
    public async Task GetElevationsAsync_FirstBatchProviderFails_UsesSecondProviderSequentially()
    {
        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500),
                new GeoCoordinate(35.7100, 51.4600)
            };

        var firstProvider =
            new FailingBatchTerrainElevationProvider(
                "First");

        var secondProvider =
            new StubTerrainElevationProvider(
                "Second",
                new TerrainElevationResult(
                    1400,
                    "Second"));

        var provider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    firstProvider,
                    secondProvider
                });

        var batchProvider =
            Assert.IsAssignableFrom<
                ITerrainElevationBatchProvider>(
                provider);

        IReadOnlyList<TerrainElevationResult> results =
            await batchProvider.GetElevationsAsync(
                coordinates,
                CancellationToken.None);

        Assert.Equal(3, results.Count);

        Assert.All(
            results,
            result =>
                Assert.Equal(
                    1400,
                    result.ElevationMeters));

        Assert.Equal(
            1,
            firstProvider.BatchCallCount);

        Assert.Equal(
            3,
            secondProvider.CallCount);
    }


    [Fact]
    public async Task GetElevationsAsync_FirstProviderDoesNotSupportBatch_UsesSequentialCalls()
    {
        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500),
                new GeoCoordinate(35.7100, 51.4600)
            };

        var firstProvider =
            new StubTerrainElevationProvider(
                "First",
                new TerrainElevationResult(
                    1100,
                    "First"));

        var secondProvider =
            new StubTerrainElevationProvider(
                "Second",
                new TerrainElevationResult(
                    1400,
                    "Second"));

        var provider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    firstProvider,
                    secondProvider
                });

        var batchProvider =
            Assert.IsAssignableFrom<
                ITerrainElevationBatchProvider>(
                provider);

        IReadOnlyList<TerrainElevationResult> results =
            await batchProvider.GetElevationsAsync(
                coordinates,
                CancellationToken.None);

        Assert.Equal(3, results.Count);

        Assert.All(
            results,
            result =>
                Assert.Equal(
                    1100,
                    result.ElevationMeters));

        Assert.Equal(
            3,
            firstProvider.CallCount);

        Assert.Equal(
            0,
            secondProvider.CallCount);
    }


    [Fact]
    public async Task GetElevationsAsync_FirstBatchProviderReturnsWrongCount_UsesSecondProvider()
    {
        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500),
                new GeoCoordinate(35.7100, 51.4600)
            };

        var firstProvider =
            new BatchStubTerrainElevationProvider(
                "First",
                new[]
                {
                    new TerrainElevationResult(
                        1200,
                        "First")
                });

        var secondProvider =
            new StubTerrainElevationProvider(
                "Second",
                new TerrainElevationResult(
                    1400,
                    "Second"));

        var provider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    firstProvider,
                    secondProvider
                });

        var batchProvider =
            Assert.IsAssignableFrom<
                ITerrainElevationBatchProvider>(
                provider);

        IReadOnlyList<TerrainElevationResult> results =
            await batchProvider.GetElevationsAsync(
                coordinates,
                CancellationToken.None);

        Assert.Equal(3, results.Count);

        Assert.All(
            results,
            result =>
                Assert.Equal(
                    1400,
                    result.ElevationMeters));

        Assert.Equal(
            1,
            firstProvider.BatchCallCount);

        Assert.Equal(
            3,
            secondProvider.CallCount);
    }


    [Fact]
    public async Task GetElevationsAsync_AllProvidersFail_ThrowsControlledException()
    {
        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500),
                new GeoCoordinate(35.7100, 51.4600)
            };

        var firstProvider =
            new FailingBatchTerrainElevationProvider(
                "First");

        var secondProvider =
            new FailingBatchTerrainElevationProvider(
                "Second");

        var provider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    firstProvider,
                    secondProvider
                });

        var batchProvider =
            Assert.IsAssignableFrom<
                ITerrainElevationBatchProvider>(
                provider);

        TerrainDataUnavailableException exception =
            await Assert.ThrowsAsync<
                TerrainDataUnavailableException>(
                () =>
                    batchProvider.GetElevationsAsync(
                        coordinates,
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

        Assert.All(
            aggregateException.InnerExceptions,
            error =>
                Assert.IsType<InvalidOperationException>(
                    error));

        Assert.Equal(
            1,
            firstProvider.BatchCallCount);

        Assert.Equal(
            1,
            secondProvider.BatchCallCount);
    }


    private sealed class BatchStubTerrainElevationProvider
        : ITerrainElevationProvider,
          ITerrainElevationBatchProvider
    {
        private readonly IReadOnlyList<TerrainElevationResult> _results;

        public BatchStubTerrainElevationProvider(
            string name,
            IReadOnlyList<TerrainElevationResult> results)
        {
            Name = name;
            _results = results;
        }

        public string Name { get; }

        public int BatchCallCount { get; private set; }

        public Task<TerrainElevationResult> GetElevationAsync(
            GeoCoordinate coordinate,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "Single-coordinate calls should not be used by this batch test.");
        }

        public Task<IReadOnlyList<TerrainElevationResult>>
            GetElevationsAsync(
                IReadOnlyList<GeoCoordinate> coordinates,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            BatchCallCount++;

            return Task.FromResult(
                _results);
        }
    }


    private sealed class FailingBatchTerrainElevationProvider
        : ITerrainElevationProvider,
          ITerrainElevationBatchProvider
    {
        public FailingBatchTerrainElevationProvider(
            string name)
        {
            Name = name;
        }

        public string Name { get; }

        public int BatchCallCount { get; private set; }

        public Task<TerrainElevationResult> GetElevationAsync(
            GeoCoordinate coordinate,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "Single-coordinate calls should not be used by this batch test.");
        }

        public Task<IReadOnlyList<TerrainElevationResult>>
            GetElevationsAsync(
                IReadOnlyList<GeoCoordinate> coordinates,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            BatchCallCount++;

            throw new InvalidDataException(
                "Simulated batch provider failure.");
        }
    }


    private sealed class StubTerrainElevationProvider
        : ITerrainElevationProvider
    {
        private readonly TerrainElevationResult _result;

        public StubTerrainElevationProvider(
            string name,
            TerrainElevationResult result)
        {
            Name = name;
            _result = result;
        }

        public string Name { get; }

        public int CallCount { get; private set; }

        public Task<TerrainElevationResult> GetElevationAsync(
            GeoCoordinate coordinate,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                _result);
        }
    }
}