using System.Text;
using CryptoCanut.Crypto;
using NFluent;

namespace CryptoCanut.Test.Crypto;

public class IntegrationTest
{
    [Fact]
    public void FullTest()
    {
        const string payload = "Represents the abstract base class from which all implementations of the Advanced Encryption Standard (AES) must inherit.";
        const string password = "Azerty123!";

        // Arrange
        PasswordHasher passwordHasher = new(1_000);

        FileInfo inputFile = new(Path.GetTempFileName());
        FileInfo encryptedFile = new(Path.GetTempFileName());
        DirectoryInfo outputDir = new(Path.Combine(Path.GetTempPath(), $"CryptoCanut_{Guid.NewGuid()}"));
        try
        {
            File.WriteAllText(inputFile.FullName, payload);
            outputDir.Create();

            // Act
            Encrypter encrypter = new(passwordHasher);
            encrypter.EncryptFile(inputFile, password, encryptedFile);

            Decrypter decrypter = new(passwordHasher);
            var result = decrypter.TryDecryptFile(encryptedFile, password, outputDir);

            // Assert
            Check.That(result).IsTrue();

            var outputFilePath = Path.Combine(outputDir.FullName, inputFile.Name);
            Check.That(File.Exists(outputFilePath)).IsTrue();

            var outputFileContent = File.ReadAllText(outputFilePath, Encoding.UTF8);
            Check.That(outputFileContent).IsEqualTo(payload);
        }
        finally
        {
            // Cleanup
            inputFile.Delete();
            encryptedFile.Delete();
            outputDir.Delete(recursive: true);
        }
    }
}
