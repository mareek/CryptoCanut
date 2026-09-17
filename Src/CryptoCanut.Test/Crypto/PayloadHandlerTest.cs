using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using CryptoCanut.Crypto;
using NFluent;

namespace CryptoCanut.Test.Crypto;

public class PayloadHandlerTest
{
    [Fact]
    public void FullTest()
    {
        // Arrange
        const string payload = "WizeBot: J’ai demandé ce que je pouvais faire pour toi, champion";
        FileInfo tempFile = new(Path.GetTempFileName());
        try
        {
            File.WriteAllText(tempFile.FullName, payload);

            // Act
            using MemoryStream stream = new();
            PayloadHandler handler = new();
            handler.WriteToStream(stream, tempFile);

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            Check.That(handler.TryReadHeader(stream, out var version, out var fileNameLength, out var payloadLength)).IsTrue();
            Check.That(version).Is((byte)1);
            Check.That((int)fileNameLength).Is(Encoding.UTF8.GetByteCount(tempFile.Name));
            Check.That((int)payloadLength).Is(Encoding.UTF8.GetByteCount(payload));

            var fileNameBytes = new byte[fileNameLength];
            stream.ReadExactly(fileNameBytes);
            Check.That(Encoding.UTF8.GetString(fileNameBytes)).Is(tempFile.Name);

            var payloadBytes = new byte[payloadLength];
            stream.ReadExactly(payloadBytes);
            Check.That(Encoding.UTF8.GetString(payloadBytes)).Is(payload);
        }
        finally
        {
            tempFile.Delete();
        }
    }
}
