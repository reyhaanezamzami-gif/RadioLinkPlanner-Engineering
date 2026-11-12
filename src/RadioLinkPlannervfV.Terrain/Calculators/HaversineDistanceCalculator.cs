using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Calculators;

public sealed class HaversineDistanceCalculator : IDistanceCalculator
{
    private const double EarthRadiusMeters = 6371008.8;

    public double CalculateDistanceMeters(GeoCoordinate pointA, GeoCoordinate pointB)
    {
        double lat1 = DegreesToRadians(pointA.Latitude);
        double lat2 = DegreesToRadians(pointB.Latitude);
        double deltaLat = DegreesToRadians(pointB.Latitude - pointA.Latitude);
        double deltaLon = DegreesToRadians(pointB.Longitude - pointA.Longitude);

        double a = Math.Sin(deltaLat / 2.0) * Math.Sin(deltaLat / 2.0) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(deltaLon / 2.0) * Math.Sin(deltaLon / 2.0);

        double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180.0;
}
