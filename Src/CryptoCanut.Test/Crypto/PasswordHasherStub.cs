using System.Security.Cryptography;
using System.Text;
using CryptoCanut.Crypto;

namespace CryptoCanut.Test.Crypto;

internal class PasswordHasherStub : IPasswordHasher
{
    public byte[] DeriveBytes(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        return SHA256.HashData([.. passwordBytes, .. salt]);
    }
}
