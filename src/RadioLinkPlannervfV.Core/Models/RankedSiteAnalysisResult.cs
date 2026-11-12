using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Models;

public sealed record RankedSiteAnalysisResult
(
    int Rank,
    SiteAnalysisResult SiteResult
);
