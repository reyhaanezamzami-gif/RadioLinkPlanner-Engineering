using RadioLinkPlannervfV.Core.Enums;
using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.RF.Calculators;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Optimizers;
using RadioLinkPlannervfV.Terrain.Services;

namespace RadioLinkPlannervfV.Terrain.Tests;

public sealed class MinimumAntennaHeightAslTests
{
    [Fact]
    public void LinkAnalyzer_ClearTerrain_ShouldCalculateMinimumAntennaHeightAslFromOptimizedEndpointTops()
    {
        var analyzer =
            CreateAnalyzer();

        var siteA =
            new Site(
                "A",
                new GeoCoordinate(
                    35,
                    51),
                100,
                30);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                30);

        var link =
            new Link(
                siteA,
                siteB,
                6);

        double distance =
            new HaversineDistanceCalculator()
                .CalculateDistanceMeters(
                    siteA.Coordinate,
                    siteB.Coordinate);

        var terrain =
            new List<TerrainPoint>
            {
                new(
                    siteA.Coordinate,
                    0,
                    100),

                new(
                    new GeoCoordinate(
                        35.045,
                        51),
                    distance / 2,
                    100),

                new(
                    siteB.Coordinate,
                    distance,
                    100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.Equal(
            LinkStatus.LineOfSight,
            result.Status);

        Assert.NotNull(
            result.HeightOptimization);

        Assert.Equal(
            30,
            result.HeightOptimization!
                .OptimizedAntennaHeightASiteAMeters,
            precision: 6);

        Assert.Equal(
            30,
            result.HeightOptimization
                .OptimizedAntennaHeightBSiteBMeters,
            precision: 6);

        double expectedMinimumAntennaHeightAsl =
            Math.Max(
                siteA.GroundElevationMeters +
                result.HeightOptimization
                    .OptimizedAntennaHeightASiteAMeters,
                siteB.GroundElevationMeters +
                result.HeightOptimization
                    .OptimizedAntennaHeightBSiteBMeters);

        Assert.Equal(
            expectedMinimumAntennaHeightAsl,
            result.MinimumAntennaHeightASLMeters,
            precision: 6);

        Assert.Equal(
            130,
            result.MinimumAntennaHeightASLMeters,
            precision: 6);
    }

    private static LinkAnalyzer CreateAnalyzer()
    {
        var propagationSettings =
            new PropagationSettings
            {
                KFactor = 4.0 / 3.0,
                FresnelClearanceFactor = 0.6,
                EarthRadiusMeters = 6371000
            };

        var distanceCalculator =
            new HaversineDistanceCalculator();

        var fresnelCalculator =
            new FresnelCalculator();

        var earthCurvatureCalculator =
            new EarthCurvatureCalculator(
                propagationSettings);

        var heightOptimizer =
            new HeightOptimizer(
                distanceCalculator,
                fresnelCalculator,
                earthCurvatureCalculator,
                propagationSettings);

        return new LinkAnalyzer(
            distanceCalculator,
            fresnelCalculator,
            heightOptimizer,
            earthCurvatureCalculator,
            propagationSettings);
    }
}