using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Services;

public sealed class EarthCurvatureCalculator : IEarthCurvatureCalculator
{
    private readonly PropagationSettings _propagationSettings;

    public EarthCurvatureCalculator(
        PropagationSettings propagationSettings)
    {
        _propagationSettings =
            propagationSettings ??
            throw new ArgumentNullException(
                nameof(propagationSettings));
    }

    public double CalculateCorrectionMeters(
        double distanceMeters,
        double kFactor)
    {
        if (distanceMeters <= 0)
        {
            return 0;
        }

        if (kFactor <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kFactor),
                "K factor must be greater than zero.");
        }

        if (_propagationSettings.EarthRadiusMeters <= 0)
        {
            throw new InvalidOperationException(
                "Earth radius must be greater than zero.");
        }

        double effectiveEarthRadius =
            _propagationSettings.EarthRadiusMeters *
            kFactor;

        double correction =
            Math.Pow(distanceMeters, 2) /
            (8 * effectiveEarthRadius);

        return correction;
    }
}