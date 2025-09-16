using System.Threading.Tasks;
using Client.Services;
using Common.Enums;
using Common.Networking;
using Common.Utils;

namespace Client.Handlers
{
    public sealed class SystemInfoHandler
    {
        private readonly SystemInfoService _service;
        private readonly Networking.ClientConnection _connection;

        public SystemInfoHandler(SystemInfoService service, Networking.ClientConnection connection)
        {
            _service = service;
            _connection = connection;
        }

        public async Task HandleAsync(SystemInfoRequest request)
        {
            var model = await _service.GetSystemInfoAsync(request.IncludeHardware, request.IncludeNetwork, request.IncludeSoftware);
            var response = new SystemInfoResponse
            {
                RequestId = request.RequestId,
                Status = ResponseStatusType.Ok,
                ClientId = request.ClientId ?? model.ClientId,
                ClientName = model.ClientName,
                Payload = model
            };
            var json = JsonHelper.Serialize(response);
            response.PayloadSizeBytes = json.Length;
            await _connection.SendAsync(json);
        }
    }
}


