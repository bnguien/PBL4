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

        public PacketHandler(Action<SystemInfoResponse> onSystemInfoResponse, Action<RemoteShellResponse> onRemoteShellResponse, Action<FileManagerResponse> onFileManagerResponse)
        {
            _onSystemInfoResponse = onSystemInfoResponse;
            _onRemoteShellResponse = onRemoteShellResponse;
            _onFileManagerResponse = onFileManagerResponse;
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


