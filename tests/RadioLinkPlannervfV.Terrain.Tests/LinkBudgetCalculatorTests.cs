
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.RF.Calculators;

namespace RadioLinkPlannervfV.Terrain.Tests;

public class LinkBudgetCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsExpectedLinkBudget()
    {
        var calculator =
            new LinkBudgetCalculator();

        var input =
            new LinkBudgetInput(
                FrequencyGHz: 6.0,
                TransmitPowerDbm: 20.0,
                TransmitAntennaGainDbi: 30.0,
                ReceiveAntennaGainDbi: 30.0,
                CableLossDb: 2.0,
                OtherLossesDb: 1.0,
                ReceiverSensitivityDbm: -70.0);

        LinkBudgetResult result =
            calculator.Calculate(
                input,
                10_000.0);

        Assert.True(
            result.FreeSpacePathLossDb > 0.0);

        Assert.True(
            result.ReceivedPowerDbm < 60.0);

        Assert.Equal(
            result.ReceivedPowerDbm -
            input.ReceiverSensitivityDbm,
            result.FadeMarginDb,
            10);

        Assert.Equal(
            result.FadeMarginDb >= 0.0,
            result.IsLinkViable);
    }

    [Fact]
    public void Calculate_ShouldReturnExpectedNumericalValues()
    {
        var calculator =
            new LinkBudgetCalculator();

        var input =
            new LinkBudgetInput(
                FrequencyGHz: 6.0,
                TransmitPowerDbm: 20.0,
                TransmitAntennaGainDbi: 30.0,
                ReceiveAntennaGainDbi: 30.0,
                CableLossDb: 2.0,
                OtherLossesDb: 1.0,
                ReceiverSensitivityDbm: -70.0);

        LinkBudgetResult result =
            calculator.Calculate(
                input,
                10_000.0);

        Assert.Equal(
            128.003025007673,
            result.FreeSpacePathLossDb,
            precision: 10);

        Assert.Equal(
            -51.003025007673,
            result.ReceivedPowerDbm,
            precision: 10);

        Assert.Equal(
            18.996974992327,
            result.FadeMarginDb,
            precision: 10);

        Assert.True(
            result.IsLinkViable);
    }

    [Fact]
    public void Calculate_InvalidDistance_Throws()
    {
        var calculator =
            new LinkBudgetCalculator();

        var input =
            new LinkBudgetInput(
                6.0,
                20.0,
                30.0,
                30.0,
                2.0,
                1.0,
                -70.0);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                calculator.Calculate(
                    input,
                    0.0));
    }

    [Fact]
    public void Calculate_InvalidFrequency_Throws()
    {
        var calculator =
            new LinkBudgetCalculator();

        var input =
            new LinkBudgetInput(
                0.0,
                20.0,
                30.0,
                30.0,
                2.0,
                1.0,
                -70.0);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                calculator.Calculate(
                    input,
                    10_000.0));
    }
}

