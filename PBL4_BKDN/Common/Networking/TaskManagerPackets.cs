using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Common.Models;

namespace Common.Networking
{
	public sealed class TaskManagerRequest : BasePacket
	{
		public TaskManagerRequest()
		{
			PacketType = PacketType.TaskManagerRequest;
		}

		public TaskManagerOperationType OperationType { get; set; }
		public int? TargetPID { get; set; } // For operations like KillProcess
		public string? SearchKeyword { get; set; }
	}

	public sealed class TaskManagerResponse : BasePacket
	{
		public TaskManagerResponse()
		{
			PacketType = PacketType.TaskManagerResponse;
		}
		public ResponseStatusType Status { get; set; } = ResponseStatusType.Unknown;
		public List<TaskManagerModel>? Processes { get; set; }
		public bool? OperationSuccess { get; set; } // Cho kill process
		public string? ErrorMessage { get; set; }
		public TaskManagerModel? Payload { get; set; }
		public int? PayloadSizeBytes { get; set; }
	}
}
