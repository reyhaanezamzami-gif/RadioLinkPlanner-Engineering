namespace RadioLinkPlannervfV.Core.Models;

public sealed record NetworkLinkAnalysisResult
(
    Site HubSite,
    Site RemoteSite,
    LinkAnalysisResult AnalysisResult
);