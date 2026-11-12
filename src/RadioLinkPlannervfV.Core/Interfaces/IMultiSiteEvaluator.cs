using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IMultiSiteEvaluator
{
    Task<MultiSiteAnalysisResult> AnalyzeAsync(
        Site hubSite,
        IReadOnlyList<Site> remoteSites,
        double frequencyGHz,
        CancellationToken cancellationToken = default);
}