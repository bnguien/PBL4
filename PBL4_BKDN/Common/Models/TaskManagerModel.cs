using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
	public sealed class TaskManagerModel
	{
		public string ClientId { get; set; } = string.Empty;
		public string ClientName { get; set; } = string.Empty;
		public DateTime Timestamp { get; set; }

		public List<ProcessInfo> Processes { get; set; } = new List<ProcessInfo>();

		public TaskManagerOperationResult? OperationResult { get; set; }
	}

	public sealed class ProcessInfo
	{
		public string Name { get; set; } = string.Empty;
		public int PID { get; set; }
		public ProcessStatus Status { get; set; } = ProcessStatus.Running;
		public string UserName { get; set; } = string.Empty;
		public double CPU { get; set; }
		public long WorkingSet { get; set; }
		public Platform Platform { get; set; } = Platform.Unknown;
		public UACVirtualization UACVirtualization { get; set; } = UACVirtualization.NotAllowed;
		public string Description { get; set; } = string.Empty; 
	}
	public sealed class TaskManagerOperationResult
	{
		public TaskManagerOperationType OperationType { get; set; }
		public bool Success { get; set; }
		public string? ErrorMessage { get; set; }
		public int? TargetPID { get; set; } // The PID of the process that was the target of the operation
		public string? SearchKeyword { get; set; }
		public string? ProcessName { get; set; }
		public int? ProcessesCount { get; set; }
	}

	public enum ProcessStatus
	{
		Running,
		Suspended,
		Terminated,
		Unknown
	}

	public enum UACVirtualization
	{
		Disabled, 
		Enabled, 
		NotAllowed
	}

	public enum Platform
	{
		X86,//32 bit
		X64,//64 bit
		Arm,
		Arm64,
		Unknown
	}

	public enum TaskManagerOperationType
	{
		GetProcessList = 1,
		SearchProcess = 2,
		KillProcess = 3,
		RefreshProcessList = 4,
	}
}
