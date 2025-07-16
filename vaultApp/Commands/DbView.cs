using Microsoft.Data.Sqlite;

namespace vaultApp.Commands
{
    public static class DbViewCommand
    {
        public static void Handle(string[] args)
        {
            Console.WriteLine("=== Database Viewer ===\n");

            try
            {
                using var connection = Database.Database.GetConnection();
                connection.Open();

                // Show Users table
                ShowUsersTable(connection);

                // Show Files table
                ShowFilesTable(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing database: {ex.Message}");
            }
        }

        private static void ShowUsersTable(SqliteConnection connection)
        {
            Console.WriteLine("📁 USERS TABLE:");
            Console.WriteLine("----------------------------------------");

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users";
            var userCount = Convert.ToInt32(command.ExecuteScalar());

            if (userCount == 0)
            {
                Console.WriteLine("No users found.");
            }
            else
            {
                command.CommandText = "SELECT Id, Username, Email, CreatedAt FROM Users ORDER BY CreatedAt DESC";
                using var reader = command.ExecuteReader();

                Console.WriteLine($"{"ID",-12} | {"Username",-15} | {"Email",-25} | {"Created At",-20}");
                Console.WriteLine(new string('-', 80));

                while (reader.Read())
                {
                    Console.WriteLine($"{reader["Id"]?.ToString(),-12} | {reader["Username"]?.ToString(),-15} | {reader["Email"]?.ToString(),-25} | {reader["CreatedAt"]?.ToString(),-20}");
                }
            }

            Console.WriteLine($"\nTotal Users: {userCount}\n");
        }

        private static void ShowFilesTable(SqliteConnection connection)
        {
            Console.WriteLine("📂 FILES TABLE:");
            Console.WriteLine("----------------------------------------");

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Files";
            var fileCount = Convert.ToInt32(command.ExecuteScalar());

            if (fileCount == 0)
            {
                Console.WriteLine("No files found.");
            }
            else
            {
                command.CommandText = "SELECT Id, Name, Size, UploadTime FROM Files ORDER BY UploadTime DESC";
                using var reader = command.ExecuteReader();

                Console.WriteLine($"{"ID",-12} | {"Name",-20} | {"Size (bytes)",-15} | {"Upload Time",-20}");
                Console.WriteLine(new string('-', 75));

                while (reader.Read())
                {
                    var size = Convert.ToInt64(reader["Size"]);
                    var sizeDisplay = size > 1024 * 1024
                        ? $"{size / (1024.0 * 1024.0):F2} MB"
                        : $"{size / 1024.0:F2} KB";

                    Console.WriteLine($"{reader["Id"]?.ToString(),-12} | {reader["Name"]?.ToString(),-20} | {sizeDisplay,-15} | {reader["UploadTime"]?.ToString(),-20}");
                }
            }

            Console.WriteLine($"\nTotal Files: {fileCount}\n");
        }
    }
}
