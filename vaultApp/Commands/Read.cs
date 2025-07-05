// you input your UUID and it give the information about it from the metadata file
using vaultApp.Services;

public static class ReadCommand
{
    public static void Handle(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: read <file_id>");
            return;
        }

        var fileId = args[0];
        var fileMetas = FileService.GetAllFiles();
        var fileMeta = fileMetas.FirstOrDefault(f => f.Id == fileId);

        if (fileMeta == null)
        {
            Console.WriteLine($"File with ID '{fileId}' not found in the Vault.");
            return;
        }

        Console.WriteLine("---------------------------------------------------------------------------------");
        Console.WriteLine($"ID: {fileMeta.Id}  | Name: {fileMeta.Name}  | Size: {fileMeta.Size} -> {(fileMeta.Size / (1024.0 * 1024.0)):F2}MB  | Time: {fileMeta.UploadTime}");
    }
}
