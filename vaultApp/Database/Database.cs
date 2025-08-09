using Microsoft.Data.Sqlite;
using System.IO;

namespace vaultApp.Database;

public static class Database
{
    // Use shared storage path
    private static readonly string DatabasePath = Path.GetFullPath(Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "SharedStorage", "vault.db"));

    private static readonly string ConnectionString = $"Data Source={DatabasePath}";

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    public static void Initialize()
    {
        // Ensure SharedStorage directory exists
        var sharedStorageDir = Path.GetDirectoryName(DatabasePath);
        if (!Directory.Exists(sharedStorageDir))
        {
            Directory.CreateDirectory(sharedStorageDir!);
        }

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        // Create Users table
        var createUsersTableCommand = connection.CreateCommand();
        createUsersTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Username TEXT UNIQUE,
                Email TEXT UNIQUE NOT NULL,
                HashedPassword TEXT NOT NULL,
                CreatedAt TEXT NOT NULL              
            )";
        createUsersTableCommand.ExecuteNonQuery();

        // Create Files table for file metadata
        var createFilesTableCommand = connection.CreateCommand();
        createFilesTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS Files (
                UserId TEXT NOT NULL,
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Path TEXT NOT NULL,
                Type TEXT NOT NULL DEFAULT 'file',
                Visibility TEXT NOT NULL DEFAULT 'private',
                Size INTEGER NOT NULL,
                UploadTime TEXT NOT NULL,
                ParentId TEXT,
                FOREIGN KEY (UserId) REFERENCES Users (Id),
                FOREIGN KEY (ParentId) REFERENCES Files (Id)
            )";
        createFilesTableCommand.ExecuteNonQuery();
    }
}