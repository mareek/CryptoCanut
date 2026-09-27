using System.Text;

namespace CryptoCanut.Crypto;

internal class PayloadReader
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

    public bool TryExtractPayload(DirectoryInfo outputDirectory, Stream inputStream)
    {
        if (!TryReadHeader(inputStream, out var fileNameLength, out var payloadLength))
            return false;

        byte[] fileNameBytes = new byte[fileNameLength];
        long read = inputStream.ReadFullBuffer(fileNameBytes);
        if (read != fileNameLength)
            return false;

        string fileName = Encoding.UTF8.GetString(fileNameBytes);

        using var outputStream = CreateOutputFileStream(outputDirectory, fileName);
        read = inputStream.CopyStream(outputStream, payloadLength);

        return read == payloadLength;
    }

    private bool TryReadHeader(Stream inputStream, out ushort fileNameLength, out long payloadLength)
    {
        fileNameLength = 0;
        payloadLength = 0;

        var read = inputStream.ReadByte();
        if (read != FileVersion)
            return false;

        Span<byte> fileNameLengthBytes = stackalloc byte[2];
        read = inputStream.ReadFullBuffer(fileNameLengthBytes);
        if (read != 2)
            return false;

        fileNameLength = BitConverter.ToUInt16(fileNameLengthBytes);

        Span<byte> payloadLengthBytes = stackalloc byte[8];
        read = inputStream.ReadFullBuffer(payloadLengthBytes);
        if (read != 8)
            return false;

        payloadLength = BitConverter.ToInt64(payloadLengthBytes);

        Span<byte> magicStringBytes = stackalloc byte[MagicString.Length];
        read = inputStream.ReadFullBuffer(magicStringBytes);
        if (read != MagicString.Length)
            return false;

        return MagicString.SequenceEqual(magicStringBytes);
    }

    private static Stream CreateOutputFileStream(DirectoryInfo outputDirectory, string preferedFileName)
    {
        var fileName = Path.GetFileNameWithoutExtension(preferedFileName);
        var extension = Path.GetExtension(preferedFileName);
        var filePath = Path.Combine(outputDirectory.FullName, preferedFileName);

        int counter = 1;
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(outputDirectory.FullName, $"{fileName}-{counter++}.{extension}");
        }

        return File.Create(filePath);
    }
}
