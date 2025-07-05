using System.Text.Json;
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
        File.Copy(filePath, destinationPath, true);
        var fileInfo = new FileInfo(destinationPath);
        var fileSize = fileInfo.Length; // in bytes;
        var uploadTime = DateTime.Now;
        var meta = new FileMeta
        {
            Id = fileId,
            Name = fileName,
            Path = destinationPath,
            Size = fileSize,
            UploadTime = uploadTime
        };
        var metaFilePath = Path.Combine("Storage", $"metadata.json");
        List<FileMeta> existingMeta = new();
        if (File.Exists(metaFilePath))
        {
            string json = File.ReadAllText(metaFilePath);
            if (!string.IsNullOrEmpty(json))
            {
                existingMeta = JsonSerializer.Deserialize<List<FileMeta>>(json) ?? new List<FileMeta>();
            }
        }
        if (existingMeta.Any(m => m.Name == fileName))
        {
            throw new InvalidOperationException($"File with the name '{fileName}' already exists in the Vault!.");
        }

        existingMeta.Add(meta);
        var newJson = JsonSerializer.Serialize(existingMeta, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(metaFilePath, newJson);

        return fileId;
    }
<<<<<<< HEAD

    // Retrieves all files from the vault for listing or reading
=======
>>>>>>> 685d007ffb6b30c33911ceabb1d0be355d85e9d6
    public static List<FileMeta> GetAllFiles()
    {
        var metaFilePath = Path.Combine("Storage", "metadata.json");
        if (!File.Exists(metaFilePath))
        {
            return new List<FileMeta>();
        }

        string json = File.ReadAllText(metaFilePath);
        if (string.IsNullOrEmpty(json))
        {
            Console.WriteLine("No files found in the Vault.");
            return new List<FileMeta>();
        }

        return JsonSerializer.Deserialize<List<FileMeta>>(json) ?? new List<FileMeta>();
    }

<<<<<<< HEAD
    // Deletes a file from the vault by its ID
=======
>>>>>>> 685d007ffb6b30c33911ceabb1d0be355d85e9d6
    public static bool Delete(string fileId)
    {
        string metadataFile = "Storage/metadata.json";

        if (!File.Exists(metadataFile))
            return false;

        var json = File.ReadAllText(metadataFile);
        var metadata = JsonSerializer.Deserialize<List<FileMeta>>(json) ?? new();

        var match = metadata.FirstOrDefault(m => m.Id == fileId);
        if (match is null)
            return false;

<<<<<<< HEAD

        if (File.Exists(match.Path))
            File.Delete(match.Path);


=======
        
        if (File.Exists(match.Path))
            File.Delete(match.Path);

        
>>>>>>> 685d007ffb6b30c33911ceabb1d0be355d85e9d6
        metadata.Remove(match);
        var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(metadataFile, updatedJson);

        return true;
<<<<<<< HEAD
    }
=======
>>>>>>> 685d007ffb6b30c33911ceabb1d0be355d85e9d6
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