using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Common.Models;

namespace Common.Networking
{
    public sealed class ShutdownActionRequest : BasePacket
    {
        public ShutdownActionRequest()
        {
            PacketType = PacketType.ShutdownActionRequest;
        }
        public ShutdownAction Action { get; set; }

        public string Command { get; set; } = string.Empty;
        public string? WorkingDirectory { get; set; }

    }
    public sealed class ShutdownActionResponse : BasePacket
    {
        public ShutdownActionResponse()
        {
            PacketType = PacketType.ShutdownActionResponse;
        }
        public ResponseStatusType Status { get; set; } = ResponseStatusType.Unknown;
        public ShutdownActionModel? Payload { get; set; }
        public string? Result { get; set; }
        public int? PayloadSizeBytes { get; set; }
        public string? ErrorMessage { get; set; }

        public string? ClientId { get; set; }
        public string? ClientName { get; set; }
    }
}
