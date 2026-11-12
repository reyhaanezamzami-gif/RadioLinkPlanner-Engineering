using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Generators;
using RadioLinkPlannervfV.Terrain.Models;
using RadioLinkPlannervfV.Terrain.Providers;
using RadioLinkPlannervfV.Terrain.Readers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class OfflineTerrainIntegrationTests
{
    [Fact]
    public async Task GenerateAsync_ReadsElevationFromOfflineSrtmTile()
    {
        string directory =
            CreateTemporaryDirectory();

        try
        {
            string filePath =
                Path.Combine(
                    directory,
                    "N35E051.hgt");

            WriteTestTile(
                filePath,
                new short[]
                {
                    100,
                    200,
                    300,
                    400
                });

            var demReader =
                new SrtmDemReader(
                    new DemOptions
                    {
                        DataDirectory = directory
                    });

            var elevationProvider =
                new OfflineTerrainProvider(
                    demReader);

            var generator =
                new TerrainProfileGenerator(
                    elevationProvider,
                    new HaversineDistanceCalculator());

            var start =
                new GeoCoordinate(
                    35.9999,
                    51.0000);

            var end =
                new GeoCoordinate(
                    35.0000,
                    51.9999);

            TerrainProfileResult result =
                await generator.GenerateAsync(
                    start,
                    end,
                    2);

            Assert.Equal(
                2,
                result.Points.Count);

            Assert.Equal(
                100,
                result.Points[0].ElevationMeters);

            Assert.Equal(
                400,
                result.Points[1].ElevationMeters);
        }
        finally
        {
            Directory.Delete(
                directory,
                true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        string directory =
            Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid()
                    .ToString());

        Directory.CreateDirectory(
            directory);

        return directory;
    }

    private static void WriteTestTile(
        string filePath,
        IEnumerable<short> values)
    {
        using var stream =
            new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write);

        foreach (short value in values)
        {
            byte highByte =
                (byte)(
                    (value >> 8) & 0xFF);

            byte lowByte =
                (byte)(
                    value & 0xFF);

            stream.WriteByte(
                highByte);

            stream.WriteByte(
                lowByte);
        }
    }
}