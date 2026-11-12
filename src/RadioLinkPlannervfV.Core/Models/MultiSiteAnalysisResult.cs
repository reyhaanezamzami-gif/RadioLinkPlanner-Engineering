namespace RadioLinkPlannervfV.Core.Models;

public sealed record MultiSiteAnalysisResult
(
    Site HubSite,
    IReadOnlyList<SiteAnalysisResult> SiteResults
);