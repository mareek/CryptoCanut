using System.Text;

namespace CryptoCanut.Crypto;

internal class PayloadWriter
{
    /* Payload structure 
     *  1 byte  : version
     *  2 bytes : file name length
     *  8 bytes : payload length
     * 11 bytes : "CRYPTOCANUT" magic ASCII string
     *  N bytes : file name in UTF-8
     *  N bytes : padding
     */

    private const byte FileVersion = 1;
    private static readonly byte[] MagicString = [67, 82, 89, 80, 84, 79, 67, 65, 78, 85, 84]; //"CRYPTOCANUT"

    public void WriteToStream(Stream stream, FileInfo file)
    {
        var fileNameBytes = Encoding.UTF8.GetBytes(file.Name);
        if (fileNameBytes.Length > ushort.MaxValue)
            throw new ArgumentException("file name too long", nameof(file));

        Span<byte> fileNameLengthBytes = stackalloc byte[2];
        BitConverter.TryWriteBytes(fileNameLengthBytes, (ushort)fileNameBytes.Length);

        Span<byte> payloadLengthBytes = stackalloc byte[8];
        BitConverter.TryWriteBytes(payloadLengthBytes, file.Length);

        stream.WriteByte(FileVersion);
        stream.Write(fileNameLengthBytes);
        stream.Write(payloadLengthBytes);
        stream.Write(MagicString);
        stream.Write(fileNameBytes);

        using (var fileStream = file.OpenRead())
            fileStream.CopyTo(stream);

        PadStream(stream, ComputePadingLength(file.Length));
    }

    private static long ComputePadingLength(long payloadLength)
    {
        const int minPowerOf2 = 11;
        const int maxPowerOf2 = 63;

        for (int i = minPowerOf2; i < maxPowerOf2; i++)
        {
            var targetLength = (long)Math.Pow(2, i);
            if (payloadLength < targetLength)
                return targetLength - payloadLength;
        }

        throw new ArgumentException("file is too big", nameof(payloadLength));
    }

    private static void PadStream(Stream stream, long padLength)
    {
        const int bufferSize = 256;
        Span<byte> buffer = stackalloc byte[bufferSize];
        for (int i = 0; (i + bufferSize) < padLength; i += bufferSize)
        {
            Random.Shared.NextBytes(buffer);
            stream.Write(buffer);
        }

        Span<byte> remainder = buffer[..(int)(padLength % bufferSize)];
        Random.Shared.NextBytes(remainder);
        stream.Write(remainder);
    }
}
