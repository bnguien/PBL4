using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Common.Networking;
using Common.Models;

namespace Server.Handlers
{
	public sealed class TaskManagerHandler
	{
		private readonly ConcurrentDictionary<string, TaskManagerResponse> _lastResponses = new();
		private readonly ConcurrentDictionary<string, List<TaskManagerResponse>> _responseHistory = new();

		public event EventHandler<TaskManagerResponse>? OnResponseReceived;
		public event EventHandler<TaskManagerOperationResult>? OnOperationCompleted;

		public void SaveLastResponse(string clientId, TaskManagerResponse response)
		{
			_lastResponses[clientId] = response;

			_responseHistory.AddOrUpdate(clientId,
				new List<TaskManagerResponse> { response },
				(key, existing) =>
				{
					existing.Add(response);
					if (existing.Count > 50) existing.RemoveAt(0);
					return existing;
				});
			OnResponseReceived?.Invoke(this, response);
		}

		public bool TryGetLastResponse(string clientId, out TaskManagerResponse? response)
		{
			return _lastResponses.TryGetValue(clientId, out response);
		}

		public List<TaskManagerResponse> GetResponseHistory(string clientId)
		{
			return _responseHistory.TryGetValue(clientId, out var history)
				? new List<TaskManagerResponse>(history)
				: new List<TaskManagerResponse>();
		}

		public bool TryGetResponseByRequestId(string clientId, string requestId, out TaskManagerResponse? response)
		{
			response = null;
			if (_responseHistory.TryGetValue(clientId, out var history))
			{
				response = history.Find(r => r.RequestId == requestId);
				return response != null;
			}
			return false;
		}

		// ================ CÁC METHOD MỚI - SỬA CHO TaskManagerModel ================

		// Chuẩn hóa lấy danh sách ProcessInfo từ phản hồi, ưu tiên Payload rồi fallback Processes
		private static List<ProcessInfo> GetProcessesFromResponse(TaskManagerResponse response)
		{
			if (response == null) return new List<ProcessInfo>();
			if (response.Payload?.Processes != null)
			{
				return response.Payload.Processes;
			}
			if (response.Processes != null)
			{
				return response.Processes.SelectMany(tm => tm.Processes).ToList();
			}
			return new List<ProcessInfo>();
		}

		public List<ProcessInfo> SearchProcesses(string clientId, string searchTerm)
		{
			if (TryGetLastResponse(clientId, out var response) && response != null)
			{
				var allProcesses = GetProcessesFromResponse(response);

				return allProcesses
					.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
							   p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
							   p.PID.ToString().Contains(searchTerm) ||
							   p.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
					.ToList();
			}
			return new List<ProcessInfo>();
		}

		public ProcessStatistics CalculateProcessStatistics(string clientId)
		{
			var stats = new ProcessStatistics();

			if (!TryGetLastResponse(clientId, out var response) || response == null)
				return stats;

			var allProcesses = GetProcessesFromResponse(response);

			stats.TotalProcesses = allProcesses.Count;
			stats.RunningProcesses = allProcesses.Count(p => p.Status == ProcessStatus.Running);
			stats.SuspendedProcesses = allProcesses.Count(p => p.Status == ProcessStatus.Suspended);
			stats.TerminatedProcesses = allProcesses.Count(p => p.Status == ProcessStatus.Terminated);
			stats.TotalMemoryUsage = allProcesses.Sum(p => p.WorkingSet);
			stats.AverageCPUUsage = allProcesses.Any() ? allProcesses.Average(p => p.CPU) : 0;
			stats.X86Processes = allProcesses.Count(p => p.Platform == Platform.X86);
			stats.X64Processes = allProcesses.Count(p => p.Platform == Platform.X64);
			stats.ArmProcesses = allProcesses.Count(p => p.Platform == Platform.Arm);
			stats.Arm64Processes = allProcesses.Count(p => p.Platform == Platform.Arm64);
			stats.UnknownPlatformProcesses = allProcesses.Count(p => p.Platform == Platform.Unknown);

			return stats;
		}

		public List<ProcessAlert> DetectSuspiciousProcesses(string clientId)
		{
			var alerts = new List<ProcessAlert>();

			if (!TryGetLastResponse(clientId, out var response) || response == null)
				return alerts;

			var allProcesses = GetProcessesFromResponse(response);

			foreach (var process in allProcesses)
			{
				if (process.CPU > 50.0)
				{
					alerts.Add(new ProcessAlert
					{
						ProcessName = process.Name,
						PID = process.PID,
						Type = AlertType.HighCPU,
						Description = $"High CPU usage: {process.CPU}%"
					});
				}

				if (process.WorkingSet > 500 * 1024 * 1024)
				{
					alerts.Add(new ProcessAlert
					{
						ProcessName = process.Name,
						PID = process.PID,
						Type = AlertType.HighMemory,
						Description = $"High memory usage: {process.WorkingSet / (1024 * 1024)}MB"
					});
				}
			}

			return alerts;
		}

		public void NotifyOperationCompleted(TaskManagerOperationResult operation)
		{
			OnOperationCompleted?.Invoke(this, operation);
		}

		// Lấy tất cả processes từ response
		public List<ProcessInfo> GetAllProcesses(string clientId)
		{
			if (TryGetLastResponse(clientId, out var response) && response != null)
			{
				return GetProcessesFromResponse(response);
			}
			return new List<ProcessInfo>();
		}
	}

	// ================ SUPPORTING CLASSES ================

	public class ProcessStatistics
	{
		public int TotalProcesses { get; set; }
		public int RunningProcesses { get; set; }
		public int SuspendedProcesses { get; set; }
		public int TerminatedProcesses { get; set; }
		public long TotalMemoryUsage { get; set; }
		public double AverageCPUUsage { get; set; }
		public int X86Processes { get; set; }
		public int X64Processes { get; set; }
		public int ArmProcesses { get; set; }
		public int Arm64Processes { get; set; }
		public int UnknownPlatformProcesses { get; set; }
	}

	public class ProcessAlert
	{
		public string ProcessName { get; set; } = string.Empty;
		public int PID { get; set; }
		public AlertType Type { get; set; }
		public string Description { get; set; } = string.Empty;
	}

	public enum AlertType
	{
		HighCPU,
		HighMemory,
		UnknownPlatform
	}
}