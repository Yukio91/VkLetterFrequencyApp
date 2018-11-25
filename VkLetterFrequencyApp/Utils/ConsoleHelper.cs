using System;

namespace VkLetterFrequencyApp.Utils
{
    public static class ConsoleHelper
    {
        public static string MaskingPassword()
        {
            string password = "";

            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.Enter)
                {
                    if (key.Key != ConsoleKey.Backspace || String.IsNullOrEmpty(password))
                    {
                        if (key.Key != ConsoleKey.Enter)
                            continue;

                        break;
                    }

                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
                else
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }

            } while (true);

            Console.WriteLine();

            return password;
        }
    }
}
