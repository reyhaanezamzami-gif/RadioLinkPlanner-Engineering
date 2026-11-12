using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IFresnelCalculator
{
    double CalculateFirstZoneRadiusMeters(
        double totalDistanceMeters,
        double distanceFromPointAMeters,
        double frequencyGHz);
}
