using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IHubNetworkAnalyzer
{
    Task<HubNetworkAnalysisResult> AnalyzeAsync(
        HubNetworkAnalysisRequest request,
        CancellationToken cancellationToken = default);
}