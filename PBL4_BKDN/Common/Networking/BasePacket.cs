using System;
using Common.Enums;

namespace Common.Networking
{
    public class BasePacket
    {
        public PacketType PacketType { get; set; }
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? ClientId { get; set; }
        public string? ClientName { get; set; }
    }
}


