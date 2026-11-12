using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Services;
using Xunit;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class MultiSiteEvaluatorExecutionTests
{
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
                        new GeoCoordinate(
                            start.Latitude +
                            (end.Latitude - start.Latitude) *
                            i / (sampleCount - 1),

                            start.Longitude +
                            (end.Longitude - start.Longitude) *
                            i / (sampleCount - 1)),

                        i * 1000,
                        1000));
            }

            return Task.FromResult(
                new TerrainProfileResult(
                    points,
                    "Fake"));
        }
    }


    [Fact]
    public async Task MultiSiteEvaluator_ShouldAnalyze_AllLinks_FromHub()
    {
        var evaluator =
            new MultiSiteEvaluator(
                new FakeTerrainProfileGenerator(),
                new FakeLinkAnalyzer());


        var hub =
            new Site(
                "Hub-Semnan",
                new GeoCoordinate(
                    35.57,
                    53.39),
                1000,
                40);


        var sites =
            new[]
            {
                new Site(
                    "Site-A",
                    new GeoCoordinate(
                        35.58,
                        53.40),
                    1000,
                    30),

                new Site(
                    "Site-B",
                    new GeoCoordinate(
                        35.59,
                        53.41),
                    1000,
                    30)
            };


        MultiSiteAnalysisResult result =
            await evaluator.AnalyzeAsync(
                hub,
                sites,
                18);


        Assert.Equal(
            2,
            result.SiteResults.Count);
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
                1000,
                30,
                30,
                1000,
                0,
                RadioLinkPlannervfV.Core.Enums.LinkStatus.LineOfSight,
                null,
                null,
                1000,
                1000,
                Array.Empty<TerrainProfileAnalysisPoint>());
        }
    }
}