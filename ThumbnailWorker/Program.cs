using System.Threading.Tasks;
using ThumbnailWorker;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Thumbnail Worker is starting...");
        Worker.ProcessQueue();   
    }
}