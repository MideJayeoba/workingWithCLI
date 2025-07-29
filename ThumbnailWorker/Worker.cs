using StackExchange.Redis;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace ThumbnailWorker
{
    public static class Worker
    {
        public static void ProcessQueue()
        {
            var connection = ConnectionMultiplexer.Connect("localhost:6379");
            var database = connection.GetDatabase();

            if (database is null) return;

            while (true)
            {
                var jobData = database.ListLeftPop("new_job_queue");

                if (!jobData.HasValue)
                {
                    Task.Delay(1000);
                    continue;
                }

                //Deserialize the jobData
                var job = JsonSerializer.Deserialize<Job>(jobData.ToString());
                if (job == null)
                {
                    // Skip this iteration if deserialization failed
                    continue;
                }
                string fileId = job.FileId;
                var path = Path.Combine("..", "vaultapp", job.Path);

                // Create output path to vaultApp's storage/thumbnails folder
                string vaultAppStoragePath = Path.Combine("..", "vaultApp", "Storage", "thumbnails");

                // Create thumbnails directory if it doesn't exist
                if (!Directory.Exists(vaultAppStoragePath))
                {
                    Directory.CreateDirectory(vaultAppStoragePath);
                }

                // Create the full output path with thumbnail filename
                var outputPath = Path.Combine(vaultAppStoragePath, fileId);

                using var image = Image.Load(path);
                image.Mutate(x => x.Resize(100, 100));
                image.SaveAsJpeg(outputPath);
            }
        }
    }

    public class Job
    {
        required public string FileId { get; set; } 
        required public string Path { get; set; }
    }
}
