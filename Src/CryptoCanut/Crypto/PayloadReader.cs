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

    public bool TryReadHeader(Stream stream, out byte version, out ushort fileNameLength, out long payloadLength)
    {
        version = 0;
        fileNameLength = 0;
        payloadLength = 0;

        var read = stream.ReadByte();
        if (read != FileVersion)
            return false;

        version = (byte)read;

        Span<byte> fileNameLengthBytes = stackalloc byte[2];
        read = stream.ReadFullBuffer(fileNameLengthBytes);
        if (read != 2)
            return false;

        fileNameLength = BitConverter.ToUInt16(fileNameLengthBytes);

        Span<byte> payloadLengthBytes = stackalloc byte[8];
        read = stream.ReadFullBuffer(payloadLengthBytes);
        if (read != 8)
            return false;

        payloadLength = BitConverter.ToInt64(payloadLengthBytes);

        Span<byte> magicStringBytes = stackalloc byte[MagicString.Length];
        read = stream.ReadFullBuffer(magicStringBytes);
        if (read != MagicString.Length)
            return false;

        return MagicString.SequenceEqual(magicStringBytes);
    }
}
