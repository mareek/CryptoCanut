using System.Security.Cryptography;

namespace CryptoCanut.Crypto;

internal class PasswordHasher : IPasswordHasher
{
    private const int IterationCount = 1_000_000;

    public byte[] DeriveBytes(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, IterationCount, HashAlgorithmName.SHA256, 32);
    }
}
