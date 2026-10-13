using CryptoCanut.Tui;

var rootCommand = Commands.GetRootCommand();

var parseResult = rootCommand.Parse(args);
if (parseResult.Errors.Count == 0)
{
    parseResult.Invoke();
    return 0;
}

foreach (var parseError in parseResult.Errors)
{
    Console.Error.WriteLine(parseError.Message);
}

return 1;
