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
            ConnectionMultiplexer? connection = null;
            IDatabase? database = null;

            // Try to connect to Redis with retry logic
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    Console.WriteLine($"Attempting to connect to Redis (attempt {attempt}/3)...");
                    connection = ConnectionMultiplexer.Connect("localhost:6379");
                    database = connection.GetDatabase();
                    Console.WriteLine("✅ Connected to Redis successfully");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Redis connection attempt {attempt} failed: {ex.Message}");
                    if (attempt == 3)
                    {
                        Console.WriteLine("Failed to connect to Redis after 3 attempts. Please ensure Redis is running.");
                        return;
                    }
                    await Task.Delay(2000); // Wait 2 seconds before retry
                }
            }

            if (database is null) return;

            while (true)
            {
                try
                {
                    var jobData = database.ListLeftPop("new_job_queue");

                    if (!jobData.HasValue)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    // Deserialize the jobData
                    var job = JsonSerializer.Deserialize<Job>(jobData.ToString());
                    if (job == null)
                    {
                        Console.WriteLine("Failed to deserialize job data");
                        continue;
                    }

                    Console.WriteLine($"Processing job for file ID: {job.FileId}");

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
                    var outputPath = Path.Combine(vaultAppStoragePath, $"{fileId}-thumbnail.jpg");

                    try
                    {
                        using var image = Image.Load(path);
                        image.Mutate(x => x.Resize(100, 100));
                        image.SaveAsJpeg(outputPath);
                        Console.WriteLine($"✅ Thumbnail created for {job.FileId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error processing image {job.FileId}: {ex.Message}");
                    }
                }
                catch (RedisTimeoutException ex)
                {
                    Console.WriteLine($"❌ Redis timeout error: {ex.Message}");
                    Console.WriteLine("Waiting 5 seconds before retrying...");
                    await Task.Delay(5000);
                }
                catch (RedisConnectionException ex)
                {
                    Console.WriteLine($"❌ Redis connection error: {ex.Message}");
                    Console.WriteLine("Attempting to reconnect...");

                    // Try to reconnect
                    try
                    {
                        connection?.Dispose();
                        connection = ConnectionMultiplexer.Connect("localhost:6379");
                        database = connection.GetDatabase();
                        Console.WriteLine("✅ Reconnected to Redis");
                    }
                    catch
                    {
                        Console.WriteLine("❌ Failed to reconnect. Waiting 10 seconds...");
                        await Task.Delay(10000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
                    await Task.Delay(2000);
                }
            }
        }
    }

    public class Job
    {
        [JsonPropertyName("file_id")]
        required public string FileId { get; set; }

        [JsonPropertyName("path")]
        required public string Path { get; set; }
    }
}
