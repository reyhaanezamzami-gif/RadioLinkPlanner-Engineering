using RadioLinkPlannervfV.Terrain.Readers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class SrtmTileMetadataTests
{
    [Theory]
    [InlineData(1201)]
    [InlineData(3601)]
    public void GetSamplesPerSide_ReturnsCorrectValue(
        int samplesPerSide)
    {
        long fileLengthBytes =
            (long)samplesPerSide *
            samplesPerSide *
            SrtmTileMetadata.BytesPerSample;

        int result =
            SrtmTileMetadata.GetSamplesPerSide(
                fileLengthBytes);

        Assert.Equal(
            samplesPerSide,
            result);
    }

    [Fact]
    public void GetSamplesPerSide_InvalidFileSize_ThrowsException()
    {
        Assert.Throws<InvalidDataException>(
            () =>
                SrtmTileMetadata.GetSamplesPerSide(
                    12345));
    }
}
