namespace RadioLinkPlannervfV.Core.Models;

public sealed record HubNetworkAnalysisRequest
(
    NetworkHub Hub,
    IReadOnlyList<Site> RemoteSites,
    double FrequencyGHz
);