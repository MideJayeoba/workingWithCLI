using vaultApp.Services;
using vaultApp.Database;

namespace vaultApp.Commands
{
    public static class LsCommand
    {
        public static void Handle(string[] args)
        {
            string? parentId = null;

            // If argument provided, use it as parent_id
            if (args.Length > 0)
            {
                parentId = args[0];
            }

            // Get files and folders in the specified directory
            var items = GetDirectoryContents(parentId);

            if (items == null || !items.Any())
            {
                if (parentId == null)
                {
                    Console.WriteLine("No files or folders found in root directory.");
                }
                else
                {
                    Console.WriteLine($"No files or folders found in directory with ID: {parentId}");
                }
                return;
            }

            Console.WriteLine("Directory contents:");
            Console.WriteLine();

            // Sort items: folders first, then files
            var sortedItems = items.OrderBy(item => item.Type == "file" ? 1 : 0)
                                  .ThenBy(item => item.Name);

            foreach (var item in sortedItems)
            {
                string prefix = item.Type == "folder" ? "[folder]" : "[file]";
                string sizeInfo = item.Type == "file" ? $" ({item.Size} bytes)" : "";

                Console.WriteLine($"{prefix} {item.Name}{sizeInfo}");

                // Show ID for reference
                Console.WriteLine($"    ID: {item.Id}");
                Console.WriteLine();
            }
        }

        private static List<FileService.FileMeta>? GetDirectoryContents(string? parentId)
        {
            var userId = Redis.GetCurrentUserId();
            if (userId == null)
            {
                Console.WriteLine("User not logged in.");
                return null;
            }

            try
            {
                using var connection = Database.Database.GetConnection();
                connection.Open();

                var command = connection.CreateCommand();

                // Query for files and folders in the specified parent directory
                if (parentId == null)
                {
                    // List root directory contents (where ParentId is NULL)
                    command.CommandText = @"
                        SELECT Id, Name, Path, Type, Size, UploadTime, ParentId, Visibility 
                        FROM Files 
                        WHERE UserId = @userId AND ParentId IS NULL 
                        ORDER BY Type DESC, Name ASC";
                }
                else
                {
                    // List contents of specific folder
                    command.CommandText = @"
                        SELECT Id, Name, Path, Type, Size, UploadTime, ParentId, Visibility 
                        FROM Files 
                        WHERE UserId = @userId AND ParentId = @parentId 
                        ORDER BY Type DESC, Name ASC";
                    command.Parameters.AddWithValue("@parentId", parentId);
                }

                command.Parameters.AddWithValue("@userId", userId);

                var items = new List<FileService.FileMeta>();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(new FileService.FileMeta
                    {
                        Id = reader["Id"].ToString()!,
                        Name = reader["Name"].ToString()!,
                        Path = reader["Path"].ToString()!,
                        Type = reader["Type"].ToString()!,
                        Size = reader["Size"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Size"]),
                        UploadTime = DateTime.Parse(reader["UploadTime"].ToString()!),
                        ParentId = reader["ParentId"]?.ToString(),
                        Visibility = reader["Visibility"].ToString()!
                    });
                }

                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing directory contents: {ex.Message}");
                return null;
            }
        }
    }
}
