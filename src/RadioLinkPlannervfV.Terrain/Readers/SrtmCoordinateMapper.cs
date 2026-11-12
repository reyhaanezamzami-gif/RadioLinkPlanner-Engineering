using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Readers;

public static class SrtmCoordinateMapper
{
    public static (
        int Row,
        int Column)
        GetRowAndColumn(
            GeoCoordinate coordinate,
            int samplesPerSide)
    {
        if (samplesPerSide < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(samplesPerSide));
        }

        double tileSouthLatitude =
            Math.Floor(
                coordinate.Latitude);

        double tileWestLongitude =
            Math.Floor(
                coordinate.Longitude);

        double latitudeFraction =
            coordinate.Latitude -
            tileSouthLatitude;

        double longitudeFraction =
            coordinate.Longitude -
            tileWestLongitude;

        double positionScale =
            samplesPerSide - 1;

        int row =
            (int)Math.Round(
                (1.0 - latitudeFraction) *
                positionScale);

        int column =
            (int)Math.Round(
                longitudeFraction *
                positionScale);

        row =
            Math.Clamp(
                row,
                0,
                samplesPerSide - 1);

        column =
            Math.Clamp(
                column,
                0,
                samplesPerSide - 1);

        return (
            row,
            column);
    }
}
