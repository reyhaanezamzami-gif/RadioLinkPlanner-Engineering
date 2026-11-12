using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Calculators;
using RadioLinkPlannervfV.Terrain.Generators;
using RadioLinkPlannervfV.Terrain.Models;
using RadioLinkPlannervfV.Terrain.Optimizers;
using RadioLinkPlannervfV.Terrain.Providers;
using RadioLinkPlannervfV.Terrain.Readers;
using RadioLinkPlannervfV.Terrain.Services;
using RadioLinkPlannervfV.RF.Calculators;
using Xunit;

namespace RadioLinkPlannervfV.Terrain.Tests;

public sealed class MultiSiteNonFiniteDiagnosticTests
{
    [Fact]
    public async Task MultiSite_RealPipeline_ShouldIdentifyNonFiniteValues()
    {
        string demDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "SRTM");

        var elevationReader =
            new SrtmDemReader(
                new DemOptions
                {
                    DataDirectory = demDirectory
                });

        var offlineProvider =
            new OfflineTerrainProvider(
                elevationReader);

        var openMeteoProvider =
            new OpenMeteoTerrainElevationProvider(
                new HttpClient(),
                new OnlineTerrainOptions
                {
                    Enabled = true
                });

        var terrainProvider =
            new FallbackTerrainElevationProvider(
                new ITerrainElevationProvider[]
                {
                    offlineProvider,
                    openMeteoProvider
                });

        var distanceCalculator =
            new HaversineDistanceCalculator();

        var profileGenerator =
            new TerrainProfileGenerator(
                terrainProvider,
                distanceCalculator);

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

        var heightOptimizer =
            new HeightOptimizer(
                distanceCalculator,
                fresnelCalculator,
                earthCurvatureCalculator,
                settings);

        var analyzer =
            new LinkAnalyzer(
                distanceCalculator,
                fresnelCalculator,
                heightOptimizer,
                earthCurvatureCalculator,
                settings);

        var hub =
            new Site(
                "Semnan-East-Hub",
                new GeoCoordinate(35.5700, 53.3900),
                0,
                40);

        var remotes =
            new[]
            {
                new Site(
                    "Semnan-Site-A",
                    new GeoCoordinate(35.5800, 53.4000),
                    0,
                    30),

                new Site(
                    "Semnan-Site-B",
                    new GeoCoordinate(35.5900, 53.4100),
                    0,
                    30)
            };

