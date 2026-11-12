namespace RadioLinkPlannervfV.Core.Models;

public sealed record SiteAnalysisResult
(
    Site Site,
    LinkAnalysisResult AnalysisResult
);