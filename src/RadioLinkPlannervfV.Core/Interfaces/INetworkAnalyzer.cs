using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface INetworkAnalyzer
{
    Task<NetworkAnalysisResult> AnalyzeAsync(
        NetworkAnalysisRequest request,
        CancellationToken cancellationToken = default);
}