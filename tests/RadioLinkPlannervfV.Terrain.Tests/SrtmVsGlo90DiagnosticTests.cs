using System.IO.Compression;
using System.Runtime.InteropServices;
using RadioLinkPlannervfV.Core.Enums;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.RF.Calculators;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Models;
using RadioLinkPlannervfV.Terrain.Optimizers;
using RadioLinkPlannervfV.Terrain.Readers;
using RadioLinkPlannervfV.Terrain.Services;

namespace RadioLinkPlannervfV.Terrain.Tests;

public sealed class SrtmVsGlo90DiagnosticTests
{
    private const int SampleCount = 100;
    private const double FrequencyGHz = 18.0;

    private const double StartLatitude = 36.322331526431554;
    private const double StartLongitude = 59.57538704550899;

    private const double EndLatitude = 36.32446807551602;
    private const double EndLongitude = 59.580907558046874;

    private const double AntennaHeightA = 30.0;
    private const double AntennaHeightB = 25.0;

    private const int Glo90RasterWidth = 1200;
    private const int Glo90RasterHeight = 1200;
    private const int Glo90TileWidth = 2048;
    private const int Glo90TileHeight = 2048;
    private const double Glo90PixelSize = 1.0 / 1200.0;

    private const long Glo90TileOffset = 1265316;
    private const long Glo90TileByteCount = 3656568;

    [Fact]
    public void CompareSrtmAndGlo90_UsingSameRfAnalyzer()
    {
        string projectRoot = FindProjectRoot();

        string srtmDirectory =
            Path.Combine(
                projectRoot,
                "src",
                "RadioLinkPlannervfV.API",
                "Data",
                "SRTM");

        string srtmFile =
            Path.Combine(
                srtmDirectory,
                "N36E059.hgt");

        Assert.True(
            File.Exists(srtmFile),
            $"Expected SRTM file was not found: {srtmFile}");

        string glo90File =
            Path.Combine(
                FindProjectRoot(),
                "assets-files",
                "Copernicus_DSM_COG_30_N36_00_E059_00_DEM.tif");

        Assert.True(
            File.Exists(glo90File),
            $"Expected GLO-90 file was not found: {glo90File}");

        var start =
            new GeoCoordinate(
                StartLatitude,
                StartLongitude);

        var end =
            new GeoCoordinate(
                EndLatitude,
                EndLongitude);

        var coordinates =
            BuildCoordinates(
                start,
                end);

        var distanceCalculator =
            new HaversineDistanceCalculator();

        double distanceMeters =
            distanceCalculator.CalculateDistanceMeters(
                start,
                end);

        var srtmProfile =
            BuildSrtmProfile(
                srtmDirectory,
                coordinates,
                distanceMeters);

        var glo90Reader =
            new Glo90DiagnosticReader(
                glo90File);

        var glo90Profile =
            BuildGlo90Profile(
                glo90Reader,
                coordinates,
                distanceMeters);

        Assert.Equal(
            SampleCount,
            srtmProfile.Count);

        Assert.Equal(
            SampleCount,
            glo90Profile.Count);

        LinkAnalyzer analyzer =
            CreateAnalyzer();

        Link srtmLink =
            CreateLink(
                start,
                end,
                srtmProfile[0].ElevationMeters,
                srtmProfile[^1].ElevationMeters);

        Link glo90Link =
            CreateLink(
                start,
                end,
                glo90Profile[0].ElevationMeters,
                glo90Profile[^1].ElevationMeters);

        LinkAnalysisResult srtmResult =
            analyzer.Analyze(
                srtmLink,
                srtmProfile);

        LinkAnalysisResult glo90Result =
            analyzer.Analyze(
                glo90Link,
                glo90Profile);

        PrintProfileSummary(
            "SRTM",
            srtmProfile);

        PrintProfileSummary(
            "GLO-90",
            glo90Profile);

        PrintProfileDifferenceSummary(
            srtmProfile,
            glo90Profile);

        PrintLargeProfileDifferences(
            srtmProfile,
            glo90Profile);

        PrintAnalysisSummary(
            "SRTM",
            srtmResult);

        PrintAnalysisSummary(
            "GLO-90",
            glo90Result);

        PrintDifferenceSummary(
            srtmResult,
            glo90Result);

        Assert.Equal(
            srtmResult.DistanceMeters,
            glo90Result.DistanceMeters,
            precision: 9);

        Assert.Equal(
            srtmResult.FreeSpacePathLossDb,
            glo90Result.FreeSpacePathLossDb,
            precision: 9);

        Assert.Equal(
            srtmResult.FirstFresnelZoneRadiusMeters,
            glo90Result.FirstFresnelZoneRadiusMeters,
            precision: 9);

        Assert.Equal(
            srtmResult.RequiredClearanceMeters,
            glo90Result.RequiredClearanceMeters,
            precision: 9);
    }

