using RadioLinkPlannervfV.Core.Enums;
using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Services;

public sealed class LinkAnalyzer : ILinkAnalyzer
{
    private const double SpeedOfLightMetersPerSecond = 299792458.0;
    private const int BinarySearchIterations = 60;
    private const double MaximumRequiredHeightMeters = 10000.0;

    private readonly IDistanceCalculator _distanceCalculator;
    private readonly IFresnelCalculator _fresnelCalculator;
    private readonly IHeightOptimizer _heightOptimizer;
    private readonly IEarthCurvatureCalculator _earthCurvatureCalculator;
    private readonly PropagationSettings _propagationSettings;

    public LinkAnalyzer(
        IDistanceCalculator distanceCalculator,
        IFresnelCalculator fresnelCalculator,
        IHeightOptimizer heightOptimizer,
        IEarthCurvatureCalculator earthCurvatureCalculator,
        PropagationSettings propagationSettings)
    {
        _distanceCalculator =
            distanceCalculator ??
            throw new ArgumentNullException(nameof(distanceCalculator));

        _fresnelCalculator =
            fresnelCalculator ??
            throw new ArgumentNullException(nameof(fresnelCalculator));

        _heightOptimizer =
            heightOptimizer ??
            throw new ArgumentNullException(nameof(heightOptimizer));

        _earthCurvatureCalculator =
            earthCurvatureCalculator ??
            throw new ArgumentNullException(nameof(earthCurvatureCalculator));

        _propagationSettings =
            propagationSettings ??
            throw new ArgumentNullException(nameof(propagationSettings));
    }

    public LinkAnalysisResult Analyze(
        Link link,
        IReadOnlyList<TerrainPoint> terrainProfile)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(terrainProfile);

        if (terrainProfile.Count == 0)
        {
            throw new ArgumentException(
                "Terrain profile cannot be empty.",
                nameof(terrainProfile));
        }

        double distanceMeters =
            _distanceCalculator.CalculateDistanceMeters(
                link.SiteA.Coordinate,
                link.SiteB.Coordinate);

        if (distanceMeters <= 0)
        {
            throw new ArgumentException(
                "Link endpoints must be different.",
                nameof(link));
        }

        double frequencyHz =
            link.FrequencyGHz *
            1_000_000_000.0;

        double wavelengthMeters =
            SpeedOfLightMetersPerSecond /
            frequencyHz;

        double freeSpacePathLossDb =
            20 *
            Math.Log10(
                4 *
                Math.PI *
                distanceMeters /
                wavelengthMeters);

        double fresnelRadiusMeters =
            _fresnelCalculator.CalculateFirstZoneRadiusMeters(
                distanceMeters,
                distanceMeters / 2,
                link.FrequencyGHz);

        double requiredClearanceMeters =
            fresnelRadiusMeters *
            _propagationSettings.FresnelClearanceFactor;

        double earthCurvatureCorrection =
            _earthCurvatureCalculator.CalculateCorrectionMeters(
                distanceMeters,
                _propagationSettings.KFactor);

        double maximumTerrainElevation =
            terrainProfile.Max(
                x => x.ElevationMeters);

        double terrainObstructionDeficit =
            CalculateTerrainDeficit(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                link.SiteA.AntennaHeightMeters,
                link.SiteB.AntennaHeightMeters);

        LinkObstruction? worstTerrainObstruction =
            FindWorstObstruction(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                link.SiteA.AntennaHeightMeters,
                link.SiteB.AntennaHeightMeters);

        double fresnelDeficit =
            CalculateFresnelDeficit(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                link.SiteA.AntennaHeightMeters,
                link.SiteB.AntennaHeightMeters);

        LinkStatus status =
            terrainObstructionDeficit > 0
                ? LinkStatus.Obstructed
                : fresnelDeficit > 0
                    ? LinkStatus.FresnelViolation
                    : LinkStatus.LineOfSight;

        double maximumDeficit =
            Math.Max(
                terrainObstructionDeficit,
                fresnelDeficit);

        double recommendedAHeight =
            CalculateRequiredHeight(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                true);

        double recommendedBHeight =
            CalculateRequiredHeight(
                link,
                terrainProfile,
                distanceMeters,
                earthCurvatureCorrection,
                false);

        HeightOptimizationResult optimization =
            _heightOptimizer.Optimize(
                link,
                terrainProfile,
                requiredClearanceMeters);

        double minimumHeightASL =
            Math.Min(
                link.SiteA.GroundElevationMeters +
                optimization.OptimizedAntennaHeightASiteAMeters,

                link.SiteB.GroundElevationMeters +
                optimization.OptimizedAntennaHeightBSiteBMeters);

