namespace CryptoCanut.Crypto;

internal interface IPayloadHandler
{
    bool TryReadHeader(Stream stream, out byte version, out ushort fileNameLength, out long payloadLength);
    void WriteToStream(Stream stream, FileInfo file);
}