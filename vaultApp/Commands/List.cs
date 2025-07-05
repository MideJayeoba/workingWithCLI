using vaultApp.Services;

namespace vaultApp.Commands
{
    public static class ListCommand
    {
        public static void Handle(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine("Usage: list does not require any arguments.");
                return;
            }
            
            var metaFilePath = Path.Combine("Storage", "metadata.json");
            if (!File.Exists(metaFilePath))
            {
                Console.WriteLine("No files found in the Vault.");
                return;
            }
            var fileMetas = FileService.GetAllFiles();
            if (fileMetas == null || !fileMetas.Any())
            {
                Console.WriteLine("No files found in the Vault.");
                return;
            }
            

            foreach (var fileMeta in fileMetas)
            {
                Console.WriteLine("||------------------------------------------------------------------------------------------------------||");
                Console.WriteLine($"ID: {fileMeta.Id}  | Name: {fileMeta.Name}  | Size: {fileMeta.Size} -> {fileMeta.Size/(1024.0*1024.0):F2}MB  | Time: {fileMeta.UploadTime}");

            }
        }
    }
}