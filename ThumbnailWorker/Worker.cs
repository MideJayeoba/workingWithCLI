using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace ThumbnailWorker
{
    public static class Worker
    {
        public static async Task ProcessQueue()
        {
            var connection = ConnectionMultiplexer.Connect("localhost:6379");
            var database = connection.GetDatabase();

            if (database is null) return;

            while (true)
            {
                var jobData = database.ListLeftPop("new_job_queue");

                if (!jobData.HasValue)
                {
                    await Task.Delay(1000);  // Properly await the delay
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
                // job.Path contains relative path like "Storage\uploads\Mide\meent.jpg" 
                // Need to build absolute path from ThumbnailWorker directory
                var path = Path.Combine("..", "vaultApp", job.Path);

                // Create output path to vaultApp's storage/thumbnails folder
                string vaultAppStoragePath = Path.Combine("..", "vaultApp", "Storage", "thumbnails");

                // Create thumbnails directory if it doesn't exist
                if (!Directory.Exists(vaultAppStoragePath))
                {
                    Directory.CreateDirectory(vaultAppStoragePath);
                }

                // Create the full output path with thumbnail filename
                var outputPath = Path.Combine(vaultAppStoragePath, $"{job.ImageName}.jpg");

                using var image = Image.Load(path);
                image.Mutate(x => x.Resize(100, 100));
                image.SaveAsJpeg(outputPath);
            }
        }
    }

    public class Job
    {
        [JsonPropertyName("file_id")]
        public required string FileId { get; set; }

        [JsonPropertyName("path")]
        public required string Path { get; set; }

        [JsonPropertyName("image_name")]
        public required string ImageName { get; set; }
    }
}
