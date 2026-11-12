namespace RadioLinkPlannervfV.Core.ValueObjects;

public readonly record struct Distance
(
    double Meters
)
{
    public double Kilometers => Meters / 1000.0;

    public static Distance FromMeters(double meters) => new(meters);

    public static Distance FromKilometers(double kilometers) => new(kilometers * 1000.0);
}
