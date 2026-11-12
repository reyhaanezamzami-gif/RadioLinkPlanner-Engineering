namespace RadioLinkPlannervfV.Core.Models;

public sealed record NetworkAnalysisResult
(
    Site HubSite,
    IReadOnlyList<NetworkLinkAnalysisResult> Links
);