        foreach (Site remote in remotes)
        {
            TerrainProfileResult profile =
                await profileGenerator.GenerateAsync(
                    hub.Coordinate,
                    remote.Coordinate,
                    100);

            Link link =
                new(
                    hub,
                    remote,
                    18);

            LinkAnalysisResult result =
                analyzer.Analyze(
                    link,
                    profile.Points);

            Console.WriteLine(
                $"===== {remote.Name} =====");

            Console.WriteLine(
                $"TerrainSource = {profile.Source}");

            PrintValue(
                nameof(result.DistanceMeters),
                result.DistanceMeters);

            PrintValue(
                nameof(result.FreeSpacePathLossDb),
                result.FreeSpacePathLossDb);

            PrintValue(
                nameof(result.FirstFresnelZoneRadiusMeters),
                result.FirstFresnelZoneRadiusMeters);

            PrintValue(
                nameof(result.RequiredClearanceMeters),
                result.RequiredClearanceMeters);

            PrintValue(
                nameof(result.MinimumAntennaHeightASLMeters),
                result.MinimumAntennaHeightASLMeters);

            PrintValue(
                nameof(result.RecommendedAntennaHeightASiteAMeters),
                result.RecommendedAntennaHeightASiteAMeters);

            PrintValue(
                nameof(result.RecommendedAntennaHeightBSiteBMeters),
                result.RecommendedAntennaHeightBSiteBMeters);

            PrintValue(
                nameof(result.MaximumTerrainElevationMeters),
                result.MaximumTerrainElevationMeters);

            PrintValue(
                nameof(result.MaximumTerrainClearanceDeficitMeters),
                result.MaximumTerrainClearanceDeficitMeters);

            if (result.HeightOptimization is not null)
            {
                PrintValue(
                    "HeightOptimization." +
                    nameof(result.HeightOptimization.OptimizedAntennaHeightASiteAMeters),
                    result.HeightOptimization.OptimizedAntennaHeightASiteAMeters);

                PrintValue(
                    "HeightOptimization." +
                    nameof(result.HeightOptimization.OptimizedAntennaHeightBSiteBMeters),
                    result.HeightOptimization.OptimizedAntennaHeightBSiteBMeters);

                PrintValue(
                    "HeightOptimization." +
                    nameof(result.HeightOptimization.TotalAdditionalHeightMeters),
                    result.HeightOptimization.TotalAdditionalHeightMeters);
            }

            if (result.WorstTerrainObstruction is not null)
            {
                PrintValue(
                    "WorstTerrainObstruction." +
                    nameof(result.WorstTerrainObstruction.DistanceFromSiteAMeters),
                    result.WorstTerrainObstruction.DistanceFromSiteAMeters);

                PrintValue(
                    "WorstTerrainObstruction." +
                    nameof(result.WorstTerrainObstruction.TerrainElevationMeters),
                    result.WorstTerrainObstruction.TerrainElevationMeters);

                PrintValue(
                    "WorstTerrainObstruction." +
                    nameof(result.WorstTerrainObstruction.TerrainElevationWithCurvatureMeters),
                    result.WorstTerrainObstruction.TerrainElevationWithCurvatureMeters);

                PrintValue(
                    "WorstTerrainObstruction." +
                    nameof(result.WorstTerrainObstruction.LineOfSightElevationMeters),
                    result.WorstTerrainObstruction.LineOfSightElevationMeters);

                PrintValue(
                    "WorstTerrainObstruction." +
                    nameof(result.WorstTerrainObstruction.ObstructionAmountMeters),
                    result.WorstTerrainObstruction.ObstructionAmountMeters);
            }

            if (result.TerrainProfileAnalysis is not null)
            {
                for (int index = 0;
                     index < result.TerrainProfileAnalysis.Count;
                     index++)
                {
                    TerrainProfileAnalysisPoint point =
                        result.TerrainProfileAnalysis[index];

                    CheckProfileValue(
                        remote.Name,
                        index,
                        nameof(point.TerrainElevationMeters),
                        point.TerrainElevationMeters);

                    CheckProfileValue(
                        remote.Name,
                        index,
                        nameof(point.TerrainElevationWithCurvatureMeters),
                        point.TerrainElevationWithCurvatureMeters);

                    CheckProfileValue(
                        remote.Name,
                        index,
                        nameof(point.LineOfSightElevationMeters),
                        point.LineOfSightElevationMeters);

                    CheckProfileValue(
                        remote.Name,
                        index,
                        nameof(point.FirstFresnelZoneRadiusMeters),
                        point.FirstFresnelZoneRadiusMeters);

                    CheckProfileValue(
                        remote.Name,
                        index,
                        nameof(point.RequiredFresnelClearanceMeters),
                        point.RequiredFresnelClearanceMeters);

                    CheckProfileValue(
                        remote.Name,
                        index,
                        nameof(point.ActualClearanceMeters),
                        point.ActualClearanceMeters);

                    CheckProfileValue(
                        remote.Name,
                        index,
                        nameof(point.ClearanceDeficitMeters),
                        point.ClearanceDeficitMeters);
                }
            }
        }
    }

    private static void PrintValue(
        string name,
        double value)
    {
        Console.WriteLine(
            $"{name} = {value:R} | Finite={double.IsFinite(value)}");
    }

    private static void CheckProfileValue(
        string site,
        int index,
        string name,
        double value)
    {
        if (!double.IsFinite(value))
        {
            Console.WriteLine(
                $"NON_FINITE: Site={site}, Index={index}, {name}={value:R}");
        }
    }
}

