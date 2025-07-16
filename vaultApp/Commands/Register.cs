using vaultApp.Services;

namespace vaultApp.Commands
{
    public static class RegisterCommand
    {
        public static void Handle(string[] args)
        {

            // if (args.Length < 2)
            // {
            //     Console.WriteLine("Usage: register <file_name> <file_path>");
            //     return;
            // }

            //connect to the database



            Console.Write("Username: ");
            var username = Console.ReadLine();

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("All fields are required for registration. Make sure to provide a username, email, and password.");
                return; // Indicating registration failed
            }

            bool registered = UserServices.Register(username, email, password);
            if (registered)
            {
                Console.WriteLine("Registration successful!");
            }
            else
            {
                Console.WriteLine("Registration failed. User with this email already exists.");
            }
        }
    }
}