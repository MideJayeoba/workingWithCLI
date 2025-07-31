using System.ComponentModel.DataAnnotations;
using vaultApp.Services;

namespace vaultApp.Commands;

public static class Logincommand
{
    public static void Handle(string[] args)
    {
        for (int i = 0; i < 3; i++)
        {
            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("All fields are required for registration. Make sure to provide a username, email, and password.");
                return; // Indicating registration failed
            }


            bool loggedIn = UserServices.Login(email, password);
            if (loggedIn)
            {
                Console.WriteLine($"Welcome back {email}");
                return;
            }
            else
            {
                Console.WriteLine($"Login failed.you have {2 - i} attempts left.");
            }
        }

        Console.WriteLine("Login failed after 3 attempts. Please try again later.");
        return; // Indicating login failed
    }
}
