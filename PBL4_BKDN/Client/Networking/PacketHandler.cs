using System;
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

        public PacketHandler(Action<SystemInfoRequest> onSystemInfoRequest, Action<RemoteShellRequest> onRemoteShellRequest, Action<FileManagerRequest> onFileManagerRequest)
        {
            _onSystemInfoRequest = onSystemInfoRequest;
            _onRemoteShellRequest = onRemoteShellRequest;
            _onFileManagerRequest = onFileManagerRequest;
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
                        if(shellReq != null)
                        {
                            _onRemoteShellRequest(shellReq);
                        }
                        break;

                    case PacketType.FileManagerRequest:
                        var fileReq = JsonHelper.Deserialize<FileManagerRequest>(json);
                        if(fileReq != null)
                        {
                            _onFileManagerRequest(fileReq);
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


