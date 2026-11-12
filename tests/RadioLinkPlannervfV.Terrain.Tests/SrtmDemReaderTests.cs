using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Models;
using RadioLinkPlannervfV.Terrain.Readers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class SrtmDemReaderTests
{
    [Fact]
    public void ReadElevation_ReadsExpectedElevation()
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

            var reader =
                new SrtmDemReader(
                    new DemOptions
                    {
                        DataDirectory = directory
                    });

            var coordinate =
                new GeoCoordinate(
                    35.9999,
                    51.0000);

            TerrainElevationResult result =
                reader.ReadElevation(
                    coordinate);

            Assert.Equal(
                100,
                result.ElevationMeters);

            Assert.Equal(
                "SRTM",
                result.Source);
        }
        finally
        {
            Directory.Delete(
                directory,
                true);
        }
    }

    [Fact]
    public void ReadElevation_ThrowsWhenFileDoesNotExist()
    {
        string directory =
            CreateTemporaryDirectory();

        try
        {
            var reader =
                new SrtmDemReader(
                    new DemOptions
                    {
                        DataDirectory = directory
                    });

            var coordinate =
                new GeoCoordinate(
                    35.5000,
                    51.5000);

            Assert.Throws<FileNotFoundException>(
                () =>
                    reader.ReadElevation(
                        coordinate));
        }
        finally
        {
            Directory.Delete(
                directory,
                true);
        }
    }

    [Fact]
    public void ReadElevation_ThrowsWhenElevationIsVoid()
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
                    -32768,
                    200,
                    300,
                    400
                });

            var reader =
                new SrtmDemReader(
                    new DemOptions
                    {
                        DataDirectory = directory
                    });

            var coordinate =
                new GeoCoordinate(
                    35.9999,
                    51.0000);

            Assert.Throws<InvalidDataException>(
                () =>
                    reader.ReadElevation(
                        coordinate));
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
                (byte)((value >> 8) & 0xFF);

            byte lowByte =
                (byte)(value & 0xFF);

            stream.WriteByte(
                highByte);

            stream.WriteByte(
                lowByte);
        }
    }
}
