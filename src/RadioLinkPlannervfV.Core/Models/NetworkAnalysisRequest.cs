namespace RadioLinkPlannervfV.Core.Models;

public sealed record NetworkAnalysisRequest
(
    Site HubSite,
    IReadOnlyList<Site> RemoteSites,
    double FrequencyGHz
);