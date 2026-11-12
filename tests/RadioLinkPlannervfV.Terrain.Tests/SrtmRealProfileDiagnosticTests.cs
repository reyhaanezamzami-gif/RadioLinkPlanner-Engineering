using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Models;
using RadioLinkPlannervfV.Terrain.Readers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class SrtmRealProfileDiagnosticTests
{
    [Fact]
    public void ReadRealSrtmProfile_Prints100GreatCircleSamples()
    {
        string projectRoot = FindProjectRoot();

        string demDirectory = Path.Combine(
            projectRoot,
            "src",
            "RadioLinkPlannervfV.API",
            "Data",
            "SRTM");

        string filePath = Path.Combine(
            demDirectory,
            "N36E059.hgt");

        Assert.True(
            File.Exists(filePath),
            $"Expected SRTM file was not found: {filePath}");

        var reader = new SrtmDemReader(
            new DemOptions
            {
                DataDirectory = demDirectory
            });

        var start = new GeoCoordinate(
            36.322331526431554,
            59.57538704550899);

        var end = new GeoCoordinate(
            36.32446807551602,
            59.580907558046874);

        Console.WriteLine($"DEM: {filePath}");
        Console.WriteLine($"Size: {new FileInfo(filePath).Length} bytes");
        Console.WriteLine();
        Console.WriteLine("Index | Latitude           | Longitude          | Elevation");
        Console.WriteLine("------------------------------------------------------------");

        for (int index = 0; index < 100; index++)
        {
            double fraction = (double)index / 99.0;

            GeoCoordinate coordinate =
                GreatCircleInterpolator.Interpolate(
                    start,
                    end,
                    fraction);

            TerrainElevationResult result =
                reader.ReadElevation(coordinate);

            Console.WriteLine(
                $"{index,5} | " +
                $"{coordinate.Latitude,18:F12} | " +
                $"{coordinate.Longitude,18:F12} | " +
                $"{result.ElevationMeters,9:F0} m");
        }
    }

    private static string FindProjectRoot()
    {
        DirectoryInfo? directory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionFile = Path.Combine(
                directory.FullName,
                "RadioLinkPlannervfV.slnx");

            if (File.Exists(solutionFile))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate RadioLinkPlannervfV.slnx from the test output directory.");
    }
}