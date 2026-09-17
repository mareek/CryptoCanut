using System;
using System.Collections.Generic;
using System.Text;
using CryptoCanut.Crypto;

namespace CryptoCanut.Test.Crypto;

internal class PayloadHandlerStub : IPayloadHandler
{
    public bool TryReadHeader(Stream stream, out byte version, out ushort fileNameLength, out long payloadLength)
    {
        throw new NotImplementedException();
    }

    public void WriteToStream(Stream stream, FileInfo file)
    {
        using var streamReader = file.OpenRead();
        streamReader.CopyTo(stream);
    }
}
