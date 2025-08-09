using vaultApp.Services;
using vaultApp.Database;
using vaultApp.Repositories;

namespace vaultApp.Commands
{
    public static class LsCommand
    {
        public static void Handle(string[] args)
        {
            string? parentId = null;

            // If argument provided, use it as parent_id
            if (args.Length > 0)
            {
                parentId = args[0];
            }

            // Get files and folders in the specified directory using FileService
            var items = FileService.GetAllFilesbyUSerId()
                .Where(f => f.ParentId == parentId)
                .ToList();

            if (items == null || !items.Any())
            {
                if (parentId == null)
                {
                    Console.WriteLine("No files or folders found in root directory.");
                }
                else
                {
                    Console.WriteLine($"No files or folders found in directory with ID: {parentId}");
                }
                return;
            }

            Console.WriteLine("Directory contents:");
            Console.WriteLine();

            // Sort items: folders first, then files
            var sortedItems = items.OrderBy(item => item.Type == "file" ? 1 : 0)
                                  .ThenBy(item => item.Name);

            foreach (var item in sortedItems)
            {
                string prefix = item.Type == "folder" ? "[folder]" : "[file]";
                string sizeInfo = item.Type == "file" ? $" ({item.Size} bytes)" : "";

                Console.WriteLine($"{prefix} {item.Name}{sizeInfo}");

                // Show ID for reference
                Console.WriteLine($"    ID: {item.Id}");
                Console.WriteLine();
            }
        }
    }
}
