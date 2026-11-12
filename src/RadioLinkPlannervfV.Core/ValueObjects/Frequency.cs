namespace RadioLinkPlannervfV.Core.ValueObjects;

public readonly record struct Frequency
(
    double GHz
)
{
    public double MHz => GHz * 1000.0;

    public double Hz => GHz * 1_000_000_000.0;

    public static Frequency FromGHz(double ghz) => new(ghz);

    public static Frequency FromMHz(double mhz) => new(mhz / 1000.0);
}
