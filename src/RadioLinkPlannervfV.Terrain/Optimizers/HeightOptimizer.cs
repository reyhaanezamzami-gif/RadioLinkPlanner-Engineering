using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Optimizers;

public sealed class HeightOptimizer : IHeightOptimizer
{
    private const int BinarySearchIterations = 60;

    private const double MaximumAdditionalHeightMeters = 10000.0;

    private const double ConstraintToleranceMeters = 1e-9;

    private readonly IDistanceCalculator _distanceCalculator;
    private readonly IFresnelCalculator _fresnelCalculator;
    private readonly IEarthCurvatureCalculator _earthCurvatureCalculator;
    private readonly PropagationSettings _propagationSettings;

    public HeightOptimizer(
        IDistanceCalculator distanceCalculator,
        IFresnelCalculator fresnelCalculator,
        IEarthCurvatureCalculator earthCurvatureCalculator,
        PropagationSettings propagationSettings)
    {
        _distanceCalculator =
            distanceCalculator ??
            throw new ArgumentNullException(
                nameof(distanceCalculator));

        _fresnelCalculator =
            fresnelCalculator ??
            throw new ArgumentNullException(
                nameof(fresnelCalculator));

        _earthCurvatureCalculator =
            earthCurvatureCalculator ??
            throw new ArgumentNullException(
                nameof(earthCurvatureCalculator));

        _propagationSettings =
            propagationSettings ??
            throw new ArgumentNullException(
                nameof(propagationSettings));
    }

    public HeightOptimizationResult Optimize(
        Link link,
        IReadOnlyList<TerrainPoint> terrainProfile,
        double requiredClearanceMeters)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(terrainProfile);

        if (terrainProfile.Count == 0)
        {
            throw new ArgumentException(
                "Terrain profile cannot be empty.",
                nameof(terrainProfile));
        }

