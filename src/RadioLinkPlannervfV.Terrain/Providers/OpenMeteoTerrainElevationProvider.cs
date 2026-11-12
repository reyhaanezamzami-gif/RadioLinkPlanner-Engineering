using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Models;

namespace RadioLinkPlannervfV.Terrain.Providers;

public sealed class OpenMeteoTerrainElevationProvider
    : ITerrainElevationProvider,
      ITerrainElevationBatchProvider
{
    private readonly HttpClient _httpClient;
    private readonly OnlineTerrainOptions _options;


    public OpenMeteoTerrainElevationProvider(
        HttpClient httpClient,
        OnlineTerrainOptions options)
    {
        _httpClient =
            httpClient ??
            throw new ArgumentNullException(
                nameof(httpClient));

        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));
    }


    public string Name =>
        "Open-Meteo";


    public async Task<TerrainElevationResult> GetElevationAsync(
        GeoCoordinate coordinate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();


        if (!_options.Enabled)
        {
            throw new InvalidOperationException(
                "Online terrain provider is disabled.");
        }


        string latitude =
            coordinate.Latitude.ToString(
                "G",
                CultureInfo.InvariantCulture);


        string longitude =
            coordinate.Longitude.ToString(
                "G",
                CultureInfo.InvariantCulture);


        string requestUri =
            $"{_options.BaseUrl.TrimEnd('/')}/v1/elevation" +
            $"?latitude={latitude}" +
            $"&longitude={longitude}";


        OpenMeteoElevationResponse? response =
            await _httpClient.GetFromJsonAsync<OpenMeteoElevationResponse>(
                requestUri,
                cancellationToken);


        ValidateResponse(response);


        return new TerrainElevationResult(
            response!.Elevation![0],
            Name);
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


        cancellationToken.ThrowIfCancellationRequested();


        if (!_options.Enabled)
        {
            throw new InvalidOperationException(
                "Online terrain provider is disabled.");
        }


        string latitudes =
            string.Join(
                ",",
                coordinates.Select(
                    x => x.Latitude.ToString(
                        "G",
                        CultureInfo.InvariantCulture)));


        string longitudes =
            string.Join(
                ",",
                coordinates.Select(
                    x => x.Longitude.ToString(
                        "G",
                        CultureInfo.InvariantCulture)));


        string requestUri =
            $"{_options.BaseUrl.TrimEnd('/')}/v1/elevation" +
            $"?latitude={latitudes}" +
            $"&longitude={longitudes}";


        OpenMeteoElevationResponse? response =
            await _httpClient.GetFromJsonAsync<OpenMeteoElevationResponse>(
                requestUri,
                cancellationToken);


        ValidateResponse(response);


        if (response!.Elevation!.Count != coordinates.Count)
        {
            throw new InvalidDataException(
                "Open-Meteo elevation count mismatch.");
        }


        return response.Elevation
            .Select(
                elevation =>
                    new TerrainElevationResult(
                        elevation,
                        Name))
            .ToList();
    }


    private static void ValidateResponse(
        OpenMeteoElevationResponse? response)
    {
        if (response?.Elevation is null ||
            response.Elevation.Count == 0)
        {
            throw new InvalidDataException(
                "Open-Meteo returned no elevation data.");
        }


        foreach (double elevation in response.Elevation)
        {
            if (double.IsNaN(elevation) ||
                double.IsInfinity(elevation))
            {
                throw new InvalidDataException(
                    "Open-Meteo returned invalid elevation.");
            }
        }
    }


    private sealed class OpenMeteoElevationResponse
    {
        [JsonPropertyName("elevation")]
        public List<double>? Elevation { get; init; }
    }
}