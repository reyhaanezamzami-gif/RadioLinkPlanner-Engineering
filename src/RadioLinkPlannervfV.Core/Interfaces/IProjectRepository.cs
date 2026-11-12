using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IProjectRepository
{
    Task SaveAsync(
        Project project,
        CancellationToken cancellationToken = default);

    Task<Project?> LoadAsync(
        string projectName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListAsync(
        CancellationToken cancellationToken = default);
}