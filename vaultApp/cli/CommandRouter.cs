using vaultApp.Commands;
using vaultApp.Database;

namespace vaultApp.cli
{
    public class CommandRouter
    {
        public static void Route(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a command. Use 'help' to see available commands.");
                return;
            }

            var command = args[0].ToLower();
            if (command == "register")
            {
                RegisterCommand.Handle(args.Skip(1).ToArray());
                return;
            }
            if (command == "login")
            {
                while (Redis.IsLoggedIn())
                {
                    Console.WriteLine("A user is logged in already.");
                    Console.WriteLine("You can logout to log in another user");
                    return;
                }
                Logincommand.Handle(args.Skip(1).ToArray());
                return;
            }
            if (command == "dbview")
            {
                DbViewCommand.Handle(args.Skip(1).ToArray());
                return;
            }
            // if (command == "listpublic")
            // {
            //     ListPublicCommand.Handle(args.Skip(1).ToArray());
            //     return;
            // }

            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a command (e.g., upload <filepath>)");
                return;
            }

            if (!Redis.IsLoggedIn())
            {
                Console.WriteLine("No user is logged in.");
                return;
            }
            else
            {
                switch (command)
                {
                    case "upload":
                        UploadCommand.Handle(args.Skip(1).ToArray());
                        break;
                    case "list":
                        ListCommand.Handle(args.Skip(1).ToArray());
                        break;
                    case "ls":
                        LsCommand.Handle(args.Skip(1).ToArray());
                        break;
                    case "read":
                        ReadCommand.Handle(args.Skip(1).ToArray());
                        break;
                    case "delete":
                        DeleteCommand.Handle(args.Skip(1).ToArray());
                        break;
                    case "logout":
                        LogoutCommand.Handle();
                        break;
                    case "mkdir":
                        MkDirCommand.Handle(args.Skip(1).ToArray());
                        break;
                    case "publish":
                        PublishCommand.Handle(args.Skip(1).ToArray());
                        break;
                    case "unpublish":
                        UnpublishCommand.Handle(args.Skip(1).ToArray());
                        break;
                    case "clearqueue":
                        Redis.ClearJobQueue();
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        break;
                }
            }
        }
    }
}