        IReadOnlyList<TerrainProfileAnalysisPoint>
            terrainProfileAnalysis =
                CalculateTerrainProfileAnalysis(
                    link,
                    terrainProfile,
                    distanceMeters,
                    earthCurvatureCorrection,
                    link.SiteA.AntennaHeightMeters,
                    link.SiteB.AntennaHeightMeters);

        return new LinkAnalysisResult(
            distanceMeters,
            freeSpacePathLossDb,
            fresnelRadiusMeters,
            requiredClearanceMeters,
            minimumHeightASL,
            recommendedAHeight,
            recommendedBHeight,
            maximumTerrainElevation,
            maximumDeficit,
            status,
            optimization,
            worstTerrainObstruction,
            link.SiteA.GroundElevationMeters,
            link.SiteB.GroundElevationMeters,
            terrainProfileAnalysis);
    }

    private IReadOnlyList<TerrainProfileAnalysisPoint>
        CalculateTerrainProfileAnalysis(
            Link link,
            IReadOnlyList<TerrainPoint> profile,
            double distance,
            double curvature,
            double heightA,
            double heightB)
    {
        double elevationA =
            link.SiteA.GroundElevationMeters +
            heightA;

        double elevationB =
            link.SiteB.GroundElevationMeters +
            heightB;

        var result =
            new List<TerrainProfileAnalysisPoint>(
                profile.Count);

        foreach (TerrainPoint point in profile)
        {
            double ratio =
                point.DistanceFromStartMeters /
                distance;

            double lineOfSightElevation =
                elevationA +
                (elevationB - elevationA) *
                ratio;

            double earthCurvatureElevation =
                curvature *
                4 *
                ratio *
                (1 - ratio);

            double terrainElevationWithCurvature =
                point.ElevationMeters +
                earthCurvatureElevation;

            double firstFresnelZoneRadius =
                _fresnelCalculator.CalculateFirstZoneRadiusMeters(
                    distance,
                    point.DistanceFromStartMeters,
                    link.FrequencyGHz);

            double requiredFresnelClearance =
                firstFresnelZoneRadius *
                _propagationSettings.FresnelClearanceFactor;

            double actualClearance =
                lineOfSightElevation -
                terrainElevationWithCurvature;

            double clearanceDeficit =
                requiredFresnelClearance -
                actualClearance;

            result.Add(
                new TerrainProfileAnalysisPoint(
                    point.Coordinate,
                    point.DistanceFromStartMeters,
                    point.ElevationMeters,
                    terrainElevationWithCurvature,
                    lineOfSightElevation,
                    firstFresnelZoneRadius,
                    requiredFresnelClearance,
                    actualClearance,
                    clearanceDeficit));
        }

        return result;
    }

    private static double CalculateTerrainDeficit(
        Link link,
        IReadOnlyList<TerrainPoint> profile,
        double distance,
        double curvature,
        double heightA,
        double heightB)
    {
        double max = 0;

        double elevationA =
            link.SiteA.GroundElevationMeters +
            heightA;

        double elevationB =
            link.SiteB.GroundElevationMeters +
            heightB;

        foreach (TerrainPoint point in profile)
        {
            double ratio =
                point.DistanceFromStartMeters /
                distance;

            double los =
                elevationA +
                (elevationB - elevationA) *
                ratio;

            double earth =
                curvature *
                4 *
                ratio *
                (1 - ratio);

            double deficit =
                point.ElevationMeters +
                earth -
                los;

            if (deficit > max)
            {
                max = deficit;
            }
        }

        return max;
    }

    private static LinkObstruction? FindWorstObstruction(
        Link link,
        IReadOnlyList<TerrainPoint> profile,
        double distance,
        double curvature,
        double heightA,
        double heightB)
    {
        LinkObstruction? result = null;

        double elevationA =
            link.SiteA.GroundElevationMeters +
            heightA;

        double elevationB =
            link.SiteB.GroundElevationMeters +
            heightB;

        foreach (TerrainPoint point in profile)
        {
            double ratio =
                point.DistanceFromStartMeters /
                distance;

            double los =
                elevationA +
                (elevationB - elevationA) *
                ratio;

            double earth =
                curvature *
                4 *
                ratio *
                (1 - ratio);

            double actual =
                point.ElevationMeters +
                earth;

            double obstruction =
                actual -
                los;

            if (obstruction <= 0)
            {
                continue;
            }

            if (result == null ||
                obstruction >
                result.ObstructionAmountMeters)
            {
                result =
                    new LinkObstruction(
                        point.Coordinate,
                        point.DistanceFromStartMeters,
                        point.ElevationMeters,
                        actual,
                        los,
                        obstruction);
            }
        }

        return result;
    }

    private double CalculateFresnelDeficit(
        Link link,
        IReadOnlyList<TerrainPoint> profile,
        double distance,
        double curvature,
        double heightA,
        double heightB)
    {
        double max = 0;

        double elevationA =
            link.SiteA.GroundElevationMeters +
            heightA;

        double elevationB =
            link.SiteB.GroundElevationMeters +
            heightB;

        foreach (TerrainPoint point in profile)
        {
            double ratio =
                point.DistanceFromStartMeters /
                distance;

            double los =
                elevationA +
                (elevationB - elevationA) *
                ratio;

            double earth =
                curvature *
                4 *
                ratio *
                (1 - ratio);

            double terrain =
                point.ElevationMeters +
                earth;

            if (terrain > los)
            {
                continue;
            }

            double radius =
                _fresnelCalculator.CalculateFirstZoneRadiusMeters(
                    distance,
                    point.DistanceFromStartMeters,
                    link.FrequencyGHz);

            double required =
                radius *
                _propagationSettings.FresnelClearanceFactor;

            double deficit =
                terrain +
                required -
                los;

            if (deficit > max)
            {
                max = deficit;
            }
        }

        return max;
    }

    private double CalculateRequiredHeight(
        Link link,
        IReadOnlyList<TerrainPoint> profile,
        double distance,
        double curvature,
        bool siteA)
    {
        double current =
            siteA
                ? link.SiteA.AntennaHeightMeters
                : link.SiteB.AntennaHeightMeters;

        double other =
            siteA
                ? link.SiteB.AntennaHeightMeters
                : link.SiteA.AntennaHeightMeters;

        double low = current;

        if (low >= MaximumRequiredHeightMeters)
        {
            double currentDeficit =
                CalculateCombinedDeficit(
                    link,
                    profile,
                    distance,
                    curvature,
                    siteA ? low : other,
                    siteA ? other : low);

            return currentDeficit <= 0
                ? low
                : double.PositiveInfinity;
        }

        double high =
            Math.Min(
                Math.Max(current + 1, 1),
                MaximumRequiredHeightMeters);

        while (
            high < MaximumRequiredHeightMeters &&
            CalculateCombinedDeficit(
                link,
                profile,
                distance,
                curvature,
                siteA ? high : other,
                siteA ? other : high) > 0)
        {
            double nextHigh =
                Math.Min(
                    high * 2,
                    MaximumRequiredHeightMeters);

            if (nextHigh <= high)
            {
                break;
            }

            high = nextHigh;
        }

        double highDeficit =
            CalculateCombinedDeficit(
                link,
                profile,
                distance,
                curvature,
                siteA ? high : other,
                siteA ? other : high);

        if (highDeficit > 0)
        {
            return double.PositiveInfinity;
        }

        for (int i = 0; i < BinarySearchIterations; i++)
        {
            double mid =
                (low + high) / 2;

            double deficit =
                CalculateCombinedDeficit(
                    link,
                    profile,
                    distance,
                    curvature,
                    siteA ? mid : other,
                    siteA ? other : mid);

            if (deficit <= 0)
            {
                high = mid;
            }
            else
            {
                low = mid;
            }
        }

        return high;
    }

    private double CalculateCombinedDeficit(
        Link link,
        IReadOnlyList<TerrainPoint> profile,
        double distance,
        double curvature,
        double heightA,
        double heightB)
    {
        double max = 0;

        double elevationA =
            link.SiteA.GroundElevationMeters +
            heightA;

        double elevationB =
            link.SiteB.GroundElevationMeters +
            heightB;

        foreach (var point in profile)
        {
            double ratio =
                point.DistanceFromStartMeters /
                distance;

            double los =
                elevationA +
                (elevationB - elevationA) *
                ratio;

            double earth =
                curvature *
                4 *
                ratio *
                (1 - ratio);

            double fresnel =
                _fresnelCalculator.CalculateFirstZoneRadiusMeters(
                    distance,
                    point.DistanceFromStartMeters,
                    link.FrequencyGHz)
                *
                _propagationSettings.FresnelClearanceFactor;

            double deficit =
                point.ElevationMeters +
                earth +
                fresnel -
                los;

            if (deficit > max)
            {
                max = deficit;
            }
        }

        return max;
    }
}