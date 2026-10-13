using System.Text;

namespace CryptoCanut.Tui;

internal class PasswordHandler
{
    public static string ReadAndConfirmPassword()
    {
        do
        {
            Console.WriteLine("Enter password :");
            string password = ReadPassword();
            Console.WriteLine("Confirm password :");
            if (password == ReadPassword())
                return password;

            Console.WriteLine("Password does not match, try again");
        }
        while (true);
    }

    public static string ReadPassword()
    {
        StringBuilder passwordBuilder = new();
        do
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
                break;
            if (key.Key == ConsoleKey.Escape)
                throw new OperationCanceledException("Escape key pressed");
            else if (key.Key == ConsoleKey.Backspace)
                passwordBuilder.Remove(passwordBuilder.Length - 1, 1);
            else if (key.KeyChar != char.MinValue)
                passwordBuilder.Append(key.KeyChar);
        } while (true);

        return passwordBuilder.ToString();
    }
}
