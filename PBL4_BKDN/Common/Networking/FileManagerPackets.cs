using Common.Enums;
using Common.Models;

namespace Common.Networking
{
    public sealed class FileManagerRequest : BasePacket
    {
        public FileManagerRequest()
        {
            PacketType = PacketType.FileManagerRequest;
        }

        public FileManagerOperationType OperationType { get; set; }
        public string? TargetPath { get; set; }
        public string? NewName { get; set; } // For rename operations
        public string? SearchPattern { get; set; } // For search operations
        public string? SearchPath { get; set; } // For search operations
        public bool IncludeHidden { get; set; } = false;
        public bool IncludeSystem { get; set; } = false;
    }

    public sealed class FileManagerResponse : BasePacket
    {
        public FileManagerResponse()
        {
            PacketType = PacketType.FileManagerResponse;
        }

        public ResponseStatusType Status { get; set; } = ResponseStatusType.Unknown;
        public FileManagerModel? Payload { get; set; }
        public int? PayloadSizeBytes { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
