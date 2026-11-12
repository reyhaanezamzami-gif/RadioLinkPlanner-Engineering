using RadioLinkPlannervfV.Core.Enums;
using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.RF.Calculators;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Optimizers;
using RadioLinkPlannervfV.Terrain.Services;
using Xunit;

namespace RadioLinkPlannervfV.Terrain.Tests;

public sealed class CalculationTests
{
    [Fact]
    public void HaversineDistance_ShouldCalculateCorrectDistance()
    {
        var calculator =
        new HaversineDistanceCalculator();


        var pointA =
            new GeoCoordinate(
                35.6892,
                51.3890);

        var pointB =
            new GeoCoordinate(
                35.7000,
                51.4500);

        double distance =
            calculator.CalculateDistanceMeters(
                pointA,
                pointB);

        Assert.True(distance > 0);

        Assert.InRange(
            distance,
            5500,
            6000);
    }

    [Fact]
    public void HaversineDistance_SamePoint_ShouldReturnZero()
    {
        var calculator =
            new HaversineDistanceCalculator();

        var point =
            new GeoCoordinate(
                35,
                51);

        double distance =
            calculator.CalculateDistanceMeters(
                point,
                point);

        Assert.Equal(
            0,
            distance,
            precision: 10);
    }

    [Fact]
    public void FresnelCalculator_ShouldCalculateFirstZoneRadius()
    {
        var calculator =
            new FresnelCalculator();

        double radius =
            calculator.CalculateFirstZoneRadiusMeters(
                10000,
                5000,
                6);

        Assert.True(
            radius > 0);

        Assert.InRange(
            radius,
            11,
            12);
    }

    [Fact]
    public void FresnelCalculator_AtEndpoint_ShouldReturnZero()
    {
        var calculator =
            new FresnelCalculator();

        double radiusAtA =
            calculator.CalculateFirstZoneRadiusMeters(
                10000,
                0,
                6);

        double radiusAtB =
            calculator.CalculateFirstZoneRadiusMeters(
                10000,
                10000,
                6);

        Assert.Equal(
            0,
            radiusAtA,
            precision: 10);

        Assert.Equal(
            0,
            radiusAtB,
            precision: 10);
    }

    [Fact]
    public void FresnelCalculator_ShouldBeSymmetricAroundMidpoint()
    {
        var calculator =
            new FresnelCalculator();

        double radiusA =
            calculator.CalculateFirstZoneRadiusMeters(
                10000,
                2500,
                6);

        double radiusB =
            calculator.CalculateFirstZoneRadiusMeters(
                10000,
                7500,
                6);

        Assert.Equal(
            radiusA,
            radiusB,
            precision: 10);
    }

    [Fact]
    public void EarthCurvatureCalculator_ShouldCalculateCorrection()
    {
        var settings =
            new PropagationSettings
            {
                EarthRadiusMeters = 6371000,
                KFactor = 4.0 / 3.0
            };

        var calculator =
            new EarthCurvatureCalculator(
                settings);

        double correction =
            calculator.CalculateCorrectionMeters(
                10000,
                4.0 / 3.0);

        Assert.True(
            correction > 0);

        double expected =
            Math.Pow(10000, 2) /
            (8 * 6371000 * (4.0 / 3.0));

        Assert.Equal(
            expected,
            correction,
            precision: 10);
    }

    [Fact]
    public void EarthCurvatureCalculator_InvalidDistance_ShouldReturnZero()
    {
        var settings =
            new PropagationSettings();

        var calculator =
            new EarthCurvatureCalculator(
                settings);

        double correction =
            calculator.CalculateCorrectionMeters(
                0,
                4.0 / 3.0);

        Assert.Equal(
            0,
            correction);
    }

    [Fact]
    public void LinkAnalyzer_EmptyTerrainProfile_ShouldThrow()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

        var link =
            new Link(
                siteA,
                siteB,
                6);

