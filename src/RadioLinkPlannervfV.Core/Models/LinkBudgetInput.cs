namespace RadioLinkPlannervfV.Core.Models;

public sealed record LinkBudgetInput
(
    double FrequencyGHz,
    double TransmitPowerDbm,
    double TransmitAntennaGainDbi,
    double ReceiveAntennaGainDbi,
    double CableLossDb,
    double OtherLossesDb,
    double ReceiverSensitivityDbm
);
