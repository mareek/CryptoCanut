using System.Security.Cryptography;

namespace CryptoCanut.Crypto;

internal class PasswordHasher(int iterationCount = PasswordHasher.DefaultIterationCount) 
{
    private const int DefaultIterationCount = 1_000_000;

    public byte[] DeriveBytes(string password, Span<byte> salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterationCount, HashAlgorithmName.SHA256, 32);
    }
}
