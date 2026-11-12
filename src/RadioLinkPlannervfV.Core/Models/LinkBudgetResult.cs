namespace RadioLinkPlannervfV.Core.Models;

public sealed record LinkBudgetResult
(
    double FreeSpacePathLossDb,
    double ReceivedPowerDbm,
    double FadeMarginDb,
    bool IsLinkViable
);
