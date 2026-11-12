using System.Net;
using System.Net.Http;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Models;
using RadioLinkPlannervfV.Terrain.Providers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class OpenMeteoTerrainElevationProviderTests
{
    [Fact]
    public async Task GetElevationAsync_ValidResponse_ReturnsElevation()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[1234.5]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = true
                });

        TerrainElevationResult result =
            await provider.GetElevationAsync(
                new GeoCoordinate(35.6892, 51.3890));

        Assert.Equal(1234.5, result.ElevationMeters);
        Assert.Equal("Open-Meteo", result.Source);
        Assert.NotNull(handler.RequestUri);
        Assert.Contains(
            "latitude=35.6892",
            handler.RequestUri!.Query);
        Assert.Contains(
            "longitude=51.389",
            handler.RequestUri.Query);
    }

    [Fact]
    public async Task GetElevationAsync_EmptyResponse_ThrowsInvalidDataException()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = true
                });

        await Assert.ThrowsAsync<InvalidDataException>(
            () =>
                provider.GetElevationAsync(
                    new GeoCoordinate(35.6892, 51.3890)));
    }

    [Fact]
    public async Task GetElevationAsync_ProviderDisabled_ThrowsInvalidOperationException()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[1234.5]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = false
                });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                provider.GetElevationAsync(
                    new GeoCoordinate(35.6892, 51.3890)));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetElevationAsync_Cancelled_ThrowsOperationCanceledException()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[1234.5]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = true
                });

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () =>
                provider.GetElevationAsync(
                    new GeoCoordinate(35.6892, 51.3890),
                    cancellationTokenSource.Token));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetElevationsAsync_MultipleCoordinates_ReturnsElevationsInInputOrder()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[1173.0,1205.0,1320.5]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = true
                });

        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500),
                new GeoCoordinate(35.7100, 51.4600)
            };

        IReadOnlyList<TerrainElevationResult> results =
            await provider.GetElevationsAsync(
                coordinates);

        Assert.Equal(3, results.Count);

        Assert.Equal(
            1173.0,
            results[0].ElevationMeters);

        Assert.Equal(
            1205.0,
            results[1].ElevationMeters);

        Assert.Equal(
            1320.5,
            results[2].ElevationMeters);

        Assert.All(
            results,
            result =>
                Assert.Equal(
                    "Open-Meteo",
                    result.Source));

        Assert.Equal(
            1,
            handler.CallCount);

        Assert.NotNull(handler.RequestUri);

        Assert.Contains(
            "latitude=35.6892,35.7,35.71",
            handler.RequestUri!.Query);

        Assert.Contains(
            "longitude=51.389,51.45,51.46",
            handler.RequestUri.Query);
    }

    [Fact]
    public async Task GetElevationsAsync_EmptyCoordinates_ReturnsEmptyWithoutHttpRequest()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[1234.5]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = true
                });

        IReadOnlyList<TerrainElevationResult> results =
            await provider.GetElevationsAsync(
                Array.Empty<GeoCoordinate>());

        Assert.Empty(results);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetElevationsAsync_ResponseCountDoesNotMatchCoordinates_ThrowsInvalidDataException()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[1173.0,1205.0]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = true
                });

        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500),
                new GeoCoordinate(35.7100, 51.4600)
            };

        await Assert.ThrowsAsync<InvalidDataException>(
            () =>
                provider.GetElevationsAsync(
                    coordinates));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetElevationsAsync_ProviderDisabled_ThrowsInvalidOperationException()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[1173.0,1205.0]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = false
                });

        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500)
            };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                provider.GetElevationsAsync(
                    coordinates));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetElevationsAsync_Cancelled_ThrowsOperationCanceledException()
    {
        var handler =
            new StubHttpMessageHandler(
                HttpStatusCode.OK,
                """{"elevation":[1173.0,1205.0]}""");

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = true
                });

        var coordinates =
            new[]
            {
                new GeoCoordinate(35.6892, 51.3890),
                new GeoCoordinate(35.7000, 51.4500)
            };

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () =>
                provider.GetElevationsAsync(
                    coordinates,
                    cancellationTokenSource.Token));

        Assert.Equal(0, handler.CallCount);
    }

    private sealed class StubHttpMessageHandler
        : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public StubHttpMessageHandler(
            HttpStatusCode statusCode,
            string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        public int CallCount { get; private set; }

        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CallCount++;
            RequestUri = request.RequestUri;

            var response =
                new HttpResponseMessage(_statusCode)
                {
                    Content =
                        new StringContent(
                            _responseBody)
                };

            return Task.FromResult(response);
        }
    }
}