using vaultApp.Services;

namespace vaultApp.Commands;

public static class LogoutCommand
{
    public static void Handle()
    {
        UserServices.Logout();
        Console.WriteLine("You're logged out now!");
    }

}
