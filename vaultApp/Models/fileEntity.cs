namespace vaultApp.Models;

public class FileEntity
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string Type { get; set; } = "file"; // "file" or "folder"
    public string Visibility { get; set; } = "private";
    public long Size { get; set; }
    public DateTime UploadTime { get; set; }
    public string? ParentId { get; set; } = null; // For folders, this is the ID of the parent folder

    public FileEntity()
    {
        UploadTime = DateTime.Now;
    }

    // Main constructor
    public FileEntity(string Id, string userId, string name, string path, long size = 0, string? parentId = null) : this()
    {
        Id = Id ?? throw new ArgumentNullException(nameof(userId));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Size = size;
        ParentId = parentId;
    }

    public static FileEntity CreateFolder(string Id, string userId, string name, string path, string? parentId = null)
    {
        var folder = new FileEntity(Id, userId, name, path, 0, parentId)
        {
            Type = "folder"
        };
        return folder;
    }

}

