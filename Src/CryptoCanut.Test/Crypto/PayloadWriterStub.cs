using CryptoCanut.Crypto;

namespace CryptoCanut.Test.Crypto;

internal class PayloadWriterStub : IPayloadWriter
{
    public void WriteToStream(Stream stream, FileInfo file)
    {
        using var streamReader = file.OpenRead();
        streamReader.CopyTo(stream);
    }
}
