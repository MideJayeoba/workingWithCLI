using System.Threading.Tasks;
using ThumbnailWorker;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Thumbnail Worker is starting...");
        await Worker.ProcessQueue();
    }
}