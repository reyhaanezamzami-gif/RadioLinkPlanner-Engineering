using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Readers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class SrtmFileNameResolverTests
{
    [Theory]
    [InlineData(35.6892, 51.3890, "N35E051.hgt")]
    [InlineData(-35.2000, 51.3000, "S36E051.hgt")]
    [InlineData(35.2000, -51.3000, "N35W052.hgt")]
    [InlineData(-35.2000, -51.3000, "S36W052.hgt")]
    public void GetFileName_ReturnsCorrectSrtmTileName(
        double latitude,
        double longitude,
        string expected)
    {
        var coordinate =
            new GeoCoordinate(
                latitude,
                longitude);

        string result =
            SrtmFileNameResolver.GetFileName(
                coordinate);

        Assert.Equal(
            expected,
            result);
    }
}
