using vaultApp.Models;
using vaultApp.Database;
using System.Data;

namespace vaultApp.Repositories
{
    public class FileRepo
    {
        // Method to create new file/folder
        public async Task<string> CreateAsync(FileEntity file)
        {
            try
            {
                using var connection = Database.Database.GetConnection();
                await connection.OpenAsync();

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

                await command.ExecuteNonQueryAsync();
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

        // Method to get file by ID
        public async Task<FileEntity?> GetByIdAsync(string id, string userId)
        {
            // We go add code here
        }

        // Method to get all files for a user
        public async Task<List<FileEntity>> GetAllByUserIdAsync(string userId)
        {
            // We go add code here
        }

        // Method to delete file
        public async Task<bool> DeleteAsync(string id, string userId)
        {
            // We go add code here
        }
    }
}