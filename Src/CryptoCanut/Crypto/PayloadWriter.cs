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

        file.Refresh(); // to make sure the size is up to date
        var fileLength = file.Length;
        Span<byte> payloadLengthBytes = stackalloc byte[8];
        BitConverter.TryWriteBytes(payloadLengthBytes, fileLength);

        stream.WriteByte(FileVersion);
        stream.Write(fileNameLengthBytes);
        stream.Write(payloadLengthBytes);
        stream.Write(MagicString);
        stream.Write(fileNameBytes);

        using (var fileStream = file.OpenRead())
            fileStream.CopyTo(stream);

        long padLength = ComputePadingLength(fileLength);
        PadStream(stream, padLength);
    }

    /// <summary>
    /// Computes the number of additional bytes required to pad a payload.
    /// </summary>
    /// <param name="payloadLength">the payload length</param>
    /// <param name="maxPaddingFactor">the aproximate factor by wich te payload is alowed to grow in size</param>
    /// <returns>the number of bytes to add to the payload</returns>
    private static long ComputePadingLength(long payloadLength, double maxPaddingFactor = 1.2)
    {
        var logLength = Math.Log(payloadLength, maxPaddingFactor);
        var paddedSize = (long)Math.Pow(maxPaddingFactor, Math.Ceiling(logLength));
        return paddedSize - payloadLength;
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