    [Fact]
    public void CompareSrtmAndGlo90_AntennaHeightSensitivity()
    {
        string projectRoot = FindProjectRoot();

        string srtmDirectory =
            Path.Combine(
                projectRoot,
                "src",
                "RadioLinkPlannervfV.API",
                "Data",
                "SRTM");

        string srtmFile =
            Path.Combine(
                srtmDirectory,
                "N36E059.hgt");

        Assert.True(
            File.Exists(srtmFile),
            $"Expected SRTM file was not found: {srtmFile}");

        string glo90File =
            Path.Combine(
                FindProjectRoot(),
                "assets-files",
                "Copernicus_DSM_COG_30_N36_00_E059_00_DEM.tif");

        Assert.True(
            File.Exists(glo90File),
            $"Expected GLO-90 file was not found: {glo90File}");

        var start =
            new GeoCoordinate(
                StartLatitude,
                StartLongitude);

        var end =
            new GeoCoordinate(
                EndLatitude,
                EndLongitude);

        var coordinates =
            BuildCoordinates(
                start,
                end);

        var distanceCalculator =
            new HaversineDistanceCalculator();

        double distanceMeters =
            distanceCalculator.CalculateDistanceMeters(
                start,
                end);

        var srtmProfile =
            BuildSrtmProfile(
                srtmDirectory,
                coordinates,
                distanceMeters);

        var glo90Reader =
            new Glo90DiagnosticReader(
                glo90File);

        var glo90Profile =
            BuildGlo90Profile(
                glo90Reader,
                coordinates,
                distanceMeters);

        Assert.Equal(
            SampleCount,
            srtmProfile.Count);

        Assert.Equal(
            SampleCount,
            glo90Profile.Count);

        LinkAnalyzer analyzer =
            CreateAnalyzer();

        double[] antennaHeights =
        [
            5.00,
            5.25,
            5.50,
            5.75,
            6.00,
            6.25,
            6.50,
            6.75,
            7.00,
            7.25,
            7.50,
            7.75,
            8.00,
            8.25,
            8.50,
            8.75,
            9.00,
            9.25,
            9.50,
            9.75,
            10.00,
            15.00,
            20.00,
            25.00,
            30.00,
            35.00,
            40.00,
            50.00,
            60.00
        ];

        Console.WriteLine();
        Console.WriteLine(
            "===== SRTM VS GLO-90 ANTENNA HEIGHT SENSITIVITY =====");

        Console.WriteLine(
            "Height | SRTM Status | GLO-90 Status | " +
            "SRTM Deficit | GLO-90 Deficit | " +
            "SRTM Min ASL | GLO-90 Min ASL | " +
            "SRTM Rec A | GLO-90 Rec A | " +
            "SRTM Rec B | GLO-90 Rec B | " +
            "SRTM Add | GLO-90 Add");

        foreach (double antennaHeight in antennaHeights)
        {
            Link srtmLink =
                CreateLink(
                    start,
                    end,
                    srtmProfile[0].ElevationMeters,
                    srtmProfile[^1].ElevationMeters,
                    antennaHeight,
                    antennaHeight);

            Link glo90Link =
                CreateLink(
                    start,
                    end,
                    glo90Profile[0].ElevationMeters,
                    glo90Profile[^1].ElevationMeters,
                    antennaHeight,
                    antennaHeight);

            LinkAnalysisResult srtmResult =
                analyzer.Analyze(
                    srtmLink,
                    srtmProfile);

            LinkAnalysisResult glo90Result =
                analyzer.Analyze(
                    glo90Link,
                    glo90Profile);

            if (antennaHeight == 5.0)
            {
                PrintDeficitDiagnostics(
                    "SRTM",
                    srtmResult);

                PrintDeficitDiagnostics(
                    "GLO-90",
                    glo90Result);
            }

            Assert.Equal(
                srtmResult.DistanceMeters,
                glo90Result.DistanceMeters,
                precision: 9);

            Assert.Equal(
                srtmResult.FreeSpacePathLossDb,
                glo90Result.FreeSpacePathLossDb,
                precision: 9);

            Assert.Equal(
                srtmResult.FirstFresnelZoneRadiusMeters,
                glo90Result.FirstFresnelZoneRadiusMeters,
                precision: 9);

            Assert.Equal(
                srtmResult.RequiredClearanceMeters,
                glo90Result.RequiredClearanceMeters,
                precision: 9);

            double srtmAdditionalHeight =
                srtmResult.HeightOptimization?.TotalAdditionalHeightMeters
                ?? double.NaN;

            double glo90AdditionalHeight =
                glo90Result.HeightOptimization?.TotalAdditionalHeightMeters
                ?? double.NaN;

            Console.WriteLine(
                $"{antennaHeight,6:F1} | " +
                $"{srtmResult.Status,-11} | " +
                $"{glo90Result.Status,-13} | " +
                $"{srtmResult.MaximumTerrainClearanceDeficitMeters,12:F6} | " +
                $"{glo90Result.MaximumTerrainClearanceDeficitMeters,13:F6} | " +
                $"{srtmResult.MinimumAntennaHeightASLMeters,12:F6} | " +
                $"{glo90Result.MinimumAntennaHeightASLMeters,13:F6} | " +
                $"{srtmResult.RecommendedAntennaHeightASiteAMeters,10:F6} | " +
                $"{glo90Result.RecommendedAntennaHeightASiteAMeters,11:F6} | " +
                $"{srtmResult.RecommendedAntennaHeightBSiteBMeters,10:F6} | " +
                $"{glo90Result.RecommendedAntennaHeightBSiteBMeters,11:F6} | " +
                $"{srtmAdditionalHeight,8:F6} | " +
                $"{glo90AdditionalHeight,9:F6}");

            Console.WriteLine(
                $"  Optimized A: " +
                $"SRTM={srtmResult.HeightOptimization?.OptimizedAntennaHeightASiteAMeters.ToString("F6") ?? "null"}, " +
                $"GLO-90={glo90Result.HeightOptimization?.OptimizedAntennaHeightASiteAMeters.ToString("F6") ?? "null"}");

            Console.WriteLine(
                $"  Optimized B: " +
                $"SRTM={srtmResult.HeightOptimization?.OptimizedAntennaHeightBSiteBMeters.ToString("F6") ?? "null"}, " +
                $"GLO-90={glo90Result.HeightOptimization?.OptimizedAntennaHeightBSiteBMeters.ToString("F6") ?? "null"}");

            Console.WriteLine(
                $"  IsClear: " +
                $"SRTM={srtmResult.HeightOptimization?.IsClear.ToString() ?? "null"}, " +
                $"GLO-90={glo90Result.HeightOptimization?.IsClear.ToString() ?? "null"}");
        }
    }

    [Fact]
    public void CompareSrtmAndGlo90_FindFresnelThreshold()
    {
        string projectRoot = FindProjectRoot();

        string srtmDirectory =
            Path.Combine(
                projectRoot,
                "src",
                "RadioLinkPlannervfV.API",
                "Data",
                "SRTM");

        string srtmFile =
            Path.Combine(
                srtmDirectory,
                "N36E059.hgt");

        Assert.True(
            File.Exists(srtmFile),
            $"Expected SRTM file was not found: {srtmFile}");

        string glo90File =
            Path.Combine(
                FindProjectRoot(),
                "assets-files",
                "Copernicus_DSM_COG_30_N36_00_E059_00_DEM.tif");

        Assert.True(
            File.Exists(glo90File),
            $"Expected GLO-90 file was not found: {glo90File}");

        var start =
            new GeoCoordinate(
                StartLatitude,
                StartLongitude);

        var end =
            new GeoCoordinate(
                EndLatitude,
                EndLongitude);

        var coordinates =
            BuildCoordinates(
                start,
                end);

        var distanceCalculator =
            new HaversineDistanceCalculator();

        double distanceMeters =
            distanceCalculator.CalculateDistanceMeters(
                start,
                end);

        var srtmProfile =
            BuildSrtmProfile(
                srtmDirectory,
                coordinates,
                distanceMeters);

        var glo90Reader =
            new Glo90DiagnosticReader(
                glo90File);

        var glo90Profile =
            BuildGlo90Profile(
                glo90Reader,
                coordinates,
                distanceMeters);

        LinkAnalyzer analyzer =
            CreateAnalyzer();

        double srtmLow =
            5.0;

        double srtmHigh =
            5.25;

        double glo90Low =
            0.0;

        double glo90High =
            5.0;

        const int iterations = 40;

        for (int iteration = 0;
             iteration < iterations;
             iteration++)
        {
            double srtmMid =
                (srtmLow + srtmHigh) / 2.0;

            Link srtmLink =
                CreateLink(
                    start,
                    end,
                    srtmProfile[0].ElevationMeters,
                    srtmProfile[^1].ElevationMeters,
                    srtmMid,
                    srtmMid);

            LinkAnalysisResult srtmResult =
                analyzer.Analyze(
                    srtmLink,
                    srtmProfile);

            if (srtmResult.Status ==
                LinkStatus.FresnelViolation)
            {
                srtmLow = srtmMid;
            }
            else
            {
                srtmHigh = srtmMid;
            }

            double glo90Mid =
                (glo90Low + glo90High) / 2.0;

            Link glo90Link =
                CreateLink(
                    start,
                    end,
                    glo90Profile[0].ElevationMeters,
                    glo90Profile[^1].ElevationMeters,
                    glo90Mid,
                    glo90Mid);

            LinkAnalysisResult glo90Result =
                analyzer.Analyze(
                    glo90Link,
                    glo90Profile);

            if (glo90Result.Status ==
                LinkStatus.FresnelViolation)
            {
                glo90Low = glo90Mid;
            }
            else
            {
                glo90High = glo90Mid;
            }
        }

        double srtmThreshold =
            srtmHigh;

        double glo90Threshold =
            glo90High;

        Link srtmThresholdLink =
            CreateLink(
                start,
                end,
                srtmProfile[0].ElevationMeters,
                srtmProfile[^1].ElevationMeters,
                srtmThreshold,
                srtmThreshold);

        LinkAnalysisResult srtmThresholdResult =
            analyzer.Analyze(
                srtmThresholdLink,
                srtmProfile);

        Link glo90ThresholdLink =
            CreateLink(
                start,
                end,
                glo90Profile[0].ElevationMeters,
                glo90Profile[^1].ElevationMeters,
                glo90Threshold,
                glo90Threshold);

        LinkAnalysisResult glo90ThresholdResult =
            analyzer.Analyze(
                glo90ThresholdLink,
                glo90Profile);

        Console.WriteLine();
        Console.WriteLine(
            "===== SRTM VS GLO-90 FRESNEL THRESHOLD =====");

        Console.WriteLine(
            $"SRTM threshold height  : " +
            $"{srtmThreshold:F12} m");

        Console.WriteLine(
            $"SRTM status at high   : " +
            $"{srtmThresholdResult.Status}");

        Console.WriteLine(
            $"SRTM deficit at high  : " +
            $"{srtmThresholdResult.MaximumTerrainClearanceDeficitMeters:F12} m");

        Console.WriteLine(
            $"GLO-90 threshold height: " +
            $"{glo90Threshold:F12} m");

        Console.WriteLine(
            $"GLO-90 status at high  : " +
            $"{glo90ThresholdResult.Status}");

        Console.WriteLine(
            $"GLO-90 deficit at high : " +
            $"{glo90ThresholdResult.MaximumTerrainClearanceDeficitMeters:F12} m");

        Console.WriteLine(
            $"Threshold difference    : " +
            $"{(srtmThreshold - glo90Threshold):F12} m");

        Assert.Equal(
            LinkStatus.LineOfSight,
            srtmThresholdResult.Status);

        Assert.Equal(
            LinkStatus.LineOfSight,
            glo90ThresholdResult.Status);
    }

