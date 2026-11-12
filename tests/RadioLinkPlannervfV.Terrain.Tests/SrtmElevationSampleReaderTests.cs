using RadioLinkPlannervfV.Terrain.Readers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class SrtmElevationSampleReaderTests
{
    [Theory]
    [InlineData(0x03, 0xE8, 1000)]
    [InlineData(0xFF, 0x9C, -100)]
    [InlineData(0x80, 0x00, -32768)]
    public void ReadSample_ReadsBigEndianSignedValue(
        byte highByte,
        byte lowByte,
        short expected)
    {
        using var stream =
            new MemoryStream(
                new byte[]
                {
                    highByte,
                    lowByte
                });

        short result =
            SrtmElevationSampleReader.ReadSample(
                stream,
                0);

        Assert.Equal(
            expected,
            result);
    }

    [Fact]
    public void ReadSample_ReadsCorrectValueAtOffset()
    {
        using var stream =
            new MemoryStream(
                new byte[]
                {
                    0x00,
                    0x01,
                    0x03,
                    0xE8
                });

        short result =
            SrtmElevationSampleReader.ReadSample(
                stream,
                2);

        Assert.Equal(
            (short)1000,
            result);
    }

    [Fact]
    public void ReadSample_ThrowsWhenOffsetIsNegative()
    {
        using var stream =
            new MemoryStream(
                new byte[]
                {
                    0x00,
                    0x01
                });

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                SrtmElevationSampleReader.ReadSample(
                    stream,
                    -1));
    }

    [Fact]
    public void ReadSample_ThrowsWhenSampleDoesNotExist()
    {
        using var stream =
            new MemoryStream(
                new byte[]
                {
                    0x00
                });

        Assert.Throws<EndOfStreamException>(
            () =>
                SrtmElevationSampleReader.ReadSample(
                    stream,
                    0));
    }
}
