using vaultApp.Models;

namespace vaultApp.Repositories;

public class UserRepository
{
    public static bool Register(UserEntity user)
    {
        try
        {
            var connection = Database.Database.GetConnection();
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Users (Id, Username, Email, HashedPassword, CreatedAt) VALUES ($id, $username, $email, $hashedPassword, $createdAt)";
            cmd.Parameters.AddWithValue("$id", user.Id);
            cmd.Parameters.AddWithValue("$username", user.Name);
            cmd.Parameters.AddWithValue("$email", user.Email);
            cmd.Parameters.AddWithValue("$hashedPassword", user.Password);
            cmd.Parameters.AddWithValue("$createdAt", user.CreatedAt);
            cmd.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during registration: {ex.Message}");
            return false;
        }
    }

    public static bool UserExists(string email)
    {
        try
        {
            var connection = Database.Database.GetConnection();
            connection.Open();
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = $email";
            checkCmd.Parameters.AddWithValue("$email", email);

            var result = checkCmd.ExecuteScalar();
            long count = (result != null && result != DBNull.Value) ? Convert.ToInt64(result) : 0;

            return count > 0; // returns true if user exists
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking user existence: {ex.Message}");
            return false;
        }
    }

    public static string? Login(string email, string password)
    {
        try
        {
            var connection = Database.Database.GetConnection();
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, HashedPassword FROM Users WHERE Email = $email";
            cmd.Parameters.AddWithValue("$email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var userId = reader["Id"].ToString();
                var hashedPassword = reader["HashedPassword"].ToString();
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(hashedPassword))
                {
                    Console.WriteLine("Invalid email or password.");
                    return null; // Return null if user not found
                }

                if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                {
                    return userId; // Return user ID on successful login
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during login: {ex.Message}");
        }
        return null; // Return null if login fails
    }
}