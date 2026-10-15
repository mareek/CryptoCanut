using System.Text;
using CryptoCanut.Crypto;
using NFluent;

namespace CryptoCanut.Test.Crypto;

public class PayloadTest
{
    [Fact]
    public void FullTest()
    {
        // Arrange
        const string payload = "WizeBot: J’ai demandé ce que je pouvais faire pour toi, champion";
        FileInfo inputFile = new(Path.GetTempFileName());
        DirectoryInfo outputDir = new(Path.Combine(Path.GetTempPath(), $"CryptoCanut_{Guid.NewGuid()}"));
        try
        {
            File.WriteAllText(inputFile.FullName, payload);
            outputDir.Create();

            // Act
            using MemoryStream stream = new();
            PayloadWriter payloadWriter = new();
            payloadWriter.WriteToStream(stream, inputFile);

            stream.Seek(0, SeekOrigin.Begin);
            PayloadReader payloadReader = new();
            bool success = payloadReader.TryExtractPayload(outputDir, stream);

            // Assert
            Check.That(success).IsTrue();

            var outputFilePath = Path.Combine(outputDir.FullName, inputFile.Name);
            Check.That(File.Exists(outputFilePath)).IsTrue();

            var outputFileContent = File.ReadAllText(outputFilePath, Encoding.UTF8);
            Check.That(outputFileContent).IsEqualTo(payload);
        }
        finally
        {
            inputFile.Delete();
            outputDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void TestFileNameColision()
    {
        // Arrange
        const int fileCount = 10;
        const string payload = "J’habite dans un immeuble 4 ans donnant sur cour autour de laquelle il y a 4 autres immeubles (certains sont des logements sociaux et d’autres du privatif) Depuis cette semaine, il y a une des voisines";
        const string SecretFileName = "SecretFile";
        const string SecretFileExtension = "txt";
        DirectoryInfo outputDir = new(Path.Combine(Path.GetTempPath(), $"CryptoCanut_{Guid.NewGuid()}"));
        FileInfo inputFile = new(Path.Combine(outputDir.FullName, $"{SecretFileName}.{SecretFileExtension}"));
        try
        {
            outputDir.Create();
            File.WriteAllText(inputFile.FullName, payload);

            // Act
            using MemoryStream stream = new();
            PayloadWriter payloadWriter = new();
            payloadWriter.WriteToStream(stream, inputFile);

            PayloadReader payloadReader = new();
            for (int i = 0; i < fileCount; i++)
            {
                stream.Seek(0, SeekOrigin.Begin);
                bool success = payloadReader.TryExtractPayload(outputDir, stream);
                Check.That(success).IsTrue();
            }

            // Assert
            Check.That(outputDir.EnumerateFiles().Count()).Is(fileCount + 1);
            for (int i = 1; i <= fileCount; i++)
            {
                var outputFilePath = Path.Combine(outputDir.FullName, $"{SecretFileName}-{i}.{SecretFileExtension}");
                Check.That(File.Exists(outputFilePath)).IsTrue();

                var outputFileContent = File.ReadAllText(outputFilePath, Encoding.UTF8);
                Check.That(outputFileContent).IsEqualTo(payload);
            }
        }
        finally
        {
            outputDir.Delete(recursive: true);
        }
    }

    /// <summary>
    /// Check that files with close but different sizes are padded to have the same size
    /// </summary>
    [Theory]
    [InlineData(500)]
    [InlineData(5_000)]
    [InlineData(50_000)]
    public void PaddingTest(int payloadSize)
    {
        // Arrange
        const int fileCount = 10;
        FileInfo inputFile = new(Path.GetTempFileName());
        long[] fileSizes = new long[fileCount];
        try
        {
            // Act
            for (int i = 0; i < fileCount; i++)
            {
                byte[] payload = new byte[payloadSize + i * 1];
                Random.Shared.NextBytes(payload);
                File.WriteAllBytes(inputFile.FullName, payload);

                using MemoryStream stream = new();
                PayloadWriter payloadWriter = new();
                payloadWriter.WriteToStream(stream, inputFile);
                fileSizes[i] = stream.Position;
            }

            // Assert
            Check.That(fileSizes.Distinct()).HasSize(1);
        }
        finally
        {
            inputFile.Delete();
        }
    }
}
