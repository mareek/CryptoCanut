using System.Security.Cryptography;

namespace CryptoCanut.Crypto;

internal class Encrypter(PasswordHasher passwordHasher)
{
    /* File Format
     *  3 bytes : "CCT" magic string
     *  1 byte  : version
     * 16 bytes : salt
     *  N bytes : encrypted payload */

    private const byte FileVersion = 1;
    private const int SaltLength = 16;

    private static readonly byte[] MagicString = [67, 67, 84]; // "CCT"

    public void EncryptFile(FileInfo inputFile, string password, FileInfo outputFile)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltLength);
        byte[] passwordBytes = passwordHasher.DeriveBytes(password, salt);

        var tempFilePath = Path.GetTempFileName();

        try
        {
            using (var tempFileStream = File.OpenWrite(tempFilePath))
            {
                WriteHeader(tempFileStream, salt);
                WritePayload(tempFileStream, passwordBytes, inputFile);
            }

            File.Move(tempFilePath, outputFile.FullName, overwrite: true);
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    private static void WriteHeader(Stream fileStream, byte[] salt)
    {
        fileStream.Write(MagicString);
        fileStream.WriteByte(FileVersion);
        fileStream.Write(salt);
    }

    private static void WritePayload(Stream fileStream, byte[] passwordBytes, FileInfo inputFile)
    {
        using var aes = Aes.Create();
        aes.Key = passwordBytes[..16];
        aes.IV = passwordBytes[16..];

        using var encryptor = aes.CreateEncryptor();
        using var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);

        PayloadWriter payloadWriter = new();
        payloadWriter.WriteToStream(cryptoStream, inputFile);
    }
}
