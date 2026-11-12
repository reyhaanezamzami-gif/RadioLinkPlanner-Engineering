using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IMultiSiteRankingService
{
    IReadOnlyList<RankedSiteAnalysisResult> Rank(
        IReadOnlyList<SiteAnalysisResult> siteResults);
}
