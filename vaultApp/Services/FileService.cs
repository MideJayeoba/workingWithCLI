namespace vaultApp.Services;

public static class FileService
{
    // Uploads a file to the vault and returns its ID
    public static string Upload(string filePath)
    {
        if (!Directory.Exists("Storage/uploads"))
        {
            Directory.CreateDirectory("Storage/uploads");
        }

        var fileId = Guid.NewGuid().ToString().Substring(0, 10);
        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine("Storage", "uploads", fileName);

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
            var fileSize = fileInfo.Length;
            var uploadTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Insert file metadata into database
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO Files (Id, Name, Path, Size, UploadTime)
                VALUES (@id, @name, @path, @size, @uploadTime)";

            insertCommand.Parameters.AddWithValue("@id", fileId);
            insertCommand.Parameters.AddWithValue("@name", fileName);
            insertCommand.Parameters.AddWithValue("@path", destinationPath);
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

        try
        {
            using var connection = Database.Database.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Files ORDER BY UploadTime DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                files.Add(new FileMeta
                {
                    Id = reader["Id"].ToString(),
                    Name = reader["Name"].ToString(),
                    Path = reader["Path"].ToString(),
                    Size = Convert.ToInt64(reader["Size"]),
                    UploadTime = DateTime.Parse(reader["UploadTime"].ToString() ?? string.Empty)
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
                Console.WriteLine($"File with ID '{fileId}' not found.");
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
            if (rowsAffected > 0)
            {
                // Delete physical file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
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


    // Define the FileMeta class
    public class FileMeta
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public long Size { get; set; }
        public DateTime UploadTime { get; set; }
    }
}