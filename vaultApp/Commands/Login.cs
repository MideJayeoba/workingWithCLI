using System.ComponentModel.DataAnnotations;
using vaultApp.Services;

namespace vaultApp.Commands;

public static class Logincommand
{
    public static void Handle(string[] args)
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
            Console.WriteLine($"Welcome back {email}");
        else
            Console.WriteLine($"Try to register before logging in please!");
    }
}
