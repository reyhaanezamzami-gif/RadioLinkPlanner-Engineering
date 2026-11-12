using RadioLinkPlannervfV.Core.Models;
using Xunit;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class MultiSiteEvaluatorTests
{
    [Fact]
    public void HubSiteRequest_ShouldContainSelectedHubAndRemoteSites()
    {
        Site hub =
            new Site(
                "Hub-East-Semnan",
                new GeoCoordinate(
                    35.5700,
                    53.3900),
                1200,
                40);


        Site siteA =
            new Site(
                "Site-A",
                new GeoCoordinate(
                    35.5800,
                    53.4000),
                1210,
                30);


        Site siteB =
            new Site(
                "Site-B",
                new GeoCoordinate(
                    35.5900,
                    53.4100),
                1195,
                30);


        MultiSiteAnalysisRequest request =
            new MultiSiteAnalysisRequest(
                hub,
                new[]
                {
                    siteA,
                    siteB
                },
                18);


        Assert.Equal(
            "Hub-East-Semnan",
            request.HubSite.Name);


        Assert.Equal(
            2,
            request.RemoteSites.Count);


        Assert.Equal(
            18,
            request.FrequencyGHz);
    }
}