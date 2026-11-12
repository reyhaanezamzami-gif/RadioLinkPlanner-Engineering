using RadioLinkPlannervfV.Core.Enums;
using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Services;

public sealed class MultiSiteRankingService :
    IMultiSiteRankingService
{
    public IReadOnlyList<RankedSiteAnalysisResult> Rank(
        IReadOnlyList<SiteAnalysisResult> siteResults)
    {
        ArgumentNullException.ThrowIfNull(siteResults);

        List<SiteAnalysisResult> orderedResults =
            siteResults
                .OrderBy(GetStatusRank)
                .ThenBy(result =>
                    result.AnalysisResult
                        .MinimumAntennaHeightASLMeters)
                .ThenBy(result =>
                    result.AnalysisResult
                        .MaximumTerrainClearanceDeficitMeters)
                .ThenBy(result =>
                    result.AnalysisResult
                        .FreeSpacePathLossDb)
                .ThenBy(result =>
                    result.AnalysisResult
                        .DistanceMeters)
                .ToList();

        return orderedResults
            .Select(
                (siteResult, index) =>
                    new RankedSiteAnalysisResult(
                        index + 1,
                        siteResult))
            .ToList();
    }

    private static int GetStatusRank(
        SiteAnalysisResult siteResult)
    {
        return siteResult.AnalysisResult.Status switch
        {
            LinkStatus.LineOfSight => 0,
            LinkStatus.FresnelViolation => 1,
            LinkStatus.Obstructed => 2,
            LinkStatus.Unknown => 3,
            _ => 4
        };
    }
}
