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
    public static string Upload(string[] file)
    {
        var filePath = file[0] ?? throw new ArgumentNullException(nameof(file));
        string? parentId = file.Length > 1 ? file[1] : null;

        FileEntity? fileinfo = null;
        string? folderPath = null;
        if (parentId != null)
        {
            fileinfo = FileRepo.GetById(parentId, UserId);
            folderPath = fileinfo?.Path;
        }
        if (!Directory.Exists("Storage/uploads"))

        {
            Directory.CreateDirectory("Storage/uploads");
        }

        var fileId = Guid.NewGuid().ToString().Substring(0, 10);
        var fileName = Path.GetFileName(filePath);
        string destinationPath;
        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
        {
            // folderPath already contains the full path (Storage\uploads\Mide)
            destinationPath = Path.Combine(folderPath, fileName); // ✅ Correct!
        }
        else
        {
            destinationPath = Path.Combine("Storage", "uploads", fileName);
        }

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
            string jobJson = "";
            var fileType = Path.GetExtension(fileName).ToLower();
            if (fileType == ".jpg" || fileType == ".jpeg" || fileType == ".png" || fileType == ".gif")
            {
                // If it's an image, set type to "image"
                fileType = "image";
                // Create a job to generate thumbnail
                var job = new
                {
                    file_id = fileId,
                    path = destinationPath,
                    image_name = Path.GetFileNameWithoutExtension(fileName)
                };
                jobJson = JsonSerializer.Serialize(job);
            }
            else
            {
                // Otherwise, default to "file"
                fileType = "file";
            }
            var folderEntity = new FileEntity
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

            // Add to metadata.json file - handle multiple files properly
            var metadataPath = Path.Combine("storage", "metadata.json");
            List<FileEntity> metadataList;

            if (File.Exists(metadataPath))
            {
                try
                {
                    var existingJson = File.ReadAllText(metadataPath);

                    // Check if file is empty or whitespace
                    if (string.IsNullOrWhiteSpace(existingJson))
                    {
                        metadataList = new List<FileEntity>();
                    }
                    else
                    {
                        // Try to deserialize as array first
                        try
                        {
                            metadataList = JsonSerializer.Deserialize<List<FileEntity>>(existingJson) ?? new List<FileEntity>();
                        }
                        catch
                        {
                            // If it fails, try as single object and convert to array
                            var singleItem = JsonSerializer.Deserialize<FileEntity>(existingJson);
                            metadataList = singleItem != null ? new List<FileEntity> { singleItem } : new List<FileEntity>();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: Could not read metadata.json: {ex.Message}");
                    metadataList = new List<FileEntity>();
                }
            }
            else
            {
                metadataList = new List<FileEntity>();
            }

            // Add new file to the list
            metadataList.Add(folderEntity);

            // Save back as array
            var jsonData = JsonSerializer.Serialize(metadataList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(metadataPath, jsonData);

            FileRepo.Create(folderEntity);

            if (!string.IsNullOrEmpty(jobJson))
            {
                Redis.CreateJobQueue(jobJson);
            }

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

            // Delete the object in the metadata.json file using the id given
            var metadataPath = Path.Combine("storage", "metadata.json");
            if (File.Exists(metadataPath))
            {
                try
                {
                    var jsonContent = File.ReadAllText(metadataPath);

                    // Try to deserialize as array first
                    try
                    {
                        var metadataArray = JsonSerializer.Deserialize<List<FileEntity>>(jsonContent);
                        if (metadataArray != null)
                        {
                            var itemToRemove = metadataArray.FirstOrDefault(m => m.Id == fileId);
                            if (itemToRemove != null)
                            {
                                metadataArray.Remove(itemToRemove);

                                if (metadataArray.Count > 0)
                                {
                                    // Write back the updated array
                                    var updatedJson = JsonSerializer.Serialize(metadataArray, new JsonSerializerOptions { WriteIndented = true });
                                    File.WriteAllText(metadataPath, updatedJson);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // If array deserialization fails, try single object
                        var singleMetadata = JsonSerializer.Deserialize<FileEntity>(jsonContent);
                        if (singleMetadata != null && singleMetadata.Id == fileId)
                        {
                            // This is the only file, delete the metadata file
                            File.Delete(metadataPath);
                            Console.WriteLine("📄 Metadata file removed");
                        }
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"⚠️ Warning: Could not process metadata.json: {ex.Message}");
                    // Continue anyway since database deletion succeeded
                }
            }

            // Delete thumbnail if it exists (match ThumbnailWorker output format)
            var thumbnailPath = Path.Combine("Storage", "thumbnails", $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.jpg");
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
                Console.WriteLine("🖼️ Thumbnail deleted");
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
                dirPath = Path.Combine("Storage", "uploads", dirName);
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

            // Add to metadata.json file - same pattern as Upload method
            var metadataPath = Path.Combine("storage", "metadata.json");
            List<FileEntity> metadataList;

            if (File.Exists(metadataPath))
            {
                try
                {
                    var existingJson = File.ReadAllText(metadataPath);

                    // Check if file is empty or whitespace
                    if (string.IsNullOrWhiteSpace(existingJson))
                    {
                        metadataList = new List<FileEntity>();
                    }
                    else
                    {
                        // Try to deserialize as array first
                        try
                        {
                            metadataList = JsonSerializer.Deserialize<List<FileEntity>>(existingJson) ?? new List<FileEntity>();
                        }
                        catch
                        {
                            // If it fails, try as single object and convert to array
                            var singleItem = JsonSerializer.Deserialize<FileEntity>(existingJson);
                            metadataList = singleItem != null ? new List<FileEntity> { singleItem } : new List<FileEntity>();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: Could not read metadata.json: {ex.Message}");
                    metadataList = new List<FileEntity>();
                }
            }
            else
            {
                metadataList = new List<FileEntity>();
            }

            // Add new folder to the list
            metadataList.Add(folderEntity);

            // Save back as array
            var jsonData = JsonSerializer.Serialize(metadataList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(metadataPath, jsonData);

            // Create physical directory
            Directory.CreateDirectory(dirPath);
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

            var fileinfo = FileRepo.GetById(fileId, UserId);
            if (fileinfo == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(fileinfo.Name))
            {
                Console.WriteLine($"File name is null or empty for file ID '{fileId}'.");
                return false;
            }
            var fileExists = FileRepo.Exists(fileinfo.Name, UserId);

            // Update visibility and update it also in the metadata.json file
            if (!fileExists)
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
                // Update metadata.json
                var metadataPath = Path.Combine("storage", "metadata.json");
                if (File.Exists(metadataPath))
                {
                    try
                    {
                        var jsonContent = File.ReadAllText(metadataPath);
                        var metadataArray = JsonSerializer.Deserialize<List<FileEntity>>(jsonContent) ?? new List<FileEntity>();

                        // Find the file entity to update
                        var itemToUpdate = metadataArray.FirstOrDefault(m => m.Id == fileId);
                        if (itemToUpdate != null)
                        {
                            itemToUpdate.Visibility = visibility;

                            // Write back the updated array
                            var updatedJson = JsonSerializer.Serialize(metadataArray, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(metadataPath, updatedJson);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"⚠️ Warning: Could not process metadata.json: {ex.Message}");
                    }
                }
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

    // Get directory contents by parent ID
    public static List<FileEntity> GetDirectoryContents(string? parentId)
    {
        try
        {
            var items = FileRepo.GetByParentId(parentId, UserId);
            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing directory contents: {ex.Message}");
            return [];
        }
    }
}