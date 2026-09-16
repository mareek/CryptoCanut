namespace CryptoCanut.Crypto
{
    internal interface IPasswordHasher
    {
        byte[] DeriveBytes(string password, byte[] salt);
    }
}