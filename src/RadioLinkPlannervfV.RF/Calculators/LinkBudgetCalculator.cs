using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.RF.Calculators;

public sealed class LinkBudgetCalculator : ILinkBudgetCalculator
{
    public LinkBudgetResult Calculate(
        LinkBudgetInput input,
        double distanceMeters)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (distanceMeters <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distanceMeters));
        }

        if (input.FrequencyGHz <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.FrequencyGHz));
        }

        double distanceKilometers =
            distanceMeters / 1000.0;

        double frequencyMHz =
            input.FrequencyGHz * 1000.0;

        double freeSpacePathLossDb =
            32.44 +
            20.0 * Math.Log10(distanceKilometers) +
            20.0 * Math.Log10(frequencyMHz);

        double receivedPowerDbm =
            input.TransmitPowerDbm +
            input.TransmitAntennaGainDbi +
            input.ReceiveAntennaGainDbi -
            freeSpacePathLossDb -
            input.CableLossDb -
            input.OtherLossesDb;

        double fadeMarginDb =
            receivedPowerDbm -
            input.ReceiverSensitivityDbm;

        bool isLinkViable =
            fadeMarginDb >= 0.0;

        return new LinkBudgetResult(
            freeSpacePathLossDb,
            receivedPowerDbm,
            fadeMarginDb,
            isLinkViable);
    }
}
