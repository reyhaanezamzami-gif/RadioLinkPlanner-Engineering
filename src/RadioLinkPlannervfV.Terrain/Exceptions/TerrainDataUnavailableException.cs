using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Terrain.Exceptions;

public sealed class TerrainDataUnavailableException
    : Exception
{
    public GeoCoordinate Coordinate { get; }

    public IReadOnlyList<Exception> Errors { get; }


    public TerrainDataUnavailableException(
        GeoCoordinate coordinate,
        IReadOnlyList<Exception> errors)
        : base(
            $"Terrain data unavailable for coordinate {coordinate.Latitude},{coordinate.Longitude}.",
            new AggregateException(
                errors ??
                Array.Empty<Exception>()))
    {
        Coordinate =
            coordinate;

        Errors =
            errors ??
            Array.Empty<Exception>();
    }
}