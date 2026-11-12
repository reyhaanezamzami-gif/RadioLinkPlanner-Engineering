namespace RadioLinkPlannervfV.Core.Models;

public sealed record Link
(
    Site SiteA,
    Site SiteB,
    double FrequencyGHz
);
