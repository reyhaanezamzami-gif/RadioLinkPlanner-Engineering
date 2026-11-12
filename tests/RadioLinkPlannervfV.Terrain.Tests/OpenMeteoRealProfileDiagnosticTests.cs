using System.Net.Http;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Models;
using RadioLinkPlannervfV.Terrain.Providers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class OpenMeteoRealProfileDiagnosticTests
{
    [Fact]
    public async Task ReadRealOpenMeteoProfile_Prints100GreatCircleSamples()
    {
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        var provider =
            new OpenMeteoTerrainElevationProvider(
                httpClient,
                new OnlineTerrainOptions
                {
                    BaseUrl = "https://api.open-meteo.com",
                    Enabled = true
                });

        var start = new GeoCoordinate(
            36.322331526431554,
            59.57538704550899);

        var end = new GeoCoordinate(
            36.32446807551602,
            59.580907558046874);

        var coordinates = new List<GeoCoordinate>(100);

        for (int index = 0; index < 100; index++)
        {
            double fraction = (double)index / 99.0;

            coordinates.Add(
                GreatCircleInterpolator.Interpolate(
                    start,
                    end,
                    fraction));
        }

        IReadOnlyList<TerrainElevationResult> results =
            await provider.GetElevationsAsync(
                coordinates);

        Assert.Equal(100, results.Count);

        Console.WriteLine("Source: Open-Meteo");
        Console.WriteLine(
            "Endpoint: https://api.open-meteo.com/v1/elevation");
        Console.WriteLine();
        Console.WriteLine(
            "Index | Latitude           | Longitude          | Elevation");
        Console.WriteLine(
            "------------------------------------------------------------");

        for (int index = 0; index < results.Count; index++)
        {
            GeoCoordinate coordinate = coordinates[index];

            Console.WriteLine(
                $"{index,5} | " +
                $"{coordinate.Latitude,18:F12} | " +
                $"{coordinate.Longitude,18:F12} | " +
                $"{results[index].ElevationMeters,9:F1} m");
        }

        double minimum =
            results.Min(result => result.ElevationMeters);

        double maximum =
            results.Max(result => result.ElevationMeters);

        Console.WriteLine();
        Console.WriteLine($"Minimum elevation: {minimum:F1} m");
        Console.WriteLine($"Maximum elevation: {maximum:F1} m");
        Console.WriteLine($"Range: {maximum - minimum:F1} m");
        Console.WriteLine(
            $"Start elevation: {results[0].ElevationMeters:F1} m");
        Console.WriteLine(
            $"End elevation: {results[^1].ElevationMeters:F1} m");
    }
}