using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Readers;

public static class SrtmFileNameResolver
{
    public static string GetFileName(
        GeoCoordinate coordinate)
    {
        int latitudeDegrees =
            (int)Math.Floor(
                coordinate.Latitude);

        int longitudeDegrees =
            (int)Math.Floor(
                coordinate.Longitude);

        string latitudePrefix =
            latitudeDegrees >= 0
                ? "N"
                : "S";

        string longitudePrefix =
            longitudeDegrees >= 0
                ? "E"
                : "W";

        int absoluteLatitude =
            Math.Abs(latitudeDegrees);

        int absoluteLongitude =
            Math.Abs(longitudeDegrees);

        return
            $"{latitudePrefix}{absoluteLatitude:D2}" +
            $"{longitudePrefix}{absoluteLongitude:D3}.hgt";
    }
}
