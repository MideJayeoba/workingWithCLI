using vaultApp.Commands;

namespace vaultApp.cli
{
    public class CommandRouter
    {
        public static void Route(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a command (e.g., upload <filepath>)");
                return;
            }

            var command = args[0].ToLower();

            switch (command)
            {
                case "upload":
                    UploadCommand.Handle(args.Skip(1).ToArray());
                    break;
                case "list":
                    ListCommand.Handle(args.Skip(1).ToArray());
                    break;
                case "read":
                    ReadCommand.Handle(args.Skip(1).ToArray());
                    break;
                case "delete":
                    DeleteCommand.Handle(args.Skip(1).ToArray());
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }
    }
}
