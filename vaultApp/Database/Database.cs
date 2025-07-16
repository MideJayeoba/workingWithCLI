using Microsoft.Data.Sqlite;


namespace vaultApp.Database;

public static class Database
{
    private const string ConnectionString = "Data Source=Database/vault.db";

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    public static void Initialize()
    {
        // Create Database directory if it doesn't exist
        if (!Directory.Exists("Database"))
        {
            Directory.CreateDirectory("Database");
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
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Path TEXT NOT NULL,
                Size INTEGER NOT NULL,
                UploadTime TEXT NOT NULL
            )";
        createFilesTableCommand.ExecuteNonQuery();
    }
}