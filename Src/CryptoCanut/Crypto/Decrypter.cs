using System.Security.Cryptography;
using System.Text;

namespace CryptoCanut.Crypto;

internal class Decrypter(PasswordHasher passwordHasher)
{
    /* File Format
     *  3 bytes : "CCT" magic string
     *  1 byte  : version
     * 16 bytes : salt
     *  N bytes : encrypted payload */

    private const byte FileVersion = 1;
    private const int SaltLength = 16;

    private static readonly byte[] MagicString = [67, 67, 84]; // "CCT"

    public bool TryDecryptFile(FileInfo inputFile, string password, DirectoryInfo outputDirectory)
    {
        using var stream = inputFile.OpenRead();

        Span<byte> magicStringBuffer = stackalloc byte[MagicString.Length];
        long read = stream.Read(magicStringBuffer);
        if (read != MagicString.Length || !MagicString.SequenceEqual(magicStringBuffer))
            return false;

        read = stream.ReadByte();
        if (read != FileVersion)
            return false;

        Span<byte> saltBuffer = stackalloc byte[SaltLength];
        read = stream.Read(saltBuffer);
        if (read != SaltLength)
            return false;

        byte[] passwordBytes = passwordHasher.DeriveBytes(password, saltBuffer);
        using var aes = Aes.Create();
        aes.Key = passwordBytes[..16];
        aes.IV = passwordBytes[16..];

        using var decryptor = aes.CreateDecryptor();
        using var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read, leaveOpen: true);

        return TryExtractPayload(outputDirectory, cryptoStream);
    }

    private bool TryExtractPayload(DirectoryInfo outputDirectory, CryptoStream cryptoStream)
    {
        PayloadReader payloadReader = new();
        if (!payloadReader.TryReadHeader(cryptoStream, out var version, out var fileNameLength, out var payloadLength))
            return false;

        byte[] fileNameBytes = new byte[fileNameLength];
        long read = cryptoStream.ReadFullBuffer(fileNameBytes);
        if (read != fileNameLength)
            return false;

        string fileName = Encoding.UTF8.GetString(fileNameBytes);

        using var outputStream = CreateOutputFileStream(outputDirectory, fileName);
        read = cryptoStream.CopyStream(outputStream, payloadLength);

        if (read != payloadLength)
            return false;

        return true;
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
