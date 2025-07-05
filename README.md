# VaultApp - CLI File Storage System

A command-line interface application for securely storing and managing files in a local vault.

## Features

- ✅ **Upload Files** - Store files securely in the vault
- ✅ **List Files** - View all stored files with metadata
- ✅ **Read Files** - Access and read stored files
- ✅ **Delete Files** - Remove files from the vault
- ✅ **Duplicate Prevention** - Prevents uploading files with the same name
- ✅ **Security** - Blocks executable (.exe) files for safety
- ✅ **File Metadata** - Tracks file size, upload time, and unique IDs

## Prerequisites

- .NET 9.0 or later
- Windows, macOS, or Linux

## Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd workingWithCLI
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run --project vaultApp
   ```

## Usage

### Upload a File
```bash
dotnet run --project vaultApp upload <file-path >
```
**Example:**
```bash
dotnet run --project vaultApp upload "C:\Documents\report.pdf"
```

### List All Files
```bash
dotnet run --project vaultApp list
```

### Read a File
```bash
dotnet run --project vaultApp read <file-id >
```

### Delete a File
```bash
dotnet run --project vaultApp delete <file-id >
```

## File Structure

```
vaultApp/
├── Program.cs              # Main entry point
├── cli/
│   └── CommandRouter.cs    # Routes commands to handlers
├── Commands/
│   ├── Upload.cs          # Upload command handler
│   ├── List.cs            # List command handler
│   ├── Read.cs            # Read command handler
│   └── Delete.cs          # Delete command handler
├── Services/
│   └── FileService.cs     # Core file operations
└── Storage/
    ├── metadata.json      # File metadata storage
    └── uploads/           # Uploaded files directory
```

## Security Features

- **Executable Protection**: `.exe` files are blocked for security
- **File Validation**: Checks file existence before upload
- **Duplicate Prevention**: Prevents uploading files with identical names
- **Isolated Storage**: Files are stored in a dedicated `Storage` directory

## Error Handling

- ✅ Invalid file paths
- ✅ Missing files
- ✅ Duplicate file names
- ✅ Restricted file types
- ✅ Invalid file IDs

## Technical Details

- **Language**: C# 9.0+
- **Framework**: .NET 9.0
- **Data Storage**: JSON file-based metadata
- **File Storage**: Local file system

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License.

## Troubleshooting

### Common Issues

**"File not found" error**
- Ensure the file path is correct and the file exists
- Use absolute paths for better reliability

**"File already exists" error**
- The vault already contains a file with the same name
- Use the `list` command to see existing files

