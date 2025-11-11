using System;
using System.Drawing;

namespace Common.Models
{
    public sealed class ScreenControlModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public ScreenControlSettings Settings { get; set; } = new ScreenControlSettings();
    }

    public sealed class ScreenControlSettings
    {
        public int Quality { get; set; } = 80; // 1-100
        public int FrameRate { get; set; } = 30; // FPS
        public int Resolution { get; set; } = 1; // 1=Full, 2=Half, 4=Quarter
        public bool EnableMouse { get; set; } = true;
        public bool EnableKeyboard { get; set; } = true;
        public bool EnableCompression { get; set; } = true;
        public CaptureMethod Capture { get; set; } = CaptureMethod.Gdi;
        public EncodeFormat Encode { get; set; } = EncodeFormat.Jpeg;
        public int BitrateKbps { get; set; } = 2500; // for H.264 ABR
    }

    public sealed class ScreenFrame
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public int Width { get; set; }
        public int Height { get; set; }
        public long FrameNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public int Quality { get; set; }
        public bool IsCompressed { get; set; }
        public EncodeFormat Format { get; set; } = EncodeFormat.Jpeg; // payload format
    }

    public sealed class MouseEvent
    {
        public int X { get; set; }
        public int Y { get; set; }
        public MouseButton Button { get; set; }
        public MouseAction Action { get; set; }
        public int ScrollDelta { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public sealed class KeyboardEvent
    {
        public int KeyCode { get; set; }
        public bool IsPressed { get; set; }
        public bool IsCtrl { get; set; }
        public bool IsAlt { get; set; }
        public bool IsShift { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum MouseButton
    {
        Left = 1,
        Right = 2,
        Middle = 4,
        XButton1 = 8,
        XButton2 = 16
    }

    public enum MouseAction
    {
        Move = 0,
        Down = 1,
        Up = 2,
        Click = 3,
        DoubleClick = 4,
        Scroll = 5
    }

    public sealed class ScreenControlOperationResult
    {
        public ScreenControlOperationType OperationType { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ClientId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum ScreenControlOperationType
    {
        Start = 1,
        Stop = 2,
        MouseEvent = 3,
        KeyboardEvent = 4,
        FrameCapture = 5,
        SettingsUpdate = 6
    }

    public enum CaptureMethod
    {
        Gdi = 0,
        Dxgi = 1
    }

    public enum EncodeFormat
    {
        Jpeg = 1,
        H264 = 2
    }
}
