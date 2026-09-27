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
}
