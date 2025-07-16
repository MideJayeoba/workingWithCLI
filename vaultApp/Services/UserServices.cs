using vaultApp.Commands;
// using Microsoft.Data.Sqlite;
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

            var connection = Database.Database.GetConnection();
            connection.Open();
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = $email";
            checkCmd.Parameters.AddWithValue("$email", email);

            var result = checkCmd.ExecuteScalar();
            long count = (result != null && result != DBNull.Value) ? Convert.ToInt64(result) : 0;

            if (count > 0)
            {
                return false; // user already exists
            }
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Users (Id, Username, Email, HashedPassword, CreatedAt)
                VALUES ($id, $username, $email, $password, $createdAt)";
            insertCmd.Parameters.AddWithValue("$id", UserID);
            insertCmd.Parameters.AddWithValue("$username", username);
            insertCmd.Parameters.AddWithValue("$email", email);
            insertCmd.Parameters.AddWithValue("$password", hashedPassword);
            insertCmd.Parameters.AddWithValue("$createdAt", created_at);

            insertCmd.ExecuteNonQuery();

            return true;
        }

        public static bool Login(string email, string password)
        {

            var connection = Database.Database.GetConnection();
            connection.Open();
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT HashedPassword, Id FROM Users WHERE Email = $email";
            checkCmd.Parameters.AddWithValue("$email", email);

            var reader = checkCmd.ExecuteReader();
            if (!reader.Read())
            {
                return false; // user not found
            }
            var result = reader["HashedPassword"].ToString();
            var userId = reader["Id"]?.ToString();

            if (BCrypt.Net.BCrypt.Verify(password, result) && !string.IsNullOrEmpty(userId))
            {
                // Create session
                Redis.CreateSession(userId);
                return true;
            }
            else
                return false; // password does not match
        }
    }
}
    