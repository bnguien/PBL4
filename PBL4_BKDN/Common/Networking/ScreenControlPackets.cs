using System;
using Common.Enums;
using Common.Models;

namespace Common.Networking
{
    public sealed class ScreenControlStart : BasePacket
    {
        public ScreenControlStart()
        {
            PacketType = PacketType.ScreenControlStart;
            RequestId = Guid.NewGuid().ToString();
        }

        public ScreenControlSettings Settings { get; set; } = new ScreenControlSettings();
        public int UdpPort { get; set; } = 0; // 0 = TCP mode only
        public string SessionId { get; set; } = string.Empty; // Guid string for UDP association
    }

    public sealed class ScreenControlStop : BasePacket
    {
        public ScreenControlStop()
        {
            PacketType = PacketType.ScreenControlStop;
            RequestId = Guid.NewGuid().ToString();
        }
    }

    public sealed class ScreenControlFrame : BasePacket
    {
        public ScreenControlFrame()
        {
            PacketType = PacketType.ScreenControlFrame;
        }

        public ScreenFrame Frame { get; set; } = new ScreenFrame();
    }

    public sealed class ScreenControlMouseEvent : BasePacket
    {
        public ScreenControlMouseEvent()
        {
            PacketType = PacketType.ScreenControlMouseEvent;
            RequestId = Guid.NewGuid().ToString();
        }

        public MouseEvent MouseEvent { get; set; } = new MouseEvent();
    }

    public sealed class ScreenControlKeyboardEvent : BasePacket
    {
        public ScreenControlKeyboardEvent()
        {
            PacketType = PacketType.ScreenControlKeyboardEvent;
            RequestId = Guid.NewGuid().ToString();
        }

        public KeyboardEvent KeyboardEvent { get; set; } = new KeyboardEvent();
    }

    public sealed class ScreenControlResponse : BasePacket
    {
        public ScreenControlResponse()
        {
            PacketType = PacketType.ScreenControlResponse;
        }

        public ResponseStatusType Status { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public ScreenControlOperationResult? OperationResult { get; set; }
    }
}