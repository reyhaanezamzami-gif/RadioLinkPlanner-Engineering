using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IReportRepository
{
    Task SaveAsync(
        LinkAnalysisReport report,
        CancellationToken cancellationToken = default);

    Task<LinkAnalysisReport?> LoadAsync(
        string reportId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LinkAnalysisReport>> ListAsync(
        string projectName,
        CancellationToken cancellationToken = default);
}