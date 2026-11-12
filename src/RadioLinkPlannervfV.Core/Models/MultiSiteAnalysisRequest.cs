namespace RadioLinkPlannervfV.Core.Models;

public sealed record MultiSiteAnalysisRequest
(
    Site HubSite,
    IReadOnlyList<Site> RemoteSites,
    double FrequencyGHz
);