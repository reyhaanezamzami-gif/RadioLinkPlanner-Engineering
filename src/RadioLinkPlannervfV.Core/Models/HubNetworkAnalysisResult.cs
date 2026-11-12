namespace RadioLinkPlannervfV.Core.Models;

public sealed record HubNetworkAnalysisResult
(
    NetworkHub Hub,
    IReadOnlyList<SiteAnalysisResult> LinkResults
);