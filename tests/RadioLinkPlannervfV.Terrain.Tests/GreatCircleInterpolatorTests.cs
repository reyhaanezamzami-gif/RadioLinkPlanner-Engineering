using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Calculators;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class GreatCircleInterpolatorTests
{
    [Fact]
    public void Interpolate_ReturnsStart_WhenFractionIsZero()
    {
        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        GeoCoordinate result =
            GreatCircleInterpolator.Interpolate(
                start,
                end,
                0.0);

        Assert.Equal(
            start.Latitude,
            result.Latitude,
            10);

        Assert.Equal(
            start.Longitude,
            result.Longitude,
            10);
    }

    [Fact]
    public void Interpolate_ReturnsEnd_WhenFractionIsOne()
    {
        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        GeoCoordinate result =
            GreatCircleInterpolator.Interpolate(
                start,
                end,
                1.0);

        Assert.Equal(
            end.Latitude,
            result.Latitude,
            10);

        Assert.Equal(
            end.Longitude,
            result.Longitude,
            10);
    }

    [Fact]
    public void Interpolate_ReturnsPointBetweenStartAndEnd()
    {
        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        GeoCoordinate result =
            GreatCircleInterpolator.Interpolate(
                start,
                end,
                0.5);

        Assert.InRange(
            result.Latitude,
            Math.Min(
                start.Latitude,
                end.Latitude),
            Math.Max(
                start.Latitude,
                end.Latitude));

        Assert.InRange(
            result.Longitude,
            Math.Min(
                start.Longitude,
                end.Longitude),
            Math.Max(
                start.Longitude,
                end.Longitude));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Interpolate_Throws_WhenFractionIsOutsideRange(
        double fraction)
    {
        var start =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var end =
            new GeoCoordinate(
                35.7000,
                51.4500);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                GreatCircleInterpolator.Interpolate(
                    start,
                    end,
                    fraction));
    }

    [Fact]
    public void Interpolate_ReturnsStart_WhenCoordinatesAreEqual()
    {
        var coordinate =
            new GeoCoordinate(
                35.6892,
                51.3890);

        GeoCoordinate result =
            GreatCircleInterpolator.Interpolate(
                coordinate,
                coordinate,
                0.5);

        Assert.Equal(
            coordinate.Latitude,
            result.Latitude,
            10);

        Assert.Equal(
            coordinate.Longitude,
            result.Longitude,
            10);
    }
}
