using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Common.Models;

namespace Common.Networking
{
    public sealed class MessageBoxRequest : BasePacket
    {
        public MessageBoxRequest()
        {
            PacketType = PacketType.MessageBoxRequest;
        }
        public MessageBoxModel Message { get; set; } = new MessageBoxModel();

    }
    public sealed class MessageBoxResponse : BasePacket
    {
        public MessageBoxResponse()
        {
            PacketType = PacketType.MessageBoxResponse;
        }
        
        public ResponseStatusType Status { get; set; } = ResponseStatusType.Unknown;
        public MessageBoxModel? Payload { get; set; }
        public string? Result { get; set; }
        public int? PayloadSizeBytes { get; set; }
        public string? ErrorMessage { get; set; }

        public string? ClientId { get; set; }
        public string? ClientName { get; set; }

    }
}
