using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Calculators;

public static class GreatCircleInterpolator
{
    private const double DegreesToRadians =
        Math.PI / 180.0;

    private const double RadiansToDegrees =
        180.0 / Math.PI;

    public static GeoCoordinate Interpolate(
        GeoCoordinate start,
        GeoCoordinate end,
        double fraction)
    {
        if (fraction < 0.0 || fraction > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fraction),
                "Fraction must be between 0 and 1.");
        }

        if (fraction == 0.0)
        {
            return start;
        }

        if (fraction == 1.0)
        {
            return end;
        }

        double startLatitudeRadians =
            start.Latitude * DegreesToRadians;

        double startLongitudeRadians =
            start.Longitude * DegreesToRadians;

        double endLatitudeRadians =
            end.Latitude * DegreesToRadians;

        double endLongitudeRadians =
            end.Longitude * DegreesToRadians;

        double angularDistance =
            CalculateAngularDistance(
                startLatitudeRadians,
                startLongitudeRadians,
                endLatitudeRadians,
                endLongitudeRadians);

        if (angularDistance == 0.0)
        {
            return start;
        }

        double sinAngularDistance =
            Math.Sin(angularDistance);

        double startWeight =
            Math.Sin(
                (1.0 - fraction) *
                angularDistance) /
            sinAngularDistance;

        double endWeight =
            Math.Sin(
                fraction *
                angularDistance) /
            sinAngularDistance;

        double x =
            startWeight *
            Math.Cos(startLatitudeRadians) *
            Math.Cos(startLongitudeRadians) +
            endWeight *
            Math.Cos(endLatitudeRadians) *
            Math.Cos(endLongitudeRadians);

        double y =
            startWeight *
            Math.Cos(startLatitudeRadians) *
            Math.Sin(startLongitudeRadians) +
            endWeight *
            Math.Cos(endLatitudeRadians) *
            Math.Sin(endLongitudeRadians);

        double z =
            startWeight *
            Math.Sin(startLatitudeRadians) +
            endWeight *
            Math.Sin(endLatitudeRadians);

        double latitudeRadians =
            Math.Atan2(
                z,
                Math.Sqrt(
                    x * x +
                    y * y));

        double longitudeRadians =
            Math.Atan2(
                y,
                x);

        return new GeoCoordinate(
            latitudeRadians *
            RadiansToDegrees,

            longitudeRadians *
            RadiansToDegrees);
    }

    private static double CalculateAngularDistance(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        double cosine =
            Math.Sin(latitude1) *
            Math.Sin(latitude2) +

            Math.Cos(latitude1) *
            Math.Cos(latitude2) *
            Math.Cos(
                longitude2 -
                longitude1);

        cosine =
            Math.Clamp(
                cosine,
                -1.0,
                1.0);

        return Math.Acos(
            cosine);
    }
}
