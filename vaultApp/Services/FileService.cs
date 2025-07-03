using System.Text.Json;
namespace vaultApp.Services;

public static class FileService
{
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
        
        existingMeta.Add(meta);
        var newJson = JsonSerializer.Serialize(existingMeta, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(metaFilePath, newJson);

        return fileId;
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