    private static void PrintDeficitDiagnostics(
        string name,
        LinkAnalysisResult result)
    {
        if (result.TerrainProfileAnalysis is null ||
            result.TerrainProfileAnalysis.Count == 0)
        {
            Console.WriteLine(
                $"{name}: TerrainProfileAnalysis is empty.");

            return;
        }

        double maximumClearanceDeficit =
            double.MinValue;

        int maximumDeficitIndex =
            -1;

        double maximumTerrainObstruction =
            double.MinValue;

        int maximumTerrainObstructionIndex =
            -1;

        for (int index = 0;
             index < result.TerrainProfileAnalysis.Count;
             index++)
        {
            TerrainProfileAnalysisPoint point =
                result.TerrainProfileAnalysis[index];

            double terrainObstruction =
                point.TerrainElevationWithCurvatureMeters -
                point.LineOfSightElevationMeters;

            if (terrainObstruction >
                maximumTerrainObstruction)
            {
                maximumTerrainObstruction =
                    terrainObstruction;

                maximumTerrainObstructionIndex =
                    index;
            }

            if (point.ClearanceDeficitMeters >
                maximumClearanceDeficit)
            {
                maximumClearanceDeficit =
                    point.ClearanceDeficitMeters;

                maximumDeficitIndex =
                    index;
            }
        }

        Console.WriteLine();
        Console.WriteLine(
            $"===== {name} 5m DEFICIT DIAGNOSTIC =====");

        Console.WriteLine(
            $"Result Maximum Deficit Property : " +
            $"{result.MaximumTerrainClearanceDeficitMeters:F9} m");

        Console.WriteLine(
            $"Maximum Profile Clearance Deficit: " +
            $"{maximumClearanceDeficit:F9} m");

        Console.WriteLine(
            $"Maximum Terrain Obstruction     : " +
            $"{maximumTerrainObstruction:F9} m");

        Console.WriteLine(
            $"Max Clearance Deficit Index     : " +
            $"{maximumDeficitIndex}");

        Console.WriteLine(
            $"Max Terrain Obstruction Index   : " +
            $"{maximumTerrainObstructionIndex}");

        if (maximumDeficitIndex >= 0)
        {
            TerrainProfileAnalysisPoint point =
                result.TerrainProfileAnalysis[
                    maximumDeficitIndex];

            Console.WriteLine(
                $"Clearance Deficit Distance     : " +
                $"{point.DistanceFromStartMeters:F9} m");

            Console.WriteLine(
                $"Terrain Elevation              : " +
                $"{point.TerrainElevationMeters:F9} m");

            Console.WriteLine(
                $"Terrain + Curvature            : " +
                $"{point.TerrainElevationWithCurvatureMeters:F9} m");

            Console.WriteLine(
                $"LOS Elevation                  : " +
                $"{point.LineOfSightElevationMeters:F9} m");

            Console.WriteLine(
                $"Fresnel Radius                 : " +
                $"{point.FirstFresnelZoneRadiusMeters:F9} m");

            Console.WriteLine(
                $"Required Fresnel Clearance     : " +
                $"{point.RequiredFresnelClearanceMeters:F9} m");

            Console.WriteLine(
                $"Actual Clearance               : " +
                $"{point.ActualClearanceMeters:F9} m");

            Console.WriteLine(
                $"Clearance Deficit              : " +
                $"{point.ClearanceDeficitMeters:F9} m");

            Console.WriteLine(
                $"Coordinate                     : " +
                $"{point.Coordinate.Latitude:F12}, " +
                $"{point.Coordinate.Longitude:F12}");
        }

        if (maximumTerrainObstructionIndex >= 0)
        {
            TerrainProfileAnalysisPoint point =
                result.TerrainProfileAnalysis[
                    maximumTerrainObstructionIndex];

            Console.WriteLine(
                $"Terrain Obstruction Distance   : " +
                $"{point.DistanceFromStartMeters:F9} m");

            Console.WriteLine(
                $"Terrain Obstruction Value      : " +
                $"{(point.TerrainElevationWithCurvatureMeters -
                   point.LineOfSightElevationMeters):F9} m");
        }
    }

    private static IReadOnlyList<GeoCoordinate> BuildCoordinates(
        GeoCoordinate start,
        GeoCoordinate end)
    {
        var result =
            new List<GeoCoordinate>(
                SampleCount);

        for (int index = 0; index < SampleCount; index++)
        {
            double fraction =
                (double)index /
                (SampleCount - 1);

            result.Add(
                GreatCircleInterpolator.Interpolate(
                    start,
                    end,
                    fraction));
        }

        return result;
    }

    private static IReadOnlyList<TerrainPoint> BuildSrtmProfile(
        string demDirectory,
        IReadOnlyList<GeoCoordinate> coordinates,
        double distanceMeters)
    {
        var reader =
            new SrtmDemReader(
                new DemOptions
                {
                    DataDirectory = demDirectory
                });

        var result =
            new List<TerrainPoint>(
                coordinates.Count);

        for (int index = 0; index < coordinates.Count; index++)
        {
            double fraction =
                (double)index /
                (coordinates.Count - 1);

            double distanceFromStart =
                distanceMeters *
                fraction;

            TerrainElevationResult elevation =
                reader.ReadElevation(
                    coordinates[index]);

            result.Add(
                new TerrainPoint(
                    coordinates[index],
                    distanceFromStart,
                    elevation.ElevationMeters));
        }

        return result;
    }

    private static IReadOnlyList<TerrainPoint> BuildGlo90Profile(
        Glo90DiagnosticReader reader,
        IReadOnlyList<GeoCoordinate> coordinates,
        double distanceMeters)
    {
        var result =
            new List<TerrainPoint>(
                coordinates.Count);

        for (int index = 0; index < coordinates.Count; index++)
        {
            double fraction =
                (double)index /
                (coordinates.Count - 1);

            double distanceFromStart =
                distanceMeters *
                fraction;

            double elevation =
                reader.ReadElevation(
                    coordinates[index]);

            result.Add(
                new TerrainPoint(
                    coordinates[index],
                    distanceFromStart,
                    elevation));
        }

        return result;
    }

    private static Link CreateLink(
        GeoCoordinate start,
        GeoCoordinate end,
        double groundElevationA,
        double groundElevationB)
    {
        return CreateLink(
            start,
            end,
            groundElevationA,
            groundElevationB,
            AntennaHeightA,
            AntennaHeightB);
    }

    private static Link CreateLink(
        GeoCoordinate start,
        GeoCoordinate end,
        double groundElevationA,
        double groundElevationB,
        double antennaHeightA,
        double antennaHeightB)
    {
        var siteA =
            new Site(
                "A",
                start,
                groundElevationA,
                antennaHeightA);

        var siteB =
            new Site(
                "B",
                end,
                groundElevationB,
                antennaHeightB);

        return new Link(
            siteA,
            siteB,
            FrequencyGHz);
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

    private static void PrintProfileSummary(
        string name,
        IReadOnlyList<TerrainPoint> profile)
    {
        double min =
            profile.Min(
                x => x.ElevationMeters);

        double max =
            profile.Max(
                x => x.ElevationMeters);

        double average =
            profile.Average(
                x => x.ElevationMeters);

        Console.WriteLine();
        Console.WriteLine(
            $"===== {name} PROFILE =====");

        Console.WriteLine(
            $"Samples       : {profile.Count}");

        Console.WriteLine(
            $"Start Elev.   : {profile[0].ElevationMeters:F6} m");

        Console.WriteLine(
            $"End Elev.     : {profile[^1].ElevationMeters:F6} m");

        Console.WriteLine(
            $"Min Elev.     : {min:F6} m");

        Console.WriteLine(
            $"Max Elev.     : {max:F6} m");

        Console.WriteLine(
            $"Average Elev. : {average:F6} m");

        Console.WriteLine(
            $"Range         : {(max - min):F6} m");
    }

    private static void PrintProfileDifferenceSummary(
        IReadOnlyList<TerrainPoint> srtm,
        IReadOnlyList<TerrainPoint> glo90)
    {
        Assert.Equal(srtm.Count, glo90.Count);

        double sumDelta = 0.0;
        double sumAbsoluteDelta = 0.0;

        double maximumAbsoluteDelta = double.MinValue;
        int maximumDeltaIndex = -1;

        for (int index = 0; index < srtm.Count; index++)
        {
            double delta =
                glo90[index].ElevationMeters -
                srtm[index].ElevationMeters;

            double absoluteDelta =
                Math.Abs(delta);

            sumDelta += delta;
            sumAbsoluteDelta += absoluteDelta;

            if (absoluteDelta > maximumAbsoluteDelta)
            {
                maximumAbsoluteDelta = absoluteDelta;
                maximumDeltaIndex = index;
            }
        }

        double meanDelta =
            sumDelta /
            srtm.Count;

        double meanAbsoluteDelta =
            sumAbsoluteDelta /
            srtm.Count;

        TerrainPoint srtmPoint =
            srtm[maximumDeltaIndex];

        TerrainPoint glo90Point =
            glo90[maximumDeltaIndex];

        double maximumDelta =
            glo90Point.ElevationMeters -
            srtmPoint.ElevationMeters;

        Console.WriteLine();
        Console.WriteLine(
            "===== GLO-90 MINUS SRTM PROFILE SAMPLE DELTA =====");

        Console.WriteLine(
            $"Samples                 : {srtm.Count}");

        Console.WriteLine(
            $"Mean Delta              : {meanDelta:F9} m");

        Console.WriteLine(
            $"Mean Absolute Delta     : {meanAbsoluteDelta:F9} m");

        Console.WriteLine(
            $"Maximum Absolute Delta  : {maximumAbsoluteDelta:F9} m");

        Console.WriteLine(
            $"Maximum Delta           : {maximumDelta:F9} m");

        Console.WriteLine(
            $"Maximum Delta Index     : {maximumDeltaIndex}");

        Console.WriteLine(
            $"Maximum Delta Distance  : " +
            $"{srtmPoint.DistanceFromStartMeters:F9} m");

        Console.WriteLine(
            $"Maximum Delta Coordinate: " +
            $"{srtmPoint.Coordinate.Latitude:F12}, " +
            $"{srtmPoint.Coordinate.Longitude:F12}");

        Console.WriteLine(
            $"SRTM Elevation          : " +
            $"{srtmPoint.ElevationMeters:F9} m");

        Console.WriteLine(
            $"GLO-90 Elevation        : " +
            $"{glo90Point.ElevationMeters:F9} m");

        Console.WriteLine();
        Console.WriteLine(
            "===== SAMPLE DELTAS =====");

        Console.WriteLine(
            "Index   Distance(m)      SRTM(m)       GLO-90(m)      Delta(m)");

        for (int index = 0; index < srtm.Count; index++)
        {
            double delta =
                glo90[index].ElevationMeters -
                srtm[index].ElevationMeters;

            Console.WriteLine(
                $"{index,5}   " +
                $"{srtm[index].DistanceFromStartMeters,12:F3}   " +
                $"{srtm[index].ElevationMeters,12:F3}   " +
                $"{glo90[index].ElevationMeters,12:F3}   " +
                $"{delta,10:F3}");
        }
    }

    private static void PrintLargeProfileDifferences(
        IReadOnlyList<TerrainPoint> srtm,
        IReadOnlyList<TerrainPoint> glo90)
    {
        const double thresholdMeters = 5.0;

        Assert.Equal(
            srtm.Count,
            glo90.Count);

        Console.WriteLine();
        Console.WriteLine(
            "===== LARGE PROFILE DIFFERENCES (ABS DELTA >= 5 m) =====");

        Console.WriteLine(
            $"Threshold                 : {thresholdMeters:F3} m");

        Console.WriteLine(
            "Index   Distance(m)      Latitude          Longitude         " +
            "SRTM(m)       GLO-90(m)      Delta(m)");

        int matchCount = 0;

        for (int index = 0; index < srtm.Count; index++)
        {
            double delta =
                glo90[index].ElevationMeters -
                srtm[index].ElevationMeters;

            if (Math.Abs(delta) < thresholdMeters)
            {
                continue;
            }

            matchCount++;

            TerrainPoint currentSrtm =
                srtm[index];

            TerrainPoint currentGlo90 =
                glo90[index];

            Console.WriteLine(
                $"{index,5}   " +
                $"{currentSrtm.DistanceFromStartMeters,12:F3}   " +
                $"{currentSrtm.Coordinate.Latitude,15:F9}   " +
                $"{currentSrtm.Coordinate.Longitude,15:F9}   " +
                $"{currentSrtm.ElevationMeters,12:F3}   " +
                $"{currentGlo90.ElevationMeters,12:F3}   " +
                $"{delta,10:F3}");

            if (index > 0)
            {
                TerrainPoint previousSrtm =
                    srtm[index - 1];

                TerrainPoint previousGlo90 =
                    glo90[index - 1];

                double previousDelta =
                    previousGlo90.ElevationMeters -
                    previousSrtm.ElevationMeters;

                Console.WriteLine(
                    $"      PREV : " +
                    $"index={index - 1}, " +
                    $"distance={previousSrtm.DistanceFromStartMeters:F3} m, " +
                    $"SRTM={previousSrtm.ElevationMeters:F3} m, " +
                    $"GLO-90={previousGlo90.ElevationMeters:F3} m, " +
                    $"delta={previousDelta:F3} m");
            }

            if (index + 1 < srtm.Count)
            {
                TerrainPoint nextSrtm =
                    srtm[index + 1];

                TerrainPoint nextGlo90 =
                    glo90[index + 1];

                double nextDelta =
                    nextGlo90.ElevationMeters -
                    nextSrtm.ElevationMeters;

                Console.WriteLine(
                    $"      NEXT : " +
                    $"index={index + 1}, " +
                    $"distance={nextSrtm.DistanceFromStartMeters:F3} m, " +
                    $"SRTM={nextSrtm.ElevationMeters:F3} m, " +
                    $"GLO-90={nextGlo90.ElevationMeters:F3} m, " +
                    $"delta={nextDelta:F3} m");
            }

            Console.WriteLine();
        }

        Console.WriteLine(
            $"Samples with |delta| >= {thresholdMeters:F3} m : {matchCount}");
    }

    private static void PrintAnalysisSummary(
        string name,
        LinkAnalysisResult result)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"===== {name} RF ANALYSIS =====");

        Console.WriteLine(
            $"Distance                  : {result.DistanceMeters:F9} m");

        Console.WriteLine(
            $"FSPL                      : {result.FreeSpacePathLossDb:F9} dB");

        Console.WriteLine(
            $"First Fresnel Radius     : {result.FirstFresnelZoneRadiusMeters:F9} m");

        Console.WriteLine(
            $"Required Clearance       : {result.RequiredClearanceMeters:F9} m");

        Console.WriteLine(
            $"Minimum Antenna ASL      : {result.MinimumAntennaHeightASLMeters:F9} m");

        Console.WriteLine(
            $"Maximum Terrain          : {result.MaximumTerrainElevationMeters:F9} m");

        Console.WriteLine(
            $"Maximum Deficit          : {result.MaximumTerrainClearanceDeficitMeters:F9} m");

        Console.WriteLine(
            $"Status                    : {result.Status}");

        Console.WriteLine(
            $"Recommended A Height     : {result.RecommendedAntennaHeightASiteAMeters:F9} m");

        Console.WriteLine(
            $"Recommended B Height     : {result.RecommendedAntennaHeightBSiteBMeters:F9} m");

        if (result.HeightOptimization is not null)
        {
            Console.WriteLine(
                $"Optimized A Height       : {result.HeightOptimization.OptimizedAntennaHeightASiteAMeters:F9} m");

            Console.WriteLine(
                $"Optimized B Height       : {result.HeightOptimization.OptimizedAntennaHeightBSiteBMeters:F9} m");

            Console.WriteLine(
                $"Additional Height Total  : {result.HeightOptimization.TotalAdditionalHeightMeters:F9} m");

            Console.WriteLine(
                $"Optimization Clear       : {result.HeightOptimization.IsClear}");
        }

        if (result.WorstTerrainObstruction is not null)
        {
            Console.WriteLine(
                $"Worst Obstruction        : {result.WorstTerrainObstruction.ObstructionAmountMeters:F9} m");

            Console.WriteLine(
                $"Worst Obstruction Dist.  : {result.WorstTerrainObstruction.DistanceFromSiteAMeters:F9} m");

            Console.WriteLine(
                $"Worst Obstruction Coord. : " +
                $"{result.WorstTerrainObstruction.Coordinate.Latitude:F12}, " +
                $"{result.WorstTerrainObstruction.Coordinate.Longitude:F12}");
        }
        else
        {
            Console.WriteLine(
                "Worst Obstruction        : none");
        }
    }

    private static void PrintDifferenceSummary(
        LinkAnalysisResult srtm,
        LinkAnalysisResult glo90)
    {
        Console.WriteLine();
        Console.WriteLine(
            "===== GLO-90 MINUS SRTM =====");

        Console.WriteLine(
            $"Maximum Terrain Delta    : " +
            $"{glo90.MaximumTerrainElevationMeters - srtm.MaximumTerrainElevationMeters:F9} m");

        Console.WriteLine(
            $"Maximum Deficit Delta    : " +
            $"{glo90.MaximumTerrainClearanceDeficitMeters - srtm.MaximumTerrainClearanceDeficitMeters:F9} m");

        Console.WriteLine(
            $"Minimum ASL Delta        : " +
            $"{glo90.MinimumAntennaHeightASLMeters - srtm.MinimumAntennaHeightASLMeters:F9} m");

        Console.WriteLine(
            $"Recommended A Delta      : " +
            $"{glo90.RecommendedAntennaHeightASiteAMeters - srtm.RecommendedAntennaHeightASiteAMeters:F9} m");

        Console.WriteLine(
            $"Recommended B Delta      : " +
            $"{glo90.RecommendedAntennaHeightBSiteBMeters - srtm.RecommendedAntennaHeightBSiteBMeters:F9} m");

        if (srtm.HeightOptimization is not null &&
            glo90.HeightOptimization is not null)
        {
            Console.WriteLine(
                $"Optimized A Delta        : " +
                $"{glo90.HeightOptimization.OptimizedAntennaHeightASiteAMeters - srtm.HeightOptimization.OptimizedAntennaHeightASiteAMeters:F9} m");

            Console.WriteLine(
                $"Optimized B Delta        : " +
                $"{glo90.HeightOptimization.OptimizedAntennaHeightBSiteBMeters - srtm.HeightOptimization.OptimizedAntennaHeightBSiteBMeters:F9} m");

            Console.WriteLine(
                $"Additional Height Delta  : " +
                $"{glo90.HeightOptimization.TotalAdditionalHeightMeters - srtm.HeightOptimization.TotalAdditionalHeightMeters:F9} m");
        }

        Console.WriteLine(
            $"Status SRTM              : {srtm.Status}");

        Console.WriteLine(
            $"Status GLO-90             : {glo90.Status}");
    }

    private static string FindProjectRoot()
    {
        DirectoryInfo? directory =
            new DirectoryInfo(
                AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionFile =
                Path.Combine(
                    directory.FullName,
                    "RadioLinkPlannervfV.slnx");

            if (File.Exists(solutionFile))
            {
                return directory.FullName;
            }

            directory =
                directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate RadioLinkPlannervfV.slnx from the test output directory.");
    }

    private sealed class Glo90DiagnosticReader
    {
        private readonly byte[] _pixels;

        private readonly string _rasterTypeDescription;

        public Glo90DiagnosticReader(
            string filePath)
        {
            byte[] file =
                File.ReadAllBytes(
                    filePath);

            PrintGeoTiffMetadata(
                file);

            _rasterTypeDescription =
                ReadRasterTypeDescription(
                    file);

            long tileEnd =
                checked(
                    Glo90TileOffset +
                    Glo90TileByteCount);

            if (file.LongLength < tileEnd)
            {
                throw new InvalidDataException(
                    "GLO-90 TIFF is smaller than the expected tile range.");
            }

            byte[] compressed =
                new byte[
                    Glo90TileByteCount];

            Buffer.BlockCopy(
                file,
                checked((int)Glo90TileOffset),
                compressed,
                0,
                checked((int)Glo90TileByteCount));

            if (compressed.Length < 6 ||
                compressed[0] != 0x78)
            {
                throw new InvalidDataException(
                    "Expected a zlib-wrapped GLO-90 tile stream.");
            }

            byte[] raw;

            using (
                var input =
                    new MemoryStream(
                        compressed,
                        2,
                        compressed.Length - 6,
                        writable: false))
            using (
                var deflate =
                    new DeflateStream(
                        input,
                        CompressionMode.Decompress))
            using (
                var output =
                    new MemoryStream())
            {
                deflate.CopyTo(
                    output);

                raw =
                    output.ToArray();
            }

            int expectedLength =
                Glo90TileWidth *
                Glo90TileHeight *
                sizeof(float);

            if (raw.Length != expectedLength)
            {
                throw new InvalidDataException(
                    $"Unexpected GLO-90 tile size. " +
                    $"Expected {expectedLength}, got {raw.Length}.");
            }

            int rowBytes =
                Glo90TileWidth *
                sizeof(float);

            int planeWidth =
                Glo90TileWidth;

            for (int row = 0;
                 row < Glo90TileHeight;
                 row++)
            {
                int rowBase =
                    row *
                    rowBytes;

                for (int plane = 0;
                     plane < 4;
                     plane++)
                {
                    int start =
                        rowBase +
                        plane *
                        planeWidth;

                    for (int column = 1;
                         column < Glo90TileWidth;
                         column++)
                    {
                        int current =
                            start +
                            column;

                        int previous =
                            current -
                            1;

                        raw[current] =
                            unchecked(
                                (byte)(
                                    raw[current] +
                                    raw[previous]));
                    }
                }
            }

            _pixels =
                new byte[
                    Glo90TileWidth *
                    Glo90TileHeight *
                    sizeof(float)];

            for (int row = 0;
                 row < Glo90TileHeight;
                 row++)
            {
                int rowBase =
                    row *
                    rowBytes;

                for (int column = 0;
                     column < Glo90TileWidth;
                     column++)
                {
                    int pixel =
                        rowBase +
                        column *
                        sizeof(float);

                    _pixels[pixel] =
                        raw[
                            rowBase +
                            3 * planeWidth +
                            column];

                    _pixels[pixel + 1] =
                        raw[
                            rowBase +
                            2 * planeWidth +
                            column];

                    _pixels[pixel + 2] =
                        raw[
                            rowBase +
                            planeWidth +
                            column];

                    _pixels[pixel + 3] =
                        raw[
                            rowBase +
                            column];
                }
            }

            ValidateReferencePoints();
        }

        public double ReadElevation(
            GeoCoordinate coordinate)
        {
            double x =
                (coordinate.Longitude - 59.0) /
                Glo90PixelSize;

            double y =
                (37.0 - coordinate.Latitude) /
                Glo90PixelSize;

            int column =
                Math.Clamp(
                    (int)Math.Round(
                        x,
                        MidpointRounding.ToEven),
                    0,
                    Glo90RasterWidth - 1);

            int row =
                Math.Clamp(
                    (int)Math.Round(
                        y,
                        MidpointRounding.ToEven),
                    0,
                    Glo90RasterHeight - 1);

            return ReadPixel(
                row,
                column);
        }

        private double ReadPixel(
            int row,
            int column)
        {
            int offset =
                (
                    row *
                    Glo90TileWidth +
                    column
                ) *
                sizeof(float);

            return MemoryMarshal.Read<float>(
                _pixels.AsSpan(
                    offset,
                    sizeof(float)));
        }

        private void PrintPixelSelectionDiagnostic(
            string name,
            GeoCoordinate coordinate,
            double expected)
        {
            double x =
                (coordinate.Longitude - 59.0) /
                Glo90PixelSize;

            double y =
                (37.0 - coordinate.Latitude) /
                Glo90PixelSize;

            int floorColumn =
                Math.Clamp(
                    (int)Math.Floor(x),
                    0,
                    Glo90RasterWidth - 1);

            int floorRow =
                Math.Clamp(
                    (int)Math.Floor(y),
                    0,
                    Glo90RasterHeight - 1);

            Console.WriteLine();
            Console.WriteLine(
                $"===== GLO-90 {name} PIXEL SEMANTICS DIAGNOSTIC =====");

            Console.WriteLine(
                $"Coordinate            : " +
                $"{coordinate.Latitude:F15}, {coordinate.Longitude:F15}");

            Console.WriteLine(
                $"Raster Type           : " +
                _rasterTypeDescription);

            Console.WriteLine(
                $"x                     : {x:F15}");

            Console.WriteLine(
                $"y                     : {y:F15}");

            Console.WriteLine(
                $"x fractional          : " +
                $"{x - Math.Floor(x):F15}");

            Console.WriteLine(
                $"y fractional          : " +
                $"{y - Math.Floor(y):F15}");

            Console.WriteLine(
                $"Floor pixel           : " +
                $"row={floorRow}, column={floorColumn}");

            Console.WriteLine(
                $"Floor value           : " +
                $"{ReadPixel(floorRow, floorColumn):F12}");

            Console.WriteLine(
                $"Expected value        : " +
                $"{expected:F12}");

            PrintPixelGeoLocation(
                "floor",
                floorRow,
                floorColumn,
                coordinate);

            PrintPixelGeoLocation(
                "row+1",
                floorRow + 1,
                floorColumn,
                coordinate);

            PrintPixelGeoLocation(
                "round",
                (int)Math.Round(
                    y,
                    MidpointRounding.ToEven),
                (int)Math.Round(
                    x,
                    MidpointRounding.ToEven),
                coordinate);

            Console.WriteLine();
            Console.WriteLine(
                "3x3 neighborhood:");

            Console.WriteLine(
                "                 C-1              C               C+1");

            for (int rowOffset = -1;
                 rowOffset <= 1;
                 rowOffset++)
            {
                int row =
                    Math.Clamp(
                        floorRow + rowOffset,
                        0,
                        Glo90RasterHeight - 1);

                Console.Write(
                    $"row={row,4}      ");

                for (int columnOffset = -1;
                     columnOffset <= 1;
                     columnOffset++)
                {
                    int column =
                        Math.Clamp(
                            floorColumn + columnOffset,
                            0,
                            Glo90RasterWidth - 1);

                    double value =
                        ReadPixel(
                            row,
                            column);

                    Console.Write(
                        $"{value,16:F9} ");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine(
                "Candidate pixels:");

            PrintCandidatePixel(
                "floor",
                floorRow,
                floorColumn,
                x,
                y,
                expected);

            PrintCandidatePixel(
                "row+1",
                floorRow + 1,
                floorColumn,
                x,
                y,
                expected);

            PrintCandidatePixel(
                "row-1",
                floorRow - 1,
                floorColumn,
                x,
                y,
                expected);

            PrintCandidatePixel(
                "column+1",
                floorRow,
                floorColumn + 1,
                x,
                y,
                expected);

            PrintCandidatePixel(
                "column-1",
                floorRow,
                floorColumn - 1,
                x,
                y,
                expected);

            int roundRow =
                Math.Clamp(
                    (int)Math.Round(
                        y,
                        MidpointRounding.ToEven),
                    0,
                    Glo90RasterHeight - 1);

            int roundColumn =
                Math.Clamp(
                    (int)Math.Round(
                        x,
                        MidpointRounding.ToEven),
                    0,
                    Glo90RasterWidth - 1);

            PrintCandidatePixel(
                "round",
                roundRow,
                roundColumn,
                x,
                y,
                expected);
        }

        private void PrintPixelGeoLocation(
            string name,
            int row,
            int column,
            GeoCoordinate target)
        {
            row =
                Math.Clamp(
                    row,
                    0,
                    Glo90RasterHeight - 1);

            column =
                Math.Clamp(
                    column,
                    0,
                    Glo90RasterWidth - 1);

            double topLeftLongitude =
                59.0 +
                column *
                Glo90PixelSize;

            double topLeftLatitude =
                37.0 -
                row *
                Glo90PixelSize;

            double centerLongitude =
                topLeftLongitude +
                Glo90PixelSize / 2.0;

            double centerLatitude =
                topLeftLatitude -
                Glo90PixelSize / 2.0;

            double value =
                ReadPixel(
                    row,
                    column);

            double cornerDistance =
                HaversineApproximationMeters(
                    target.Latitude,
                    target.Longitude,
                    topLeftLatitude,
                    topLeftLongitude);

            double centerDistance =
                HaversineApproximationMeters(
                    target.Latitude,
                    target.Longitude,
                    centerLatitude,
                    centerLongitude);

            Console.WriteLine();
            Console.WriteLine(
                $"--- {name} pixel geolocation ---");

            Console.WriteLine(
                $"Pixel                 : row={row}, column={column}");

            Console.WriteLine(
                $"Value                 : {value:F12} m");

            Console.WriteLine(
                $"Top-left coordinate   : " +
                $"{topLeftLatitude:F15}, {topLeftLongitude:F15}");

            Console.WriteLine(
                $"Pixel-center coord.   : " +
                $"{centerLatitude:F15}, {centerLongitude:F15}");

            Console.WriteLine(
                $"Target->top-left      : {cornerDistance:F6} m");

            Console.WriteLine(
                $"Target->center        : {centerDistance:F6} m");
        }

        private void PrintCandidatePixel(
            string name,
            int row,
            int column,
            double x,
            double y,
            double expected)
        {
            row =
                Math.Clamp(
                    row,
                    0,
                    Glo90RasterHeight - 1);

            column =
                Math.Clamp(
                    column,
                    0,
                    Glo90RasterWidth - 1);

            double value =
                ReadPixel(
                    row,
                    column);

            double rowDistance =
                Math.Abs(
                    y - row);

            double columnDistance =
                Math.Abs(
                    x - column);

            double expectedDelta =
                Math.Abs(
                    value - expected);

            Console.WriteLine(
                $"{name,-10} row={row,4} " +
                $"column={column,4} " +
                $"value={value,14:F9} " +
                $"|dy|={rowDistance:F9} " +
                $"|dx|={columnDistance:F9} " +
                $"|value-ref|={expectedDelta:F9}");
        }

        private void ValidateReferencePoints()
        {
            var coordinateA =
                new GeoCoordinate(
                    36.322331526431554,
                    59.57538704550899);

            var coordinateB =
                new GeoCoordinate(
                    36.32446807551602,
                    59.580907558046874);

            double expectedA =
                1001.2738037109375;

            double expectedB =
                999.364990234375;

            double pointA =
                ReadElevation(
                    coordinateA);

            double pointB =
                ReadElevation(
                    coordinateB);

            PrintPixelSelectionDiagnostic(
                "A",
                coordinateA,
                expectedA);

            PrintPixelSelectionDiagnostic(
                "B",
                coordinateB,
                expectedB);

            Console.WriteLine();
            Console.WriteLine(
                "===== GLO-90 REFERENCE VALUES =====");

            Console.WriteLine(
                $"Reference A expected  : {expectedA:F12}");

            Console.WriteLine(
                $"Reference A floor     : {pointA:F12}");

            Console.WriteLine(
                $"Reference B expected  : {expectedB:F12}");

            Console.WriteLine(
                $"Reference B floor     : {pointB:F12}");

            Assert.Equal(
                expectedA,
                pointA,
                precision: 6);

            Assert.Equal(
                expectedB,
                pointB,
                precision: 6);
        }

        private static string ReadRasterTypeDescription(
            byte[] file)
        {
            try
            {
                const ushort GeoKeyDirectoryTag = 34735;

                TiffIfdEntry? entry =
                    FindIfdEntry(
                        file,
                        GeoKeyDirectoryTag);

                if (entry is null)
                {
                    return
                        "GeoKeyDirectoryTag 34735 not found";
                }

                byte[] data =
                    ReadEntryData(
                        file,
                        entry.Value);

                if (data.Length < 8)
                {
                    return
                        "GeoKeyDirectoryTag 34735 is too short";
                }

                ushort version =
                    ReadUInt16(
                        data,
                        0);

                ushort revision =
                    ReadUInt16(
                        data,
                        2);

                ushort minorRevision =
                    ReadUInt16(
                        data,
                        4);

                ushort keyCount =
                    ReadUInt16(
                        data,
                        6);

                Console.WriteLine();
                Console.WriteLine(
                    "===== GLO-90 GEOTIFF GEOKEY DIRECTORY =====");

                Console.WriteLine(
                    $"Key directory version : {version}");

                Console.WriteLine(
                    $"Revision              : {revision}.{minorRevision}");

                Console.WriteLine(
                    $"Key count             : {keyCount}");

                for (int index = 0;
                     index < keyCount;
                     index++)
                {
                    int offset =
                        8 +
                        index * 8;

                    if (offset + 8 >
                        data.Length)
                    {
                        break;
                    }

                    ushort keyId =
                        ReadUInt16(
                            data,
                            offset);

                    ushort location =
                        ReadUInt16(
                            data,
                            offset + 2);

                    ushort count =
                        ReadUInt16(
                            data,
                            offset + 4);

                    ushort valueOffset =
                        ReadUInt16(
                            data,
                            offset + 6);

                    Console.WriteLine(
                        $"Key {keyId,-6} location={location,-6} " +
                        $"count={count,-6} value={valueOffset}");
                }

                for (int index = 0;
                     index < keyCount;
                     index++)
                {
                    int offset =
                        8 +
                        index * 8;

                    ushort keyId =
                        ReadUInt16(
                            data,
                            offset);

                    if (keyId != 1025)
                    {
                        continue;
                    }

                    ushort location =
                        ReadUInt16(
                            data,
                            offset + 2);

                    ushort count =
                        ReadUInt16(
                            data,
                            offset + 4);

                    ushort valueOffset =
                        ReadUInt16(
                            data,
                            offset + 6);

                    if (location == 0 &&
                        count == 1)
                    {
                        return valueOffset switch
                        {
                            1 =>
                                "PixelIsArea (GeoKey 1025 = 1)",

                            2 =>
                                "PixelIsPoint (GeoKey 1025 = 2)",

                            _ =>
                                $"Unknown RasterTypeGeoKey 1025 value={valueOffset}"
                        };
                    }
                }

                return
                    "RasterTypeGeoKey 1025 not found";
            }
            catch (Exception exception)
            {
                return
                    $"GeoKey parsing failed: {exception.GetType().Name}: {exception.Message}";
            }
        }

        private static void PrintGeoTiffMetadata(
            byte[] file)
        {
            Console.WriteLine();
            Console.WriteLine(
                "===== GLO-90 TIFF METADATA DIAGNOSTIC =====");

            if (file.Length < 8)
            {
                Console.WriteLine(
                    "TIFF file is too short.");

                return;
            }

            ushort byteOrder =
                BitConverter.ToUInt16(
                    file,
                    0);

            bool littleEndian =
                byteOrder == 0x4949;

            Console.WriteLine(
                $"Byte order            : " +
                $"{(littleEndian ? "little-endian" : "other")}");

            Console.WriteLine(
                $"TIFF magic             : " +
                $"{BitConverter.ToUInt16(file, 2)}");

            uint ifdOffset =
                BitConverter.ToUInt32(
                    file,
                    4);

            Console.WriteLine(
                $"First IFD offset       : {ifdOffset}");

            TiffIfdEntry? widthEntry =
                FindIfdEntry(
                    file,
                    256);

            TiffIfdEntry? heightEntry =
                FindIfdEntry(
                    file,
                    257);

            TiffIfdEntry? bitsEntry =
                FindIfdEntry(
                    file,
                    258);

            TiffIfdEntry? compressionEntry =
                FindIfdEntry(
                    file,
                    259);

            TiffIfdEntry? tileWidthEntry =
                FindIfdEntry(
                    file,
                    322);

            TiffIfdEntry? tileHeightEntry =
                FindIfdEntry(
                    file,
                    323);

            TiffIfdEntry? tileOffsetEntry =
                FindIfdEntry(
                    file,
                    324);

            TiffIfdEntry? tileByteCountEntry =
                FindIfdEntry(
                    file,
                    325);

            PrintScalarEntry(
                "ImageWidth 256",
                file,
                widthEntry);

            PrintScalarEntry(
                "ImageLength 257",
                file,
                heightEntry);

            PrintScalarEntry(
                "BitsPerSample 258",
                file,
                bitsEntry);

            PrintScalarEntry(
                "Compression 259",
                file,
                compressionEntry);

            PrintScalarEntry(
                "TileWidth 322",
                file,
                tileWidthEntry);

            PrintScalarEntry(
                "TileLength 323",
                file,
                tileHeightEntry);

            PrintScalarEntry(
                "TileOffsets 324",
                file,
                tileOffsetEntry);

            PrintScalarEntry(
                "TileByteCounts 325",
                file,
                tileByteCountEntry);
        }

        private static void PrintScalarEntry(
            string name,
            byte[] file,
            TiffIfdEntry? entry)
        {
            if (entry is null)
            {
                Console.WriteLine(
                    $"{name,-22}: not found");

                return;
            }

            try
            {
                byte[] data =
                    ReadEntryData(
                        file,
                        entry.Value);

                Console.WriteLine(
                    $"{name,-22}: type={entry.Value.Type}, " +
                    $"count={entry.Value.Count}, " +
                    $"bytes={data.Length}");

                if (data.Length <= 32)
                {
                    Console.WriteLine(
                        $"{"",-22}  " +
                        $"{Convert.ToHexString(data)}");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"{name,-22}: read failed: " +
                    $"{exception.GetType().Name}: {exception.Message}");
            }
        }

        private static TiffIfdEntry? FindIfdEntry(
            byte[] file,
            ushort tag)
        {
            if (file.Length < 8)
            {
                return null;
            }

            uint ifdOffset =
                BitConverter.ToUInt32(
                    file,
                    4);

            if (ifdOffset + 2 >
                file.Length)
            {
                return null;
            }

            ushort entryCount =
                BitConverter.ToUInt16(
                    file,
                    checked((int)ifdOffset));

            long entriesStart =
                ifdOffset +
                2;

            for (int index = 0;
                 index < entryCount;
                 index++)
            {
                long offset =
                    entriesStart +
                    index * 12L;

                if (offset + 12 >
                    file.LongLength)
                {
                    return null;
                }

                int position =
                    checked((int)offset);

                ushort currentTag =
                    BitConverter.ToUInt16(
                        file,
                        position);

                if (currentTag != tag)
                {
                    continue;
                }

                ushort type =
                    BitConverter.ToUInt16(
                        file,
                        position + 2);

                uint count =
                    BitConverter.ToUInt32(
                        file,
                        position + 4);

                uint valueOffset =
                    BitConverter.ToUInt32(
                        file,
                        position + 8);

                return new TiffIfdEntry(
                    currentTag,
                    type,
                    count,
                    valueOffset,
                    position + 8);
            }

            return null;
        }

        private static byte[] ReadEntryData(
            byte[] file,
            TiffIfdEntry entry)
        {
            int typeSize =
                entry.Type switch
                {
                    1 => 1,
                    2 => 1,
                    3 => 2,
                    4 => 4,
                    5 => 8,
                    12 => 8,
                    _ =>
                        throw new InvalidDataException(
                            $"Unsupported TIFF field type {entry.Type}.")
                };

            long byteCount =
                checked(
                    (long)typeSize *
                    entry.Count);

            if (byteCount <= 4)
            {
                byte[] result =
                    new byte[
                        checked((int)byteCount)];

                Buffer.BlockCopy(
                    file,
                    entry.InlineValueOffset,
                    result,
                    0,
                    result.Length);

                return result;
            }

            if (entry.ValueOffset > file.LongLength ||
                entry.ValueOffset + byteCount >
                    file.LongLength)
            {
                throw new InvalidDataException(
                    $"TIFF entry {entry.Tag} points outside the file.");
            }

            byte[] external =
                new byte[
                    checked((int)byteCount)];

            Buffer.BlockCopy(
                file,
                checked((int)entry.ValueOffset),
                external,
                0,
                external.Length);

            return external;
        }

        private static ushort ReadUInt16(
            byte[] data,
            int offset)
        {
            return BitConverter.ToUInt16(
                data,
                offset);
        }

        private static double HaversineApproximationMeters(
            double latitudeA,
            double longitudeA,
            double latitudeB,
            double longitudeB)
        {
            const double earthRadiusMeters =
                6371000.0;

            double latitudeARadians =
                latitudeA *
                Math.PI /
                180.0;

            double latitudeBRadians =
                latitudeB *
                Math.PI /
                180.0;

            double deltaLatitude =
                (latitudeB - latitudeA) *
                Math.PI /
                180.0;

            double deltaLongitude =
                (longitudeB - longitudeA) *
                Math.PI /
                180.0;

            double sinLatitude =
                Math.Sin(
                    deltaLatitude / 2.0);

            double sinLongitude =
                Math.Sin(
                    deltaLongitude / 2.0);

            double a =
                sinLatitude * sinLatitude +
                Math.Cos(latitudeARadians) *
                Math.Cos(latitudeBRadians) *
                sinLongitude *
                sinLongitude;

            double c =
                2.0 *
                Math.Atan2(
                    Math.Sqrt(a),
                    Math.Sqrt(
                        Math.Max(
                            0.0,
                            1.0 - a)));

            return
                earthRadiusMeters *
                c;
        }

        private readonly record struct TiffIfdEntry(
            ushort Tag,
            ushort Type,
            uint Count,
            uint ValueOffset,
            int InlineValueOffset);
    }
}