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
    public sealed class ShutdownActionHandler
    {
        private readonly ShutdownActionService _service;
        private readonly Networking.ClientConnection _connection;
        public ShutdownActionHandler(ShutdownActionService service, Networking.ClientConnection connection)
        {
            _service = service;
            _connection = connection;
        }
        public async Task HandleAsync(ShutdownActionRequest request)
        {
            try
            {
                var model = await _service.ExecuteCommandAsync(request);
                var response = new ShutdownActionResponse
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
                var errorResponse = new ShutdownActionResponse
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
