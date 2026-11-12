using RadioLinkPlannervfV.Core.Interfaces;
using RadioLinkPlannervfV.Core.Models;
using RadioLinkPlannervfV.Terrain.Models;

namespace RadioLinkPlannervfV.Terrain.Readers;

public sealed class SrtmDemReader : IDemReader
{
    private readonly DemOptions _options;

    public SrtmDemReader(
        DemOptions options)
    {
        _options = options ??
            throw new ArgumentNullException(
                nameof(options));

        if (string.IsNullOrWhiteSpace(
            _options.DataDirectory))
        {
            throw new ArgumentException(
                "DEM data directory cannot be empty.",
                nameof(options));
        }
    }

    public TerrainElevationResult ReadElevation(
        GeoCoordinate coordinate)
    {
        string fileName =
            SrtmFileNameResolver.GetFileName(
                coordinate);

        string filePath =
            Path.Combine(
                _options.DataDirectory,
                fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"SRTM DEM file was not found: {filePath}",
                filePath);
        }

        var fileInfo =
            new FileInfo(filePath);

        int samplesPerSide =
            SrtmTileMetadata.GetSamplesPerSide(
                fileInfo.Length);

        var position =
            SrtmCoordinateMapper.GetRowAndColumn(
                coordinate,
                samplesPerSide);

        long sampleIndex =
            (long)position.Row *
            samplesPerSide +
            position.Column;

        long byteOffset =
            sampleIndex *
            SrtmTileMetadata.BytesPerSample;

        using var stream =
            new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        short elevation =
            SrtmElevationSampleReader.ReadSample(
                stream,
                byteOffset);

        if (elevation ==
            SrtmElevationSampleReader.VoidElevationValue)
        {
            throw new InvalidDataException(
                $"SRTM elevation data is void at coordinate " +
                $"{coordinate.Latitude}, {coordinate.Longitude}.");
        }

        return new TerrainElevationResult(
            elevation,
            "SRTM");
    }
}
