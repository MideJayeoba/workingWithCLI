using StackExchange.Redis;

namespace vaultApp.Database
{
    public static class Redis
    {
        private static ConnectionMultiplexer? _connection;
        private static IDatabase? _database;

        public static void Initialise()
        {
            try
            {
                _connection = ConnectionMultiplexer.Connect("localhost:6379");
                _database = _connection.GetDatabase();
            }
            catch (Exception)
            {
                Console.WriteLine("❌ Redis failed");
            }
        }

        // Store user session when login is successful
        public static void CreateSession(string userId)
        {
            if (_database is null) return;

            try
            {
                _database.StringSet("current_user", userId, TimeSpan.FromHours(12));
                Console.WriteLine($"✅ Session created for user: {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Session creation failed: {ex.Message}");
            }
        }

        // To store job queue in JSON format
        public static void CreateJobQueue(string jobData)
        {
            if (_database is null) return;

            try
            {
                _database.ListRightPush("new_job_queue", jobData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Job queue creation failed: {ex.Message}");
            }
        }

        // Check if user is logged in
        public static bool IsLoggedIn()
        {
            if (_database is null) return false;

            try
            {
                return _database.KeyExists("current_user");
            }
            catch
            {
                return false;
            }
        }

        // Get the current user ID
        public static string? GetCurrentUserId()
        {
            if (_database is null) return null;

            try
            {
                var userId = _database.StringGet("current_user");
                return userId.HasValue ? userId.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        public static string? GetJobQueue()
        {
            if (_database is null)
                
                return null;

            try
            {
                var job = _database.StringGet("job_queue");
                return job.HasValue ? job.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        // Clear the job queue
        public static void ClearJobQueue()
        {
            if (_database is null) return;

            try
            {
                _database.KeyDelete("new_job_queue");
                Console.WriteLine("✅ Job queue cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Job queue clear failed: {ex.Message}");
            }
        }

        // Remove session when user logs out
        public static void EndSession()
        {
            if (_database is null) return;

            try
            {
                _database.KeyDelete("current_user");
                Console.WriteLine("✅ Session ended");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Session end failed: {ex.Message}");
            }
        }
    }
}
