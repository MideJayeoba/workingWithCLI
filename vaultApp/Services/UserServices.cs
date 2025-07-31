using vaultApp.Repositories;
using vaultApp.Database;


namespace vaultApp.Services
{

    public static class UserServices
    {
        public static bool Register(string username, string email, string password)
        {
            // Here you would typically save the user information to a database or a file.

            // For simplicity, we will just print it to the console.
            var UserID = Guid.NewGuid().ToString().Substring(0, 10);
            var created_at = DateTime.Now;
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("All fields are required for registration.");
                return false; // Indicating registration failed
            }
            //we are storing to the database
            // var user = new
            // {
            //     UserID,
            //     Username = username,
            //     Email = email,
            //     Password = password, 
            //     CreatedAt = created_at
            // };
            try
            {
                var user = new UserEntity(UserID, username, email, hashedPassword, created_at);
                // Check if user already exists
                if (UserRepository.UserExists(email))
                {
                    return false; // Indicating registration failed
                }
                if (UserRepository.Register(user))
                {
                    return true; // Indicating registration was successful
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                return false; // Indicating registration failed
            }
            return false; 
        }

        public static bool Login(string email, string password)
        {
            var userExist = UserRepository.UserExists(email);
            var userId = UserRepository.Login(email, password);
            if (userExist && userId == null)
            {
                Console.WriteLine("User exists but password is incorrect.");
                return false; // Indicating login failed
            }
            if (userId != null)
            {
                Redis.CreateSession(userId);
                return true; // Indicating login was successful
            }
            else
            {
                return false; // Indicating login failed
            }
        }

        public static void Logout()
        {

            Redis.EndSession();
            return;
        }
    }
}
