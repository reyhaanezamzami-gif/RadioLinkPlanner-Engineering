using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface ILinkBudgetCalculator
{
    LinkBudgetResult Calculate(
        LinkBudgetInput input,
        double distanceMeters);
}