        Assert.Throws<ArgumentException>(
            () =>
                analyzer.Analyze(
                    link,
                    Array.Empty<TerrainPoint>()));
    }

    [Fact]
    public void LinkAnalyzer_SameEndpoints_ShouldThrow()
    {
        var analyzer =
            CreateAnalyzer();

        var coordinate =
            new GeoCoordinate(
                35,
                51);

        var siteA =
            new Site(
                "A",
                coordinate,
                100,
                10);

        var siteB =
            new Site(
                "B",
                coordinate,
                100,
                10);

        var link =
            new Link(
                siteA,
                siteB,
                6);

        var terrain =
            new List<TerrainPoint>
            {
            new(
                coordinate,
                0,
                100)
            };

        Assert.Throws<ArgumentException>(
            () =>
                analyzer.Analyze(
                    link,
                    terrain));
    }

    [Fact]
    public void LinkAnalyzer_InvalidFrequency_ShouldThrow()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

        var link =
            new Link(
                siteA,
                siteB,
                0);

        var terrain =
            new List<TerrainPoint>
            {
            new(
                siteA.Coordinate,
                0,
                100),

            new(
                siteB.Coordinate,
                10000,
                100)
            };

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                analyzer.Analyze(
                    link,
                    terrain));
    }

    [Fact]
    public void LinkAnalyzer_ClearTerrain_ShouldReportLineOfSight()
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

        Assert.True(
            result.FirstFresnelZoneRadiusMeters > 0);

        Assert.True(
            result.RequiredClearanceMeters > 0);

        Assert.True(
            result.MaximumTerrainClearanceDeficitMeters <= 0);
    }

    [Fact]
    public void LinkAnalyzer_HighTerrain_ShouldDetectViolation()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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
                150),

            new(
                siteB.Coordinate,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.True(
            result.MaximumTerrainClearanceDeficitMeters > 0);
    }

    [Fact]
    public void LinkAnalyzer_HighTerrain_ShouldReportObstructedStatus()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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
                150),

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
            LinkStatus.Obstructed,
            result.Status);
    }

    [Fact]
    public void LinkAnalyzer_HighTerrain_ShouldReturnWorstTerrainObstruction()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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

        var obstructionCoordinate =
            new GeoCoordinate(
                35.045,
                51);

        var terrain =
            new List<TerrainPoint>
            {
            new(
                siteA.Coordinate,
                0,
                100),

            new(
                obstructionCoordinate,
                distance / 2,
                150),

            new(
                siteB.Coordinate,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.NotNull(
            result.WorstTerrainObstruction);

        LinkObstruction obstruction =
            result.WorstTerrainObstruction!;

        Assert.Equal(
            obstructionCoordinate,
            obstruction.Coordinate);

        Assert.Equal(
            distance / 2,
            obstruction.DistanceFromSiteAMeters,
            precision: 6);

        Assert.Equal(
            150,
            obstruction.TerrainElevationMeters,
            precision: 6);

        Assert.True(
            obstruction.TerrainElevationWithCurvatureMeters >
            obstruction.TerrainElevationMeters);

        Assert.True(
            obstruction.LineOfSightElevationMeters <
            150);

        Assert.True(
            obstruction.ObstructionAmountMeters > 0);

        Assert.Equal(
            obstruction.TerrainElevationWithCurvatureMeters -
            obstruction.LineOfSightElevationMeters,
            obstruction.ObstructionAmountMeters,
            precision: 6);
    }

    [Fact]
    public void LinkAnalyzer_MultipleObstructions_ShouldReturnLargestObstruction()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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

        var weakerObstructionCoordinate =
            new GeoCoordinate(
                35.027,
                51);

        var strongerObstructionCoordinate =
            new GeoCoordinate(
                35.063,
                51);

        var terrain =
            new List<TerrainPoint>
            {
            new(
                siteA.Coordinate,
                0,
                100),

            new(
                weakerObstructionCoordinate,
                distance * 0.30,
                140),

            new(
                strongerObstructionCoordinate,
                distance * 0.70,
                155),

            new(
                siteB.Coordinate,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.NotNull(
            result.WorstTerrainObstruction);

        LinkObstruction obstruction =
            result.WorstTerrainObstruction!;

        Assert.Equal(
            strongerObstructionCoordinate,
            obstruction.Coordinate);

        Assert.Equal(
            155,
            obstruction.TerrainElevationMeters,
            precision: 6);

        Assert.True(
            obstruction.ObstructionAmountMeters > 0);

        Assert.NotEqual(
            weakerObstructionCoordinate,
            obstruction.Coordinate);
    }

    [Fact]
    public void LinkAnalyzer_ClearGeometricLos_WithInsufficientFresnelClearance_ShouldReportFresnelViolation()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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
                102),

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
            LinkStatus.FresnelViolation,
            result.Status);

        Assert.True(
            result.MaximumTerrainClearanceDeficitMeters > 0);
    }

    [Fact]
    public void LinkAnalyzer_TerrainObstructionAndFresnelViolation_ShouldReportObstructed()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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
                112),

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
            LinkStatus.Obstructed,
            result.Status);

        Assert.True(
            result.MaximumTerrainClearanceDeficitMeters > 0);

        Assert.NotNull(
            result.WorstTerrainObstruction);

        Assert.True(
            result.WorstTerrainObstruction!
                .ObstructionAmountMeters > 0);
    }

    [Fact]
    public void LinkAnalyzer_ShouldReturnMaximumTerrainElevation()
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
                50);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                50);

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
                    35.03,
                    51),
                distance * 0.33,
                125),

            new(
                new GeoCoordinate(
                    35.045,
                    51),
                distance * 0.50,
                175),

            new(
                new GeoCoordinate(
                    35.06,
                    51),
                distance * 0.67,
                140),

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
            175,
            result.MaximumTerrainElevationMeters);
    }

    [Fact]
    public void LinkAnalyzer_RequiredHeightAbove8192Meters_ShouldRemainFinite()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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

        var settings =
            new PropagationSettings
            {
                KFactor = 4.0 / 3.0,
                FresnelClearanceFactor = 0.6,
                EarthRadiusMeters = 6371000
            };

        var fresnelCalculator =
            new FresnelCalculator();

        var earthCurvatureCalculator =
            new EarthCurvatureCalculator(
                settings);

        double midpointDistance =
            distance / 2.0;

        double curvature =
            earthCurvatureCalculator.CalculateCorrectionMeters(
                distance,
                settings.KFactor);

        double fresnelRadius =
            fresnelCalculator.CalculateFirstZoneRadiusMeters(
                distance,
                midpointDistance,
                link.FrequencyGHz);

        double requiredFresnelClearance =
            fresnelRadius *
            settings.FresnelClearanceFactor;

        const double expectedAdditionalHeightMeters =
            9000.0;

        double baseSiteElevation =
            siteA.GroundElevationMeters +
            siteA.AntennaHeightMeters;

        double terrainElevation =
            baseSiteElevation +
            (expectedAdditionalHeightMeters / 2.0) -
            requiredFresnelClearance -
            curvature;

        var terrain =
            new List<TerrainPoint>
            {
            new(
                siteA.Coordinate,
                0,
                siteA.GroundElevationMeters),

            new(
                new GeoCoordinate(
                    35.045,
                    51),
                midpointDistance,
                terrainElevation),

            new(
                siteB.Coordinate,
                distance,
                siteB.GroundElevationMeters)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.False(
            double.IsPositiveInfinity(
                result.RecommendedAntennaHeightASiteAMeters));

        Assert.InRange(
            result.RecommendedAntennaHeightASiteAMeters,
            8192.0,
            10000.0);

        Assert.False(
            double.IsPositiveInfinity(
                result.RecommendedAntennaHeightBSiteBMeters));

        Assert.InRange(
            result.RecommendedAntennaHeightBSiteBMeters,
            8192.0,
            10000.0);
    }

    [Fact]
    public void HeightOptimizer_ClearFlatTerrain_ShouldRequireNoAdditionalHeight()
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
                100);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                100);

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

        Assert.NotNull(
            result.HeightOptimization);

        Assert.Equal(
            0,
            result.HeightOptimization!
                .TotalAdditionalHeightMeters,
            precision: 6);

        Assert.True(
            result.HeightOptimization.IsClear);
    }

    [Fact]
    public void HeightOptimizer_MidpointObstruction_ShouldRequireAdditionalHeight()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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
                150),

            new(
                siteB.Coordinate,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.NotNull(
            result.HeightOptimization);

        Assert.True(
            result.HeightOptimization!
                .TotalAdditionalHeightMeters > 0);

        Assert.True(
            result.HeightOptimization.IsClear);
    }

    [Fact]
    public void HeightOptimizer_ShouldPreserveTotalAdditionalHeight()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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
                150),

            new(
                siteB.Coordinate,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.NotNull(
            result.HeightOptimization);

        double additionalA =
            result.HeightOptimization!
                .OptimizedAntennaHeightASiteAMeters -
            siteA.AntennaHeightMeters;

        double additionalB =
            result.HeightOptimization
                .OptimizedAntennaHeightBSiteBMeters -
            siteB.AntennaHeightMeters;

        Assert.Equal(
            result.HeightOptimization
                .TotalAdditionalHeightMeters,
            additionalA + additionalB,
            precision: 6);
    }

    [Fact]
    public void HeightOptimizer_ShouldDistributeHeightEfficiently()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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
                    35.025,
                    51),
                distance * 0.28,
                150),

            new(
                new GeoCoordinate(
                    35.045,
                    51),
                distance / 2,
                120),

            new(
                new GeoCoordinate(
                    35.065,
                    51),
                distance * 0.72,
                145),

            new(
                siteB.Coordinate,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.NotNull(
            result.HeightOptimization);

        Assert.True(
            result.HeightOptimization!
                .TotalAdditionalHeightMeters > 0);

        Assert.True(
            result.HeightOptimization.IsClear);

        double optimizedA =
            result.HeightOptimization
                .OptimizedAntennaHeightASiteAMeters;

        double optimizedB =
            result.HeightOptimization
                .OptimizedAntennaHeightBSiteBMeters;

        Assert.True(
            optimizedA >= siteA.AntennaHeightMeters);

        Assert.True(
            optimizedB >= siteB.AntennaHeightMeters);
    }

    [Fact]
    public void HeightOptimizer_ShouldNotReduceExistingHeights()
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
                50);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                40);

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
                150),

            new(
                siteB.Coordinate,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.NotNull(
            result.HeightOptimization);

        Assert.True(
            result.HeightOptimization!
                .OptimizedAntennaHeightASiteAMeters >=
            siteA.AntennaHeightMeters);

        Assert.True(
            result.HeightOptimization
                .OptimizedAntennaHeightBSiteBMeters >=
            siteB.AntennaHeightMeters);
    }

    [Fact]
    public void HeightOptimizer_SameEndpoints_ShouldRemainStable()
    {
        var analyzer =
            CreateAnalyzer();

        var coordinateA =
            new GeoCoordinate(
                35,
                51);

        var coordinateB =
            new GeoCoordinate(
                35,
                51.1);

        var siteA =
            new Site(
                "A",
                coordinateA,
                100,
                20);

        var siteB =
            new Site(
                "B",
                coordinateB,
                100,
                20);

        var link =
            new Link(
                siteA,
                siteB,
                6);

        double distance =
            new HaversineDistanceCalculator()
                .CalculateDistanceMeters(
                    coordinateA,
                    coordinateB);

        var terrain =
            new List<TerrainPoint>
            {
            new(
                coordinateA,
                0,
                100),

            new(
                new GeoCoordinate(
                    35,
                    51.05),
                distance / 2,
                100),

            new(
                coordinateB,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.NotNull(
            result.HeightOptimization);

        Assert.True(
            result.HeightOptimization!
                .OptimizedAntennaHeightASiteAMeters >= 20);

        Assert.True(
            result.HeightOptimization
                .OptimizedAntennaHeightBSiteBMeters >= 20);
    }

    [Fact]
    public void HeightOptimizer_OptimizedHeights_ShouldClearTerrainObstruction()
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
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

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
                150),

            new(
                siteB.Coordinate,
                distance,
                100)
            };

        LinkAnalysisResult result =
            analyzer.Analyze(
                link,
                terrain);

        Assert.NotNull(
            result.HeightOptimization);

        HeightOptimizationResult optimization =
            result.HeightOptimization!;

        Assert.True(
            optimization.TotalAdditionalHeightMeters > 0);

        Assert.True(
            optimization.IsClear);

        Assert.True(
            optimization.OptimizedAntennaHeightASiteAMeters >=
            siteA.AntennaHeightMeters);

        Assert.True(
            optimization.OptimizedAntennaHeightBSiteBMeters >=
            siteB.AntennaHeightMeters);
    }

    [Fact]
    public void HeightOptimizer_RequiredHeightAbove8192Meters_ShouldRemainFinite()
    {
        var settings =
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
                settings);

        var heightOptimizer =
            new HeightOptimizer(
                distanceCalculator,
                fresnelCalculator,
                earthCurvatureCalculator,
                settings);

        var siteA =
            new Site(
                "A",
                new GeoCoordinate(
                    35,
                    51),
                100,
                10);

        var siteB =
            new Site(
                "B",
                new GeoCoordinate(
                    35.09,
                    51),
                100,
                10);

        var link =
            new Link(
                siteA,
                siteB,
                6);

        double distance =
            distanceCalculator.CalculateDistanceMeters(
                siteA.Coordinate,
                siteB.Coordinate);

        double midpointDistance =
            distance / 2.0;

        double curvature =
            earthCurvatureCalculator.CalculateCorrectionMeters(
                distance,
                settings.KFactor);

        double fresnelRadius =
            fresnelCalculator.CalculateFirstZoneRadiusMeters(
                distance,
                midpointDistance,
                link.FrequencyGHz);

        double requiredFresnelClearance =
            fresnelRadius *
            settings.FresnelClearanceFactor;

        const double expectedAdditionalHeightMeters =
            9000.0;

        double baseSiteElevation =
            siteA.GroundElevationMeters +
            siteA.AntennaHeightMeters;

        double terrainElevation =
            baseSiteElevation +
            (expectedAdditionalHeightMeters / 2.0) -
            requiredFresnelClearance -
            curvature;

        var terrain =
            new List<TerrainPoint>
            {
            new(
                siteA.Coordinate,
                0,
                siteA.GroundElevationMeters),

            new(
                new GeoCoordinate(
                    35.045,
                    51),
                midpointDistance,
                terrainElevation),

            new(
                siteB.Coordinate,
                distance,
                siteB.GroundElevationMeters)
            };

        HeightOptimizationResult result =
            heightOptimizer.Optimize(
                link,
                terrain,
                requiredFresnelClearance);

        Assert.False(
            double.IsPositiveInfinity(
                result.TotalAdditionalHeightMeters));

        Assert.InRange(
            result.TotalAdditionalHeightMeters,
            8192.0,
            10000.0);

        Assert.True(
            result.IsClear);
    }

    [Fact]
    public void LinkAnalyzer_ShouldReturnTerrainProfileAnalysis()
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

        var midpointCoordinate =
            new GeoCoordinate(
                35.045,
                51);

        var terrain =
            new List<TerrainPoint>
            {
            new(
                siteA.Coordinate,
                0,
                100),

            new(
                midpointCoordinate,
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

        Assert.NotNull(
            result.TerrainProfileAnalysis);

        IReadOnlyList<TerrainProfileAnalysisPoint>
            analysis =
                result.TerrainProfileAnalysis!;

        Assert.Equal(
            terrain.Count,
            analysis.Count);

        Assert.Equal(
            terrain[0].Coordinate,
            analysis[0].Coordinate);

        Assert.Equal(
            terrain[1].DistanceFromStartMeters,
            analysis[1].DistanceFromStartMeters,
            precision: 6);

        Assert.Equal(
            terrain[1].ElevationMeters,
            analysis[1].TerrainElevationMeters,
            precision: 6);

        Assert.True(
            analysis[1].FirstFresnelZoneRadiusMeters > 0);

        Assert.True(
            analysis[1].RequiredFresnelClearanceMeters > 0);

        Assert.True(
            analysis[1].ActualClearanceMeters > 0);

        Assert.Equal(
            analysis[1].RequiredFresnelClearanceMeters -
            analysis[1].ActualClearanceMeters,
            analysis[1].ClearanceDeficitMeters,
            precision: 6);

        Assert.Equal(
            terrain[^1].Coordinate,
            analysis[^1].Coordinate);
    }

    private static LinkAnalyzer CreateAnalyzer()
    {
        var settings =
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
                settings);

        var heightOptimizer =
            new HeightOptimizer(
                distanceCalculator,
                fresnelCalculator,
                earthCurvatureCalculator,
                settings);

        return new LinkAnalyzer(
            distanceCalculator,
            fresnelCalculator,
            heightOptimizer,
            earthCurvatureCalculator,
            settings);
    }


}