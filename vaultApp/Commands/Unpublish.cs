using vaultApp.Services;

namespace vaultApp.Commands
{
    public class UnpublishCommand
    {
        public static void Handle(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: unpublish <file_id>");
                Console.WriteLine("Example: unpublish 123");
                return;
            }

            string fileId = args[0];

            try
            {
                bool success = FileService.Unpublish(fileId);
                
                if (success)
                {
                    Console.WriteLine($"File/folder with ID {fileId} has been unpublished successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to unpublish file/folder with ID {fileId}. Please check if the ID exists and you have permission to modify it.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unpublishing file/folder: {ex.Message}");
            }
        }
    }
}
