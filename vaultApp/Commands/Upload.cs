using System.Threading.Tasks;
using vaultApp.Services;

namespace vaultApp.Commands
{
    public static class UploadCommand
    {
        public static void Handle(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: upload <file_path>");
                return;
            }

            var filePath = args[0];
            string? parentId = args.Length > 1 ? args[1] : null;
            if (Path.GetExtension(filePath) == ".exe")
            {
                Console.WriteLine("Oops .exe files are allowed here please.");
                return;
            }
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }
            else
            {
                string fileId = FileService.Upload(filePath, parentId);
                Console.WriteLine($"File uploaded successfully! ID: {fileId}");
            }
        }

    }
}
