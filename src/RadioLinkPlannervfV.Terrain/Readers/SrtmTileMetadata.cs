namespace RadioLinkPlannervfV.Terrain.Readers;

public static class SrtmTileMetadata
{
    public const int BytesPerSample = 2;

    public static int GetSamplesPerSide(
        long fileLengthBytes)
    {
        if (fileLengthBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileLengthBytes));
        }

        double sampleCount =
            Math.Sqrt(
                fileLengthBytes /
                (double)BytesPerSample);

        int samplesPerSide =
            (int)Math.Round(sampleCount);

        long expectedLength =
            (long)samplesPerSide *
            samplesPerSide *
            BytesPerSample;

        if (expectedLength != fileLengthBytes)
        {
            throw new InvalidDataException(
                "Invalid SRTM HGT file size.");
        }

        return samplesPerSide;
    }
}
