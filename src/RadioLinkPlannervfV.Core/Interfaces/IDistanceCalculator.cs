using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IDistanceCalculator
{
    double CalculateDistanceMeters(GeoCoordinate pointA, GeoCoordinate pointB);
}
