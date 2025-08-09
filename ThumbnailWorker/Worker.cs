using StackExchange.Redis;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace ThumbnailWorker
{
    public static class Worker
    {
        // Shared storage paths
        private static readonly string SharedStorageBase = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "SharedStorage"));
        private static readonly string ThumbnailsDirectory = Path.Combine(SharedStorageBase, "thumbnails");

        public static void ProcessQueue()
        {
            Console.WriteLine("Connecting to Redis...");
            var connection = ConnectionMultiplexer.Connect("localhost:6379");
            var database = connection.GetDatabase();

            if (database is null)
            {
                Console.WriteLine("❌ ThumbnailWorker: Failed to connect to Redis database");
                return;
            }

            Console.WriteLine("Connected to Redis. Waiting for jobs...");

            while (true)
            {
                var jobData = database.ListLeftPop("new_job_queue");

                if (!jobData.HasValue)
                {
                    Thread.Sleep(1000); // Use Thread.Sleep for synchronous operation
                    continue;
                }
                
                Console.WriteLine("***         ***");
                Console.WriteLine($"Processing... ");

                //Deserialize the jobData
                var job = JsonSerializer.Deserialize<Job>(jobData.ToString());
                if (job == null)
                {
                    Console.WriteLine("❌ ThumbnailWorker: Failed to deserialize job data");
                    continue;
                }

                string fileId = job.FileId;
                var path = job.Path; // Use absolute path from job

                // Create shared thumbnails directory if it doesn't exist
                if (!Directory.Exists(ThumbnailsDirectory))
                {
                    Directory.CreateDirectory(ThumbnailsDirectory);
                }

                // Create the full output path with thumbnail filename
                var outputPath = Path.Combine(ThumbnailsDirectory, $"{job.ImageName}.jpg");

                try
                {
                    Console.WriteLine($"Creating thumbnail... ");
                    using var image = Image.Load(path);
                    image.Mutate(x => x.Resize(100, 100));
                    image.SaveAsJpeg(outputPath);
                    Console.WriteLine($"Done");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ThumbnailWorker: Error processing {job.ImageName}: {ex.Message}");
                }
            }
        }
    }

    public class Job
    {
        public required string FileId { get; set; }
        public required string Path { get; set; }
        public required string ImageName { get; set; }
    }
}
