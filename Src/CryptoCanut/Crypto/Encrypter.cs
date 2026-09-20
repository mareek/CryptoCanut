using System.Security.Cryptography;

namespace CryptoCanut.Crypto;

internal class Encrypter(IPasswordHasher passwordHasher, IPayloadWriter payloadWriter)
{
    private const byte FileVersion = 1;

    private static readonly byte[] MagicString = [67, 67, 84]; // "CCT"

    public void EncryptFile(FileInfo inputFile, string password, FileInfo outputFile)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] passwordBytes = passwordHasher.DeriveBytes(password, salt);

        var tempFilePath = Path.GetTempFileName();

        using (var tempFileStream = File.OpenWrite(tempFilePath))
        {
            /* File Format
             *  3 bytes : "CCT" magic string
             *  1 byte  : version
             * 16 bytes : salt
             *  N bytes : encrypted payload */
            WriteHeader(tempFileStream, salt);
            WritePayload(tempFileStream, passwordBytes, inputFile);
        }

        File.Move(tempFilePath, outputFile.FullName, overwrite: true);
    }

    private static void WriteHeader(Stream fileStream, byte[] salt)
    {
        fileStream.Write(MagicString);
        fileStream.WriteByte(FileVersion);
        fileStream.Write(salt);
    }

    private void WritePayload(Stream fileStream, byte[] passwordBytes, FileInfo inputFile)
    {
        using var aes = Aes.Create();
        aes.Key = passwordBytes[..16];
        aes.IV = passwordBytes[16..];

        using var encryptor = aes.CreateEncryptor();
        using var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
        payloadWriter.WriteToStream(cryptoStream, inputFile);
    }
}
