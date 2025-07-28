using vaultApp.Database;
using System.Text.Json;


namespace vaultApp.Services;


public static class FileService
{
    private static string? dirPath;

    // Uploads a file to the vault and returns its ID
    public static string Upload(string filePath)
    {
        if (!Directory.Exists("Storage/uploads"))
        {
            Directory.CreateDirectory("Storage/uploads");
        }

        var fileId = Guid.NewGuid().ToString().Substring(0, 10);
        var userId = Redis.GetCurrentUserId() ?? throw new InvalidOperationException("User not logged in.");
        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine("Storage", "uploads", fileName);

        
        var imgextensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        if (imgextensions.Contains(Path.GetExtension(filePath).ToLower()))
        {
            var job = new
            {
                file_id = fileId,
                path = destinationPath
            };
            string jobJson = JsonSerializer.Serialize(job);

            // Here you would typically enqueue the job to a background worker or similar
            Redis.CreateJobQueue(jobJson);
        
        }

        // Check if file already exists in database
        try
        {
            using var connection = Database.Database.GetConnection();
            connection.Open();

            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Files WHERE Name = @name";
            checkCommand.Parameters.AddWithValue("@name", fileName);

            var count = Convert.ToInt32(checkCommand.ExecuteScalar());
            if (count > 0)
            {
                throw new InvalidOperationException($"File with the name '{fileName}' already exists in the Vault!");
            }

            // Copy file to storage
            File.Copy(filePath, destinationPath, true);
            var fileInfo = new FileInfo(destinationPath);
            var fileSize = fileInfo.Length; // Replace with actual user ID logic
            var uploadTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Insert file metadata into database
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO Files (UserId, Id, Name, Path, Type, Visibility, Size, UploadTime)
                VALUES (@UserId, @id, @name, @path, @type, @visibility, @size, @uploadTime)";

            insertCommand.Parameters.AddWithValue("@UserId", userId);
            insertCommand.Parameters.AddWithValue("@id", fileId);
            insertCommand.Parameters.AddWithValue("@name", fileName);
            insertCommand.Parameters.AddWithValue("@path", destinationPath);
            insertCommand.Parameters.AddWithValue("@type", "file");
            insertCommand.Parameters.AddWithValue("@visibility", "private");
            insertCommand.Parameters.AddWithValue("@size", fileSize);
            insertCommand.Parameters.AddWithValue("@uploadTime", uploadTime);

            insertCommand.ExecuteNonQuery();

            return fileId;
        }
        catch (Exception ex)
        {
            // Clean up file if database operation fails
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
            throw new InvalidOperationException($"Upload failed: {ex.Message}");
        }
    }
    public static List<FileMeta> GetAllFiles()
    {
        var files = new List<FileMeta>();
        var userId = Redis.GetCurrentUserId() ?? throw new InvalidOperationException("User not logged in.");

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
                files.Add(new FileMeta
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving files: {ex.Message}");
        }

        return files;
    }

