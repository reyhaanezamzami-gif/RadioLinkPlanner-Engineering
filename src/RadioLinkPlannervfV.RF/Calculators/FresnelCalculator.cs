using RadioLinkPlannervfV.Core.Interfaces;

namespace RadioLinkPlannervfV.RF.Calculators;

public sealed class FresnelCalculator : IFresnelCalculator
{
    public double CalculateFirstZoneRadiusMeters(
        double totalDistanceMeters,
        double distanceFromPointAMeters,
        double frequencyGHz)
    {
        if (totalDistanceMeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalDistanceMeters));

        if (distanceFromPointAMeters < 0 || distanceFromPointAMeters > totalDistanceMeters)
            throw new ArgumentOutOfRangeException(nameof(distanceFromPointAMeters));

        if (frequencyGHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(frequencyGHz));

        const double SpeedOfLightMetersPerSecond = 299792458.0;
        double frequencyHz = frequencyGHz * 1_000_000_000.0;
        double wavelengthMeters = SpeedOfLightMetersPerSecond / frequencyHz;
        double distanceB = totalDistanceMeters - distanceFromPointAMeters;

        return Math.Sqrt(
            wavelengthMeters * distanceFromPointAMeters * distanceB /
            totalDistanceMeters);
    }
}
