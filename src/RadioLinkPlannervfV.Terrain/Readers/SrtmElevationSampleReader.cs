namespace RadioLinkPlannervfV.Terrain.Readers;

public static class SrtmElevationSampleReader
{
    public const short VoidElevationValue = -32768;

    public static short ReadSample(
        Stream stream,
        long byteOffset)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new InvalidOperationException(
                "Stream must support reading.");
        }

        if (!stream.CanSeek)
        {
            throw new InvalidOperationException(
                "Stream must support seeking.");
        }

        if (byteOffset < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset));
        }

        stream.Seek(
            byteOffset,
            SeekOrigin.Begin);

        int highByte =
            stream.ReadByte();

        int lowByte =
            stream.ReadByte();

        if (highByte < 0 || lowByte < 0)
        {
            throw new EndOfStreamException(
                "Unable to read SRTM elevation sample.");
        }

        short elevation =
            unchecked(
                (short)(
                    (highByte << 8) |
                    lowByte));

        return elevation;
    }
}