    public static bool Delete(string fileId)
    {
        try
        {
            using var connection = Database.Database.GetConnection();
            connection.Open();

            // Get file info before deleting
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT Name, Path FROM Files WHERE Id = @id";
            selectCommand.Parameters.AddWithValue("@id", fileId);

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
            if (rowsAffected > 0)
            {
                // Delete physical file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }
                Console.WriteLine($"File '{fileName}' deleted successfully.");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
            return false;
        }
    }

    public static bool Mkdir(string[] directoryName)
    {
        var dirName = directoryName[0];
        string? parentId = directoryName.Length > 1 ? directoryName[1] : null;

        try
        {
            var folderId = Guid.NewGuid().ToString().Substring(0, 10);
            var userId = Redis.GetCurrentUserId() ?? throw new InvalidOperationException("User not logged in.");
            var connection = Database.Database.GetConnection();
            connection.Open();

            string dirPath;

            // If parentId is provided, get the parent directory's path first
            if (parentId != null)
            {
                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "SELECT Path FROM Files WHERE Id = @ParentId AND UserId = @userId AND Type = 'folder'";
                selectCommand.Parameters.AddWithValue("@ParentId", parentId);
                selectCommand.Parameters.AddWithValue("@userId", userId);

                var parentPath = selectCommand.ExecuteScalar()?.ToString();

                if (parentPath == null)
                {
                    Console.WriteLine($"Parent directory with ID '{parentId}' not found.");
                    return false;
                }

                // Build path inside parent directory
                dirPath = Path.Combine(parentPath, dirName);
            }
            else
            {
                // No parent - create in root uploads directory
                dirPath = Path.Combine("Storage", "uploads", dirName);
            }

            // Check if directory already exists
            if (Directory.Exists(dirPath))
            {
                Console.WriteLine($"Directory '{dirName}' already exists.");
                return false;
            }

            // Create physical directory
            Directory.CreateDirectory(dirPath);

            // Insert folder into Files table with Type = 'folder'
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"INSERT INTO Files (Id, UserId, Name, Path, Type, Visibility, Size, UploadTime, ParentId) 
                                         VALUES (@id, @userId, @name, @path, @type, @visibility, @size, @uploadTime, @parentId)";

            insertCommand.Parameters.AddWithValue("@id", folderId);
            insertCommand.Parameters.AddWithValue("@userId", userId);
            insertCommand.Parameters.AddWithValue("@name", dirName);
            insertCommand.Parameters.AddWithValue("@path", dirPath);
            insertCommand.Parameters.AddWithValue("@type", "folder");
            insertCommand.Parameters.AddWithValue("@visibility", "private");
            insertCommand.Parameters.AddWithValue("@size", 0); // Folders have 0 size
            insertCommand.Parameters.AddWithValue("@uploadTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            insertCommand.Parameters.AddWithValue("@parentId", parentId ?? (object)DBNull.Value);

            insertCommand.ExecuteNonQuery();

            Console.WriteLine($"Directory '{dirName}' created successfully at: {dirPath}");
            Console.WriteLine($"Folder ID: {folderId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating directory: {ex.Message}");
            return false;
        }
    }

    public static bool Publish(string fileId)
    {
        return ChangeVisibility(fileId, "public");
    }

    public static bool Unpublish(string fileId)
    {
        return ChangeVisibility(fileId, "private");
    }

    private static bool ChangeVisibility(string fileId, string visibility)
    {
        try
        {
            var userId = Redis.GetCurrentUserId() ?? throw new InvalidOperationException("User not logged in.");
            using var connection = Database.Database.GetConnection();
            connection.Open();

            // Check if file exists and belongs to user
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT Name, Type FROM Files WHERE Id = @fileId AND UserId = @userId";
            checkCommand.Parameters.AddWithValue("@fileId", fileId);
            checkCommand.Parameters.AddWithValue("@userId", userId);

            using var reader = checkCommand.ExecuteReader();
            if (!reader.Read())
            {
                Console.WriteLine($"File with ID '{fileId}' not found or you don't have permission.");
                return false;
            }

            var fileName = reader["Name"].ToString();
            var fileType = reader["Type"].ToString();
            reader.Close();

            // Update visibility
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = "UPDATE Files SET Visibility = @visibility WHERE Id = @fileId";
            updateCommand.Parameters.AddWithValue("@visibility", visibility);
            updateCommand.Parameters.AddWithValue("@fileId", fileId);

            var rowsAffected = updateCommand.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                var action = visibility == "public" ? "published" : "unpublished";
                Console.WriteLine($"{fileType?.ToUpper() ?? "ITEM"} '{fileName}' has been {action}.");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing visibility: {ex.Message}");
            return false;
        }
    }

    public static List<FileMeta> GetPublicFiles()
    {
        var files = new List<FileMeta>();

        try
        {
            using var connection = Database.Database.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Files WHERE Visibility = 'public' ORDER BY UploadTime DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                files.Add(new FileMeta
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving public files: {ex.Message}");
        }

        return files;
    }

    // Define the FileMeta class
    public class FileMeta
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string Type { get; set; } = "file";
        public string Visibility { get; set; } = "private";
        public long Size { get; set; }
        public DateTime UploadTime { get; set; }
        public string? ParentId { get; set; }
    }
}