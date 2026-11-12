using RadioLinkPlannervfV.Core.Enums;
using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Services;
using Xunit;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class MultiSiteEvaluationIntegrationTests
{
    [Fact]
    public async Task Hub_ShouldAnalyze_AllRemoteSites()
    {
        Site hub =
            new(
                "Semnan-East-Hub",
                new GeoCoordinate(35.5700, 53.3900),
                1200,
                40);


        Site remoteA =
            new(
                "Semnan-Site-A",
                new GeoCoordinate(35.5800, 53.4000),
                1210,
                30);


        Site remoteB =
            new(
                "Semnan-Site-B",
                new GeoCoordinate(35.5900, 53.4100),
                1195,
                30);


        var evaluator =
            new MultiSiteEvaluator(
                new FakeTerrainProfileGenerator(),
                new FakeLinkAnalyzer());


        MultiSiteAnalysisResult result =
            await evaluator.AnalyzeAsync(
                hub,
                new[]
                {
                    remoteA,
                    remoteB
                },
                18);


        Assert.NotNull(result);

        Assert.Equal(
            hub,
            result.HubSite);


        Assert.Equal(
            2,
            result.SiteResults.Count);


        foreach (SiteAnalysisResult siteResult in result.SiteResults)
        {
            Assert.NotNull(siteResult);

            Assert.Equal(
                LinkStatus.LineOfSight,
                siteResult.AnalysisResult.Status);


            Assert.Equal(
                1000,
                siteResult.AnalysisResult.MaximumTerrainElevationMeters);


            Assert.NotNull(
                siteResult.AnalysisResult.TerrainProfileAnalysis);
        }
    }


    private sealed class FakeTerrainProfileGenerator :
        ITerrainProfileGenerator
    {
        public Task<TerrainProfileResult> GenerateAsync(
            GeoCoordinate start,
            GeoCoordinate end,
            int sampleCount,
            CancellationToken cancellationToken = default)
        {
            var points =
                new List<TerrainPoint>();

            for (int i = 0; i < sampleCount; i++)
            {
                points.Add(
                    new TerrainPoint(
                        start,
                        i,
                        1000));
            }


            return Task.FromResult(
                new TerrainProfileResult(
                    points,
                    "Fake"));
        }
    }


    private sealed class FakeLinkAnalyzer :
        ILinkAnalyzer
    {
        public LinkAnalysisResult Analyze(
            Link link,
            IReadOnlyList<TerrainPoint> terrainProfile)
        {
            return new LinkAnalysisResult(
                1000,
                100,
                10,
                6,
                1200,
                30,
                30,
                1000,
                0,
                LinkStatus.LineOfSight,
                new HeightOptimizationResult(
                    30,
                    30,
                    0,
                    true),
                null,
                link.SiteA.GroundElevationMeters,
                link.SiteB.GroundElevationMeters,
                new List<TerrainProfileAnalysisPoint>
                {
                    new(
                        link.SiteA.Coordinate,
                        0,
                        1000,
                        1000,
                        1200,
                        10,
                        6,
                        6,
                        0)
                });
        }
    }
}