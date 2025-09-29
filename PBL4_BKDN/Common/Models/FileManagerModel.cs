using System;
using System.Collections.Generic;

namespace Common.Models
{
    public sealed class FileManagerModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        public List<DriveInfoModel> Drives { get; set; } = new List<DriveInfoModel>();
        public List<FileSystemItemModel> Items { get; set; } = new List<FileSystemItemModel>();
        public string CurrentPath { get; set; } = string.Empty;
        public FileManagerOperationResult? OperationResult { get; set; }
    }

    public sealed class DriveInfoModel
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
        public bool IsReady { get; set; }
    }

    public sealed class FileSystemItemModel
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "File" or "Directory"
        public long SizeBytes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime AccessedDate { get; set; }
        public string? Extension { get; set; }
        public bool IsHidden { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public sealed class FileManagerOperationResult
    {
        public FileManagerOperationType OperationType { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TargetPath { get; set; }
        public string? NewPath { get; set; } // For rename/move operations
        public byte[]? FileData { get; set; } // For download operations
        public string? FileName { get; set; } // For download operations
    }

    public enum FileManagerOperationType
    {
        ListDrives = 1,
        ListDirectory = 2,
        Delete = 3,
        Rename = 4,
        Search = 5,
        Download = 6,
        CreateDirectory = 7,
        Upload = 8
    }
}
