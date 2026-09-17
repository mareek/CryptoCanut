using System;
using System.Collections.Generic;
using System.Text;
using CryptoCanut.Crypto;

namespace CryptoCanut.Test.Crypto;

public class EncrypterTest
{
    [Fact]
    public void MyTest()
    {
        const string payload = "Represents the abstract base class from which all implementations of the Advanced Encryption Standard (AES) must inherit.";
        const string password = "Azerty123!";

        FileInfo inputFile = new(Path.GetTempFileName());
        FileInfo outputFile = new(Path.GetTempFileName());
        try
        {
            File.WriteAllText(inputFile.FullName, payload);

            PasswordHasherStub hasher = new();
            PayloadHandlerStub payloadHandler = new();

            Encrypter encrypter = new(hasher, payloadHandler);
            encrypter.EncryptFile(inputFile, password, outputFile);
        }
        finally
        {
            inputFile.Delete();
            outputFile.Delete();
        }
    }
}
