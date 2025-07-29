using vaultApp.Database;
using System.Text.Json;
using vaultApp.Repositories;
using vaultApp.Models;


namespace vaultApp.Services;


public static class FileService
{
    // private static string? dirPath;

    private static string UserId => Redis.GetCurrentUserId() ?? throw new InvalidOperationException("User not logged in.");


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
            if (FileRepo.Exists(fileName, UserId))
            {
                throw new InvalidOperationException($"File with the name '{fileName}' already exists in the Vault!");
            }

            // Copy file to storage
            File.Copy(filePath, destinationPath, true);
            var fileInfo = new FileInfo(destinationPath);
            var fileSize = fileInfo.Length; // Replace with actual user ID logic
            var uploadTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // To check if the file is an image
            var fileType = Path.GetExtension(fileName).ToLower();
            if (fileType == ".jpg" || fileType == ".jpeg" || fileType == ".png" || fileType == ".gif")
            {
                // If it's an image, set type to "image"
                fileType = "image";
                // Create a job to generate thumbnail
                var job = new
                {
                    file_id = fileId,
                    path = destinationPath
                };
                string jobJson = JsonSerializer.Serialize(job);

                // Here you would typically enqueue the job to a background worker or similar
                Redis.CreateJobQueue(jobJson);
            }
            else
            {
                // Otherwise, default to "file"
                fileType = "file";
            }
            var fileEntity = new FileEntity(fileId, UserId, fileName, destinationPath, fileSize)
            {
                UploadTime = DateTime.Parse(uploadTime),
                Type = fileType
            };
            FileRepo.Create(fileEntity);

            // Return the fileId after successful upload
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
    public static List<FileEntity> GetAllFilesbyUSerId()
    {
        var userFiles = FileRepo.GetAllByUserId(UserId);

        return userFiles;
    }

    public static bool Delete(string fileId)
    {
        try
        {
            // Get file info first (for file path)
            var fileInfo = FileRepo.GetById(fileId, UserId);
            if (fileInfo == null) return false;

            // Delete from database
            bool dbDeleted = FileRepo.Delete(fileId);
            if (!dbDeleted) return false;

            // Delete physical file
            if (File.Exists(fileInfo.Path))
            {
                File.Delete(fileInfo.Path);
            }

            // Delete thumbnail if it exists
            var thumbnailPath = Path.Combine("Storage", "thumbnails", $"{fileId}.jpg");
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }

            Console.WriteLine($"File '{fileInfo.Name}' deleted successfully.");
            return true;
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
            string dirPath;

            // Handle parent directory logic
            if (parentId != null)
            {
                // Get parent directory info from repository
                var parentDir = FileRepo.GetById(parentId, UserId);
                if (parentDir == null || parentDir.Type != "folder")
                {
                    Console.WriteLine($"Parent directory with ID '{parentId}' not found.");
                    return false;
                }
                else if (string.IsNullOrEmpty(parentDir.Path))
                {
                    Console.WriteLine($"Parent directory path is invalid.");
                    return false;
                }
                else
                    dirPath = Path.Combine(parentDir.Path, dirName);
            }
            else
            {
                // No parent - create in root uploads directory
                dirPath = Path.Combine("Storage", "uploads", dirName);
            }

            // Check if directory already exists physically
            if (Directory.Exists(dirPath))
            {
                Console.WriteLine($"Directory '{dirName}' already exists.");
                return false;
            }

            // Create physical directory
            Directory.CreateDirectory(dirPath);

            // Create folder entity using your FileEntity constructor/factory
            var folderEntity = FileEntity.CreateFolder(folderId, UserId, dirName, dirPath, 0, parentId);

            // Save to database using repository
            var createdId = FileRepo.Create(folderEntity);
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
            var fileExists = FileRepo.Exists(fileId, UserId);
            var fileinfo = FileRepo.GetById(fileId, UserId);
            // Update visibility
            if (!fileExists || fileinfo == null)
            {
                Console.WriteLine($"File with ID '{fileId}' does not exist.");
                return false;
            }
            var changeVisibility = FileRepo.ChangeVisibility(fileId, visibility, UserId);
            if (!changeVisibility)
            {
                return false;
            } 
            else
            {
                var action = visibility == "public" ? "published" : "unpublished";
                Console.WriteLine($"{fileinfo.Type.ToUpper() ?? "ITEM"} '{fileinfo.Name}' has been {action}.");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing visibility: {ex.Message}");
            return false;
        }
    }

    public static List<FileEntity> GetPublicFiles()
    {

        try
        {
            var publicFiles = FileRepo.GetPublicFiles();
            return publicFiles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving public files: {ex.Message}");
        }
        return [];
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