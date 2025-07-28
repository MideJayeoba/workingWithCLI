using System.Diagnostics.Tracing;
using vaultApp.Services;

namespace vaultApp.Commands;

public static class MkDirCommand
{
    public static void Handle(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine(args);
            return;
        }

        string directoryName = args[0];
        var Create = FileService.Mkdir(args);

        if (Create)
        {
            Console.WriteLine($"✅ Directory '{directoryName}' created successfully.");
            return;
        }
        else
        {
            Console.WriteLine($"❌ Failed! Directory '{directoryName}' already exists.");
            return;
        }


    }
}