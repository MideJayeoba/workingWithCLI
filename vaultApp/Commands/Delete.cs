using vaultApp.Services;

namespace vaultApp.Commands
{
    public static class DeleteCommand
    {
        public static void Handle(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a file ID.");
                return;
            }

            var fileId = args[0];
            bool deleted = FileService.Delete(fileId);

            if (deleted)
                Console.WriteLine($"File deleted successfully!");
            else
                Console.WriteLine($"File with ID {fileId} not found.");
        }
    }
}