using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Services;
using Common.Enums;
using Common.Models;
using Common.Networking;
using Common.Utils;

namespace Client.Handlers
{
	public sealed class TaskManagerHandler
	{
		private readonly TaskManagerService _service; 
		private readonly Networking.ClientConnection _connection; 

		public TaskManagerHandler(TaskManagerService service, Networking.ClientConnection connection)
		{
			_service = service;
			_connection = connection;
		}
		
		public async Task HandleAsync(TaskManagerRequest request)
		{
			try
			{
				if (request.OperationType == TaskManagerOperationType.GetProcessList)
				{
					// Gửi phản hồi nhanh trước
					var fast = await _service.GetProcessListFastAsync();
					var fastResp = new TaskManagerResponse
					{
						RequestId = request.RequestId,
						Status = fast.OperationResult?.Success == true ? ResponseStatusType.Ok : ResponseStatusType.Error,
						ClientId = request.ClientId ?? fast.ClientId,
						ClientName = fast.ClientName,
						Payload = fast,
						ErrorMessage = fast.OperationResult?.ErrorMessage
					};
					var fastJson = JsonHelper.Serialize(fastResp);
					fastResp.PayloadSizeBytes = fastJson.Length;
					await _connection.SendAsync(fastJson);

					// Tiếp theo gửi phản hồi đầy đủ
					var full = await _service.ProcessRequestAsync(request);
					var fullResp = new TaskManagerResponse
					{
						RequestId = request.RequestId,
						Status = full.OperationResult?.Success == true ? ResponseStatusType.Ok : ResponseStatusType.Error,
						ClientId = request.ClientId ?? full.ClientId,
						ClientName = full.ClientName,
						Payload = full,
						ErrorMessage = full.OperationResult?.ErrorMessage
					};
					var fullJson = JsonHelper.Serialize(fullResp);
					fullResp.PayloadSizeBytes = fullJson.Length;
					await _connection.SendAsync(fullJson);
				}
				else
				{
					var model = await _service.ProcessRequestAsync(request);
					var response = new TaskManagerResponse
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
			}
			catch (Exception e)
			{
				var errorResponse = new TaskManagerResponse
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
