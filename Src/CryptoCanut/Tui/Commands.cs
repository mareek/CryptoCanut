using System.CommandLine;
using CryptoCanut.Crypto;
using CryptoCanut.Helpers;

namespace CryptoCanut.Tui;

internal static class Commands
{
    public static RootCommand GetRootCommand()
    {
        RootCommand rootCommand = new("CryptoCanut - a simple tool to encrypt and decrypt files");
        rootCommand.Subcommands.Add(GetEncryptCommand());
        rootCommand.Subcommands.Add(GetDecryptCommand());
        return rootCommand;
    }

    private static Command GetEncryptCommand()
    {
        var fileArg = CreateFileArgument("The file to encrypt");
        Option<FileInfo> destOption = new("--destination", "-d") { Description = "The encrypted file" };
        Command encryptCommand = new("encrypt", "Encrypt a file")
        {
            fileArg,
            destOption,
        };

        encryptCommand.SetAction(parsedResult =>
        {
            var file = parsedResult.GetRequiredValue(fileArg);
            var outputFile = parsedResult.GetValue(destOption)
                              ?? FileHelpers.GetAvailableFilePath(file.Directory ?? FileHelpers.CurrentDirectory, file.Name + ".cct");
            EncryptFile(file, outputFile);
        });

        return encryptCommand;
    }

    private static Command GetDecryptCommand()
    {
        var fileArg = CreateFileArgument("The file to encrypt");
        Option<DirectoryInfo> destOption = new("--destination", "-d") { Description = "The directory where the decrypted file will be written" };
        Command decryptCommand = new("decrypt", "Decrypt a file")
        {
            fileArg,
            destOption,
        };

        decryptCommand.SetAction(parsedResult =>
        {
            var file = parsedResult.GetRequiredValue(fileArg);
            var outputDirectory = parsedResult.GetValue(destOption) ?? FileHelpers.CurrentDirectory;
            DecryptFile(file, outputDirectory);
        });

        return decryptCommand;
    }

    private static Argument<FileInfo> CreateFileArgument(string description)
    {
        Argument<FileInfo> argument = new("file") { Description = description };
        argument.Validators.Add(result =>
        {
            var file = result.GetValue(argument);
            if (file == null || !file.Exists)
            {
                result.AddError("File does not exist");
            }
        });

        return argument;
    }

    private static void EncryptFile(FileInfo inputFile, FileInfo outputFile)
    {
        string password;
        try
        {
            password = PasswordHandler.ReadAndConfirmPassword();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled");
            return;
        }

        Encrypter encrypter = new(new());

        encrypter.EncryptFile(inputFile, password, outputFile);

        Console.WriteLine($"Successfuly encrypted {inputFile.Name} into {outputFile.Name}");
    }

    private static void DecryptFile(FileInfo file, DirectoryInfo outputDirectory)
    {
        string password;
        try
        {
            Console.WriteLine("Enter password :");
            password = PasswordHandler.ReadPassword();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled");
            return;
        }

        Decrypter decrypter = new(new());
        if (decrypter.TryDecryptFile(file, password, outputDirectory))
            Console.WriteLine("File successfully decrypted");
        else
            Console.WriteLine("The password is incorrect or the file is not a valid CryptoCanut encrypted file");
    }

}
