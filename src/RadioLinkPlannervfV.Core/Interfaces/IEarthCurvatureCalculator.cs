namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IEarthCurvatureCalculator
{
    double CalculateCorrectionMeters(
        double distanceMeters,
        double kFactor);
}