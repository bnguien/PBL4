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

        //-------------------------------------//

    }
}