        if (requiredClearanceMeters < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requiredClearanceMeters),
                "Required clearance cannot be negative.");
        }

        double distanceMeters =
            _distanceCalculator.CalculateDistanceMeters(
                link.SiteA.Coordinate,
                link.SiteB.Coordinate);

        double originalHeightA =
            link.SiteA.AntennaHeightMeters;

        double originalHeightB =
            link.SiteB.AntennaHeightMeters;

        if (distanceMeters <= 0)
        {
            return new HeightOptimizationResult(
                originalHeightA,
                originalHeightB,
                0,
                true);
        }

        double earthCurvatureCorrection =
            _earthCurvatureCalculator.CalculateCorrectionMeters(
                distanceMeters,
                _propagationSettings.KFactor);

        if (HasRequiredClearance(
            link,
            terrainProfile,
            distanceMeters,
            earthCurvatureCorrection,
            originalHeightA,
            originalHeightB))
        {
            return new HeightOptimizationResult(
                originalHeightA,
                originalHeightB,
                0,
                true);
        }

        double minimumAdditionalHeight =
            CalculateMinimumAdditionalHeight(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection);

        if (double.IsPositiveInfinity(
            minimumAdditionalHeight))
        {
            return new HeightOptimizationResult(
                originalHeightA,
                originalHeightB,
                double.PositiveInfinity,
                false);
        }

        (double additionalA, double additionalB) =
            FindFeasibleHeightDistribution(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                minimumAdditionalHeight);

        double optimizedHeightA =
            originalHeightA +
            additionalA;

        double optimizedHeightB =
            originalHeightB +
            additionalB;

        bool isClear =
            HasRequiredClearance(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                optimizedHeightA,
                optimizedHeightB);

        return new HeightOptimizationResult(
            optimizedHeightA,
            optimizedHeightB,
            minimumAdditionalHeight,
            isClear);
    }

    private double CalculateMinimumAdditionalHeight(
        Link link,
        IReadOnlyList<TerrainPoint> terrainProfile,
        double distanceMeters,
        double earthCurvatureCorrection)
    {
        /*
         * First check whether the link is already feasible
         * without any additional height.
         */
        if (TryGetFeasibleHeightDistribution(
            link,
            terrainProfile,
            distanceMeters,
            earthCurvatureCorrection,
            0,
            out _,
            out _))
        {
            return 0;
        }

        /*
         * The optimizer has a hard upper bound of 10,000 meters
         * of total additional antenna height.
         *
         * We test the upper bound directly instead of using
         * exponential doubling. This avoids the classic:
         *
         *     8192 -> 16384
         *
         * overshoot problem.
         */
        if (!TryGetFeasibleHeightDistribution(
            link,
            terrainProfile,
            distanceMeters,
            earthCurvatureCorrection,
            MaximumAdditionalHeightMeters,
            out _,
            out _))
        {
            return double.PositiveInfinity;
        }

        /*
         * At this point:
         *
         *     0 m        = infeasible
         *     10000 m    = feasible
         *
         * Therefore binary search can safely find the minimum
         * feasible total additional height.
         */
        double low = 0.0;

        double high =
            MaximumAdditionalHeightMeters;

        for (
            int iteration = 0;
            iteration < BinarySearchIterations;
            iteration++)
        {
            double middle =
                low +
                (high - low) / 2.0;

            if (TryGetFeasibleHeightDistribution(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                middle,
                out _,
                out _))
            {
                high = middle;
            }
            else
            {
                low = middle;
            }
        }

        return high;
    }

    private (double AdditionalA, double AdditionalB)
        FindFeasibleHeightDistribution(
            Link link,
            IReadOnlyList<TerrainPoint> terrainProfile,
            double distanceMeters,
            double earthCurvatureCorrection,
            double totalAdditionalHeight)
    {
        bool feasible =
            TryGetFeasibleHeightDistribution(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                totalAdditionalHeight,
                out double minimumAdditionalA,
                out double maximumAdditionalA);

        if (!feasible)
        {
            return (
                double.NaN,
                double.NaN);
        }

        double additionalA =
            Math.Clamp(
                minimumAdditionalA,
                0,
                totalAdditionalHeight);

        double additionalB =
            totalAdditionalHeight -
            additionalA;

        if (additionalA > maximumAdditionalA)
        {
            additionalA =
                maximumAdditionalA;

            additionalB =
                totalAdditionalHeight -
                additionalA;
        }

        return (
            additionalA,
            additionalB);
    }

    private bool TryGetFeasibleHeightDistribution(
        Link link,
        IReadOnlyList<TerrainPoint> terrainProfile,
        double distanceMeters,
        double earthCurvatureCorrection,
        double totalAdditionalHeight,
        out double minimumAdditionalA,
        out double maximumAdditionalA)
    {
        minimumAdditionalA = 0;
        maximumAdditionalA = totalAdditionalHeight;

        double siteAElevation =
            link.SiteA.GroundElevationMeters +
            link.SiteA.AntennaHeightMeters;

        double siteBElevation =
            link.SiteB.GroundElevationMeters +
            link.SiteB.AntennaHeightMeters;

        foreach (TerrainPoint point in terrainProfile)
        {
            double fraction =
                Math.Clamp(
                    point.DistanceFromStartMeters /
                    distanceMeters,
                    0,
                    1);

            double lineOfSightElevation =
                siteAElevation +
                (
                    siteBElevation -
                    siteAElevation
                ) *
                fraction;

            double curvature =
                earthCurvatureCorrection *
                4 *
                fraction *
                (1 - fraction);

            double fresnelRadiusMeters =
                _fresnelCalculator
                    .CalculateFirstZoneRadiusMeters(
                        distanceMeters,
                        point.DistanceFromStartMeters,
                        link.FrequencyGHz);

            double requiredFresnelClearance =
                fresnelRadiusMeters *
                _propagationSettings.FresnelClearanceFactor;

            double requiredElevation =
                point.ElevationMeters +
                requiredFresnelClearance +
                curvature;

            double deficit =
                requiredElevation -
                lineOfSightElevation;

            if (deficit <= ConstraintToleranceMeters)
            {
                continue;
            }

            double coefficientA =
                1 - fraction;

            double coefficientB =
                fraction;

            double denominator =
                coefficientA -
                coefficientB;

            double rightHandSide =
                deficit -
                coefficientB *
                totalAdditionalHeight;

            /*
             * At the midpoint:
             *
             *     coefficientA = coefficientB = 0.5
             *
             * Therefore the distribution between A and B does
             * not matter. Only the total additional height matters.
             */
            if (Math.Abs(denominator) <=
                ConstraintToleranceMeters)
            {
                if (rightHandSide >
                    ConstraintToleranceMeters)
                {
                    return false;
                }

                continue;
            }

            double requiredA =
                rightHandSide /
                denominator;

            if (denominator > 0)
            {
                minimumAdditionalA =
                    Math.Max(
                        minimumAdditionalA,
                        requiredA);
            }
            else
            {
                maximumAdditionalA =
                    Math.Min(
                        maximumAdditionalA,
                        requiredA);
            }

            if (minimumAdditionalA >
                maximumAdditionalA +
                ConstraintToleranceMeters)
            {
                return false;
            }
        }

        minimumAdditionalA =
            Math.Max(
                minimumAdditionalA,
                0);

        maximumAdditionalA =
            Math.Min(
                maximumAdditionalA,
                totalAdditionalHeight);

        return
            minimumAdditionalA <=
            maximumAdditionalA +
            ConstraintToleranceMeters;
    }

    private bool HasRequiredClearance(
        Link link,
        IReadOnlyList<TerrainPoint> terrainProfile,
        double distanceMeters,
        double earthCurvatureCorrection,
        double antennaHeightA,
        double antennaHeightB)
    {
        double siteAElevation =
            link.SiteA.GroundElevationMeters +
            antennaHeightA;

        double siteBElevation =
            link.SiteB.GroundElevationMeters +
            antennaHeightB;

        foreach (TerrainPoint point in terrainProfile)
        {
            double fraction =
                Math.Clamp(
                    point.DistanceFromStartMeters /
                    distanceMeters,
                    0,
                    1);

            double lineOfSightElevation =
                siteAElevation +
                (
                    siteBElevation -
                    siteAElevation
                ) *
                fraction;

            double curvature =
                earthCurvatureCorrection *
                4 *
                fraction *
                (1 - fraction);

            double fresnelRadiusMeters =
                _fresnelCalculator
                    .CalculateFirstZoneRadiusMeters(
                        distanceMeters,
                        point.DistanceFromStartMeters,
                        link.FrequencyGHz);

            double requiredFresnelClearance =
                fresnelRadiusMeters *
                _propagationSettings.FresnelClearanceFactor;

            double requiredElevation =
                point.ElevationMeters +
                requiredFresnelClearance +
                curvature;

            if (lineOfSightElevation <
                requiredElevation -
                ConstraintToleranceMeters)
            {
                return false;
            }
        }

        return true;
    }
}