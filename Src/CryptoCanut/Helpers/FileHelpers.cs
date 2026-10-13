namespace CryptoCanut.Helpers;

internal static class FileHelpers
{
    public static DirectoryInfo CurrentDirectory => new(Environment.CurrentDirectory);

    public static FileInfo GetAvailableFilePath(DirectoryInfo outputDirectory, string preferedFileName)
    {
        var fileName = Path.GetFileNameWithoutExtension(preferedFileName);
        var extension = Path.GetExtension(preferedFileName);
        var filePath = Path.Combine(outputDirectory.FullName, preferedFileName);

        int counter = 1;
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(outputDirectory.FullName, $"{fileName}-{counter++}{extension}");
        }

        return new FileInfo(filePath);
    }
}
