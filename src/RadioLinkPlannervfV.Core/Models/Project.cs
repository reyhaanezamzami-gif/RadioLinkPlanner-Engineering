namespace RadioLinkPlannervfV.Core.Models;

public sealed record Project
(
    string Name,
    string CompanyName,
    Site SiteA,
    Site SiteB,
    RadioParameters RadioParameters
);