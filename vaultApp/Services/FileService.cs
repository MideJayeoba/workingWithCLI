using vaultApp.Database;
using System.Text.Json;
using vaultApp.Repositories;
using vaultApp.Models;


namespace vaultApp.Services;


public static class FileService
{
    // Shared storage paths
    private static readonly string SharedStorageBase = Path.GetFullPath(Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "SharedStorage"));
    private static readonly string UploadsDirectory = Path.Combine(SharedStorageBase, "uploads");
    private static readonly string ThumbnailsDirectory = Path.Combine(SharedStorageBase, "thumbnails");
    private static readonly string MetadataPath = Path.Combine(SharedStorageBase, "metadata.json");

    private static string UserId => Redis.GetCurrentUserId() ?? throw new InvalidOperationException("User not logged in.");


    // Uploads a file to the vault and returns its ID
    public static string Upload(string filePath, string? parentId = null)
    {
        // var filePath = file[0] ?? throw new ArgumentNullException(nameof(file));
        // string? parentId = file.Length > 1 ? file[1] : null;

        FileEntity? fileinfo = null;
        string? folderPath = null;
        if (parentId != null)
        {
            fileinfo = FileRepo.GetById(parentId, UserId);
            folderPath = fileinfo?.Path;
        }

        // Ensure shared uploads directory exists
        if (!Directory.Exists(UploadsDirectory))
        {
            Directory.CreateDirectory(UploadsDirectory);
        }

        var fileId = Guid.NewGuid().ToString("N")[..10];
        var fileName = Path.GetFileName(filePath);
        string destinationPath;
        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
        {
            // folderPath already contains the full path (Storage\uploads\Mide)
            destinationPath = Path.Combine(folderPath, fileName); // ✅ Correct!
        }
        else
        {
            destinationPath = Path.Combine(UploadsDirectory, fileName);
        }

        // var destDir = Path.GetDirectoryName(destinationPath);
        // if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
        // {
        //     Directory.CreateDirectory(destDir);
        // }

        // Check if file already exists in database
        try
        {
            if (FileRepo.Exists(fileName, UserId, parentId))
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
                    FileId = fileId,
                    Path = destinationPath,
                    ImageName = Path.GetFileNameWithoutExtension(fileName)
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
            var fileEntity = new FileEntity
            {
                Id = fileId,
                UserId = UserId,
                Name = fileName,
                Path = destinationPath,
                Size = fileSize,
                Type = fileType,
                Visibility = "private",
                ParentId = parentId,
                UploadTime = DateTime.Now
            };

            FileRepo.Create(fileEntity);

            // we save the metadata directly to SharedStorage/metadata.json using folderEntity
            SaveMetadata(fileEntity);
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

            // Delete physical file or directory
            if (fileInfo.Type == "folder")
            {
                // Delete directory and all its contents
                if (Directory.Exists(fileInfo.Path))
                {
                    Directory.Delete(fileInfo.Path, true); // true = recursive delete
                }
            }
            else
            {
                // Delete regular file
                if (File.Exists(fileInfo.Path))
                {
                    File.Delete(fileInfo.Path);
                }

                // Delete thumbnail if it exists (for images)
                if (fileInfo.Type == "image")
                {
                    var imageNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    var thumbnailPath = Path.Combine(ThumbnailsDirectory, $"{imageNameWithoutExt}.jpg");
                    if (File.Exists(thumbnailPath))
                    {
                        File.Delete(thumbnailPath);
                    }
                }
            }

            // Delete metadata
            if (File.Exists(MetadataPath))
            {
                try
                {
                    var jsonContent = File.ReadAllText(MetadataPath);
                    // Only try to deserialize if the file has valid JSON array content
                    if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent.Trim().StartsWith("["))
                    {
                        var metadataList = JsonSerializer.Deserialize<List<FileEntity>>(jsonContent) ?? new List<FileEntity>();
                        metadataList.RemoveAll(f => f.Id == fileId);
                        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                        var updatedJsonString = JsonSerializer.Serialize(metadataList, jsonOptions);
                        File.WriteAllText(MetadataPath, updatedJsonString);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to delete metadata: {ex.Message}");
                    return false;
                }
            }
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
                dirPath = Path.Combine(UploadsDirectory, dirName);
            }
            if (Directory.Exists(dirPath))
            {
                Console.WriteLine($"Directory '{dirName}' already exists.");
                return false;
            }



            // Create folder entity using your FileEntity constructor/factory
            var folderEntity = new FileEntity
            {
                Id = folderId,
                UserId = UserId,
                Name = dirName,
                Path = dirPath,
                Size = 0,
                Type = "folder",
                Visibility = "private",
                ParentId = parentId,
                UploadTime = DateTime.Now
            };

            // Save to database using repository
            var createdId = FileRepo.Create(folderEntity);
            // Create physical directory
            Directory.CreateDirectory(dirPath);

            SaveMetadata(folderEntity);
            return true;
        }
        catch (Exception ex)
        {
            // clean up if directory creation fails
            if (Directory.Exists(dirName))
            {
                Directory.Delete(dirName, true);
            }
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

    public static void SaveMetadata(FileEntity fileEntity)
    {
        try
        {
            List<FileEntity> list = new List<FileEntity>();

            // Check if file exists and has content
            if (File.Exists(MetadataPath))
            {
                var jsonContent = File.ReadAllText(MetadataPath);
                // Only try to deserialize if the file has actual content and starts with '['
                if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent.Trim().StartsWith("["))
                {
                    list = JsonSerializer.Deserialize<List<FileEntity>>(jsonContent) ?? new List<FileEntity>();
                }
            }

            list.Add(fileEntity);
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var updatedJsonString = JsonSerializer.Serialize(list, jsonOptions);
            File.WriteAllText(MetadataPath, updatedJsonString);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to save metadata: {ex.Message}");
        }
    }

}