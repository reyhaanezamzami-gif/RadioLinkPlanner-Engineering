namespace RadioLinkPlannervfV.Core.Models;

public sealed record LinkAnalysisReport
(
    string ReportId,
    string ProjectName,
    DateTimeOffset CreatedAt,
    LinkAnalysisResult LinkAnalysis,
    LinkBudgetInput LinkBudgetInput,
    LinkBudgetResult LinkBudget
);