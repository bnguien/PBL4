using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Services;
using Common.Enums;
using Common.Networking;
using Common.Utils;

namespace Client.Handlers
{
    public sealed class RemoteShellHandler
    {
        private readonly RemoteShellService _service;
        private readonly Networking.ClientConnection _connection;

        public RemoteShellHandler(RemoteShellService service, Networking.ClientConnection connection)
        {
            _service = service;
            _connection = connection;
        }
        public async Task HandleAsync(RemoteShellRequest request)
        {
            try
            {
                var model = await _service.ExecuteCommandAsync(request);
                var response = new RemoteShellResponse
                {
                    RequestId = request.RequestId,
                    Status = model.CommandResult?.ErrorMessage == null ? ResponseStatusType.Ok : ResponseStatusType.Error,
                    ClientId = request.ClientId ?? model.ClientId,
                    ClientName = model.ClientName,
                    Payload = model,
                    ErrorMessage = model.CommandResult?.ErrorMessage
                };
                var json = JsonHelper.Serialize(response);
                response.PayloadSizeBytes = json.Length;
                await _connection.SendAsync(json);
            }
            catch (Exception e)
            {
                var errorResponse = new RemoteShellResponse
                {
                    RequestId = request.RequestId,
                    Status = ResponseStatusType.Error,
                    ClientId = request.ClientId,
                    ClientName = Environment.MachineName,
                    ErrorMessage = e.Message,
                    Payload = null
                };

                var json = JsonHelper.Serialize(errorResponse);
                await _connection.SendAsync(json);
            }
        }
    }
}
