using System;
using System.Threading.Tasks;
using Client.Services;
using Common.Enums;
using Common.Networking;
using Common.Utils;

namespace Client.Handlers
{
    public sealed class FileManagerHandler
    {
        private readonly FileManagerService _service;
        private readonly Networking.ClientConnection _connection;

        public FileManagerHandler(FileManagerService service, Networking.ClientConnection connection)
        {
            _service = service;
            _connection = connection;
        }

        public async Task HandleAsync(FileManagerRequest request)
        {
            try
            {
                var model = await _service.ProcessRequestAsync(request);
                var response = new FileManagerResponse
                {
                    RequestId = request.RequestId,
                    Status = model.OperationResult?.Success == true ? ResponseStatusType.Ok : ResponseStatusType.Error,
                    ClientId = request.ClientId ?? model.ClientId,
                    ClientName = model.ClientName,
                    Payload = model,
                    ErrorMessage = model.OperationResult?.ErrorMessage
                };
                var json = JsonHelper.Serialize(response);
                response.PayloadSizeBytes = json.Length;
                await _connection.SendAsync(json);
            }
            catch (Exception e)
            {
                var errorResponse = new FileManagerResponse
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
