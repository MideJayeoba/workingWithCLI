using vaultApp.cli;
using vaultApp.Database;

class Program
{
    static void Main(string[] args)
    {
        // Initialize Redis connection
        Redis.Initialise();

        Database.Initialize();
        CommandRouter.Route(args);
    }
}
