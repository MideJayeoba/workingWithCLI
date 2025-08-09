// using vaultApp.Services;

// namespace vaultApp.Commands
// {
//     public static class MetadataCommand
//     {
//         public static void Handle(string[] args)
//         {
//             if (args.Length == 0)
//             {
//                 // Show all metadata
//                 FileService.ViewMetadata();
//                 return;
//             }

//             string subCommand = args[0].ToLower();
//             switch (subCommand)
//             {
//                 case "view":
//                 case "show":
//                 case "list":
//                     FileService.ViewMetadata();
//                     break;

//                 default:
//                     Console.WriteLine("Usage: metadata [view|show|list]");
//                     Console.WriteLine("  metadata view  - View all metadata entries");
//                     break;
//             }
//         }
//     }
// }
