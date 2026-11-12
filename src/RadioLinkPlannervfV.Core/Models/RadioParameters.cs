namespace RadioLinkPlannervfV.Core.Models;

public sealed record RadioParameters
(
    double TransmitPowerDbm,
    double TransmitAntennaGainDbi,
    double ReceiveAntennaGainDbi,
    double TransmitCableLossDb,
    double ReceiveCableLossDb,
    double OtherLossesDb,
    double ReceiverSensitivityDbm,
    double FrequencyGHz
);