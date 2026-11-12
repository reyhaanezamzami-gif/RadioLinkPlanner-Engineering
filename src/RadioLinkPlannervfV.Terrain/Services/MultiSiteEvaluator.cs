using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Services;

public sealed class MultiSiteEvaluator : IMultiSiteEvaluator
{
    private readonly ITerrainProfileGenerator _terrainProfileGenerator;
    private readonly ILinkAnalyzer _linkAnalyzer;

    public MultiSiteEvaluator(
        ITerrainProfileGenerator terrainProfileGenerator,
        ILinkAnalyzer linkAnalyzer)
    {
        _terrainProfileGenerator =
            terrainProfileGenerator ??
            throw new ArgumentNullException(nameof(terrainProfileGenerator));

        _linkAnalyzer =
            linkAnalyzer ??
            throw new ArgumentNullException(nameof(linkAnalyzer));
    }

    public async Task<MultiSiteAnalysisResult> AnalyzeAsync(
        Site hubSite,
        IReadOnlyList<Site> remoteSites,
        double frequencyGHz,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hubSite);
        ArgumentNullException.ThrowIfNull(remoteSites);

        var results =
            new List<SiteAnalysisResult>();

        foreach (Site remoteSite in remoteSites)
        {
            TerrainProfileResult profile =
                await _terrainProfileGenerator.GenerateAsync(
                    hubSite.Coordinate,
                    remoteSite.Coordinate,
                    100,
                    cancellationToken);

            if (profile.Points.Count < 2)
            {
                throw new InvalidOperationException(
                    "Terrain profile must contain at least two points.");
            }

            TerrainPoint firstTerrainPoint =
                profile.Points[0];

            TerrainPoint lastTerrainPoint =
                profile.Points[^1];

            Site siteA =
                new(
                    hubSite.Name,
                    hubSite.Coordinate,
                    firstTerrainPoint.ElevationMeters,
                    hubSite.AntennaHeightMeters);

            Site siteB =
                new(
                    remoteSite.Name,
                    remoteSite.Coordinate,
                    lastTerrainPoint.ElevationMeters,
                    remoteSite.AntennaHeightMeters);

            Link link =
                new(
                    siteA,
                    siteB,
                    frequencyGHz);

            LinkAnalysisResult analysis =
                _linkAnalyzer.Analyze(
                    link,
                    profile.Points);

            results.Add(
                new SiteAnalysisResult(
                    remoteSite,
                    analysis));
        }

        return new MultiSiteAnalysisResult(
            hubSite,
            results);
    }
}
