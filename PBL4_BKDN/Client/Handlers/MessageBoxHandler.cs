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
    public sealed class MessageBoxHandler
    {
        private readonly MessageBoxService _service;
        private readonly Networking.ClientConnection _connection;

        public MessageBoxHandler(MessageBoxService service, Networking.ClientConnection connection)
        {
            _service = service;
            _connection = connection;
        }

        public async Task HandleAsync(MessageBoxRequest request)
        {
            try
            {
                var model = await _service.ShowMessageBoxAsync(request);

                var response = new MessageBoxResponse
                {
                    RequestId = request.RequestId,
                    Status = string.IsNullOrEmpty(model.Result)
                                ? ResponseStatusType.Error
                                : ResponseStatusType.Ok,
                    ClientId = model.ClientId,
                    ClientName = model.ClientName,
                    Payload = model, 
                    Result = model.Result
                };

                var json = JsonHelper.Serialize(response);
                response.PayloadSizeBytes = json.Length;
                await _connection.SendAsync(json);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageBoxResponse
                {
                    RequestId = request.RequestId,
                    Status = ResponseStatusType.Error,
                    ClientId = request.ClientId,
                    ClientName = Environment.MachineName,
                    ErrorMessage = ex.Message
                };

                var json = JsonHelper.Serialize(errorResponse);
                await _connection.SendAsync(json);
            }
        }
    }

}
