namespace CryptoCanut.Crypto;

internal interface IPayloadWriter
{
    void WriteToStream(Stream stream, FileInfo file);
}