using vaultApp.Models;
using vaultApp.Database;


namespace vaultApp.Repositories
{
    public class FileRepo
    {
        // Method to create new file/folder
        public static string Create(FileEntity file)
        {
            try
            {
                using var connection = Database.Database.GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO Files (Id, UserId, Name, Path, Type, Visibility, Size, UploadTime, ParentId)
            VALUES (@id, @userId, @name, @path, @type, @visibility, @size, @uploadTime, @parentId)";

                command.Parameters.AddWithValue("@id", file.Id);
                command.Parameters.AddWithValue("@userId", file.UserId);
                command.Parameters.AddWithValue("@name", file.Name);
                command.Parameters.AddWithValue("@path", file.Path);
                command.Parameters.AddWithValue("@type", file.Type);
                command.Parameters.AddWithValue("@visibility", file.Visibility);
                command.Parameters.AddWithValue("@size", file.Size);
                command.Parameters.AddWithValue("@uploadTime", file.UploadTime.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@parentId", file.ParentId ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
                Console.WriteLine($"✅ File {file.Name} saved to database");
                if (file.Id == null)
                {
                    throw new InvalidOperationException("File Id cannot be null.");
                }
                return file.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving file: {ex.Message}");
                throw;
            }
        }

        // Method to check if file already exists
        public static bool Exists(string fileName, string userId)
        {
            try
            {
                using var connection = Database.Database.GetConnection();
                connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Files WHERE Name = @name AND UserId = @userId";
                command.Parameters.AddWithValue("@name", fileName);
                command.Parameters.AddWithValue("@userId", userId);

                var count = Convert.ToInt32(command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error checking file existence: {ex.Message}");
                return false; // If error, assume file no exist so upload fit continue
            }
        }

        // Method to get file by ID
        public static FileEntity? GetById(string id, string userId)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
            {
                return null;
            }

            try
            {
                using var connection = Database.Database.GetConnection();
                connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Files WHERE Id = @id AND UserId = @userId";
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@userId", userId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new FileEntity
                    {
                        Id = reader["Id"].ToString(),
                        UserId = reader["UserId"].ToString(),
                        Name = reader["Name"].ToString(),
                        Path = reader["Path"].ToString(),
                        Type = reader["Type"]?.ToString() ?? "file",
                        Visibility = reader["Visibility"]?.ToString() ?? "private",
                        Size = Convert.ToInt64(reader["Size"]),
                        UploadTime = DateTime.TryParse(reader["UploadTime"]?.ToString(), out var uploadTime) ? uploadTime : DateTime.MinValue,
                        ParentId = reader["ParentId"]?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving file: {ex.Message}");
            }
            return null;
        }

        // Method to get all files for a user
        public static List<FileEntity> GetAllByUserId(string userId)
        {
            var files = new List<FileEntity>();
            try
            {
                using var connection = Database.Database.GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Files WHERE UserId = @UserId ORDER BY UploadTime DESC";
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    files.Add(new FileEntity
                    {
                        Id = reader["Id"].ToString(),
                        Name = reader["Name"].ToString(),
                        Path = reader["Path"].ToString(),
                        Type = reader["Type"]?.ToString() ?? "file",
                        Visibility = reader["Visibility"]?.ToString() ?? "private",
                        Size = Convert.ToInt64(reader["Size"]),
                        UploadTime = DateTime.Parse(reader["UploadTime"].ToString() ?? string.Empty),
                        ParentId = reader["ParentId"]?.ToString()
                    });
                }
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving files: {ex.Message}");
                return new List<FileEntity>();
            }
        }

        // Method to delete file
        public static bool Delete(string id)
        {
            try
            {
                var fileId = id ?? throw new ArgumentNullException(nameof(id));
                {
                    using var connection = Database.Database.GetConnection();
                    connection.Open();

                    // Get file info before deleting
                    var selectCommand = connection.CreateCommand();
                    selectCommand.CommandText = "SELECT Name, Path FROM Files WHERE Id = @id";
                    selectCommand.Parameters.AddWithValue("@id", id);

                    using var reader = selectCommand.ExecuteReader();
                    if (!reader.Read())
                    {
                        return false;
                    }

                    var fileName = reader["Name"].ToString();
                    var filePath = reader["Path"].ToString();
                    reader.Close();

                    // Delete from database
                    var deleteCommand = connection.CreateCommand();
                    deleteCommand.CommandText = "DELETE FROM Files WHERE Id = @id";
                    deleteCommand.Parameters.AddWithValue("@id", fileId);

                    var rowsAffected = deleteCommand.ExecuteNonQuery();
                    var thumbnailPath = Path.Combine("Storage", "thumbnails", $"{fileId}.jpg");
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }



        // Method to change file visibility
        public static bool ChangeVisibility(string fileId, string visibility, string userId)
        {

            try
            {
                using var connection = Database.Database.GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Files SET Visibility = @visibility WHERE Id = @fileId AND UserId = @userId";
                command.Parameters.AddWithValue("@visibility", visibility);
                command.Parameters.AddWithValue("@fileId", fileId);
                command.Parameters.AddWithValue("@userId", userId);

                var rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing file visibility: {ex.Message}");
                return false;
            }
        }

        // Method to get all public files
        public static List<FileEntity> GetPublicFiles()
        {
            var files = new List<FileEntity>();
            try
            {
                using var connection = Database.Database.GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Files WHERE Visibility = 'public' ORDER BY UploadTime DESC";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    files.Add(new FileEntity
                    {
                        Id = reader["Id"].ToString(),
                        Name = reader["Name"].ToString(),
                        Path = reader["Path"].ToString(),
                        Type = reader["Type"]?.ToString() ?? "file",
                        Visibility = reader["Visibility"]?.ToString() ?? "private",
                        Size = Convert.ToInt64(reader["Size"]),
                        UploadTime = DateTime.Parse(reader["UploadTime"].ToString() ?? string.Empty),
                        ParentId = reader["ParentId"]?.ToString()
                    });
                }
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving public files: {ex.Message}");
                return new List<FileEntity>();
            }
        }
        
    }
}

            