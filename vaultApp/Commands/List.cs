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
            
            var fileMetas = FileService.GetAllFilesbyUSerId();
            if (fileMetas == null || !fileMetas.Any())
            {
                Console.WriteLine("User has not uploaded any file yet.");
                Console.WriteLine("You can run the upload {filepath} to upload files.");
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