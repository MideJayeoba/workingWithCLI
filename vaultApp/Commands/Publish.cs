using vaultApp.Services;

namespace vaultApp.Commands
{
    public class PublishCommand
    {
        public static void Handle(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: publish <file_id>");
                Console.WriteLine("Example: publish 123");
                return;
            }

            string fileId = args[0];

            try
            {
                bool success = FileService.Publish(fileId);

                if (success)
                {
                    Console.WriteLine($"File/folder with ID {fileId} has been published successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to publish file/folder with ID {fileId}. Please check if the ID exists and you have permission to modify it.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing file/folder: {ex.Message}");
            }
        }
    }
}
