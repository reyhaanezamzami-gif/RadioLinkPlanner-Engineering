using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Readers;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class SrtmCoordinateMapperTests
{
    [Theory]
    [InlineData(35.9999, 51.0000, 1201, 0, 0)]
    [InlineData(35.0000, 51.0000, 1201, 1200, 0)]
    [InlineData(35.9999, 51.9999, 1201, 0, 1200)]
    [InlineData(35.0000, 51.9999, 1201, 1200, 1200)]
    [InlineData(35.5000, 51.5000, 1201, 600, 600)]
    public void GetRowAndColumn_ReturnsExpectedPosition(
        double latitude,
        double longitude,
        int samplesPerSide,
        int expectedRow,
        int expectedColumn)
    {
        var coordinate =
            new GeoCoordinate(
                latitude,
                longitude);

        var result =
            SrtmCoordinateMapper.GetRowAndColumn(
                coordinate,
                samplesPerSide);

        Assert.Equal(
            expectedRow,
            result.Row);

        Assert.Equal(
            expectedColumn,
            result.Column);
    }

    [Fact]
    public void GetRowAndColumn_ThrowsWhenSamplesPerSideIsLessThanTwo()
    {
        var coordinate =
            new GeoCoordinate(
                35.5000,
                51.5000);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                SrtmCoordinateMapper.GetRowAndColumn(
                    coordinate,
                    1));
    }
}
