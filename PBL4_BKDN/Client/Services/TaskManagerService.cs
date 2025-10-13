using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Common.Networking;

namespace Client.Services
{
	public sealed class TaskManagerService : IDisposable
	{
		private static readonly Dictionary<int, (PerformanceCounter counter, DateTime lastUpdate)> _cpuCounters = new();
		private static readonly object _cacheLock = new object();
		private DateTime _lastCacheCleanup = DateTime.Now;

		public async Task<TaskManagerModel> ProcessRequestAsync(TaskManagerRequest request)
		{
			var model = new TaskManagerModel
			{
				ClientId = Guid.NewGuid().ToString(),
				ClientName = Environment.MachineName,
				Timestamp = DateTime.UtcNow
			};

			try
			{
				switch (request.OperationType)
				{
					case TaskManagerOperationType.GetProcessList:
						model = await GetProcessListAsync(useCache: true);
						break;
					case TaskManagerOperationType.SearchProcess:
						model = await SearchProcessAsync(request.SearchKeyword ?? "");
						break;
					case TaskManagerOperationType.KillProcess:
						model = await KillProcessAsync(request.TargetPID ?? -1);
						break;
					case TaskManagerOperationType.RefreshProcessList:
						model = await RefreshProcessListAsync();
						break;
					default:
						throw new NotSupportedException($"Operation type {request.OperationType} is not supported");
				}
			}
			catch (Exception ex)
			{
				model.OperationResult = new TaskManagerOperationResult
				{
					OperationType = request.OperationType,
					Success = false,
					ErrorMessage = ex.Message,
					TargetPID = request.TargetPID
				};
			}

			return model;
		}

		private async Task<TaskManagerModel> RefreshProcessListAsync()
		{
			return await Task.Run(() =>
			{
				try
				{
					ClearCpuCache();
					var freshData = GetProcessListAsync(useCache: false).Result;

					freshData.OperationResult.OperationType = TaskManagerOperationType.RefreshProcessList;

					return freshData;
				}
				catch (Exception ex)
				{
					return new TaskManagerModel
					{
						ClientId = Guid.NewGuid().ToString(),
						ClientName = Environment.MachineName,
						Timestamp = DateTime.UtcNow,
						OperationResult = new TaskManagerOperationResult
						{
							OperationType = TaskManagerOperationType.RefreshProcessList,
							Success = false,
							ErrorMessage = $"Refresh failed: {ex.Message}"
						}
					};
				}
			});
		}

		private async Task<TaskManagerModel> GetProcessListAsync(bool useCache = true)
		{
			return await Task.Run(() =>
			{
				var model = new TaskManagerModel
				{
					ClientId = Guid.NewGuid().ToString(),
					ClientName = Environment.MachineName,
					Timestamp = DateTime.UtcNow,
					Processes = new List<ProcessInfo>()
				};

				try
				{
					AutoCleanupCache();

					var processes = new List<ProcessInfo>();
					var processList = Process.GetProcesses();

					foreach (var process in processList)
					{
						try
						{
							var processInfo = new ProcessInfo
							{
								Name = process.ProcessName,
								PID = process.Id,
								Status = GetProcessStatus(process),
								UserName = GetProcessOwner(process),
								CPU = useCache ? CalculateCpuUsage(process) : CalculateFreshCpuUsage(process),
								WorkingSet = process.WorkingSet64,
								Platform = GetProcessPlatform(process),
								UACVirtualization = GetUACVirtualization(process),
								Description = GetProcessDescription(process)
							};
							processes.Add(processInfo);
						}
						catch
						{
							continue; // Skip inaccessible processes
						}
					}

					// 📦 PACKAGE RESULTS
					model.Processes = processes;
					model.OperationResult = new TaskManagerOperationResult
					{
						OperationType = TaskManagerOperationType.GetProcessList,
						Success = true,
						ProcessesCount = processes.Count,
					};

					return model;
				}
				catch (Exception ex)
				{
					model.OperationResult = new TaskManagerOperationResult
					{
						OperationType = TaskManagerOperationType.GetProcessList,
						Success = false,
						ErrorMessage = ex.Message
					};
					return model;
				}
			});
		}

		// FAST-FIRST: trả về danh sách nhẹ, không WMI/PerformanceCounter để UI hiển thị ngay
		public async Task<TaskManagerModel> GetProcessListFastAsync()
		{
			return await Task.Run(() =>
			{
				var model = new TaskManagerModel
				{
					ClientId = Guid.NewGuid().ToString(),
					ClientName = Environment.MachineName,
					Timestamp = DateTime.UtcNow,
					Processes = new List<ProcessInfo>()
				};

				try
				{
					var processes = new List<ProcessInfo>();
					var processList = Process.GetProcesses();

					foreach (var process in processList)
					{
						try
						{
							var info = new ProcessInfo
							{
								Name = process.ProcessName,
								PID = process.Id,
								Status = GetProcessStatus(process),
								UserName = string.Empty, // defer
								CPU = 0.0, // defer
								WorkingSet = process.WorkingSet64,
								Platform = Platform.Unknown, // defer
								UACVirtualization = UACVirtualization.NotAllowed, // defer
								Description = string.Empty // defer
							};
							processes.Add(info);
						}
						catch { }
					}

					model.Processes = processes;
					model.OperationResult = new TaskManagerOperationResult
					{
						OperationType = TaskManagerOperationType.GetProcessList,
						Success = true,
						ProcessesCount = processes.Count
					};
				}
				catch (Exception ex)
				{
					model.OperationResult = new TaskManagerOperationResult
					{
						OperationType = TaskManagerOperationType.GetProcessList,
						Success = false,
						ErrorMessage = ex.Message
					};
				}

				return model;
			});
		}

		private void ClearCpuCache()
		{
			lock (_cacheLock)
			{
				try
				{
					foreach (var counterPair in _cpuCounters.Values)
					{
						try
						{
							counterPair.counter?.Dispose();
						}
						catch { }
					}
					_cpuCounters.Clear();
					_lastCacheCleanup = DateTime.Now;
				}
				catch { }
			}
		}

		private void AutoCleanupCache()
		{
			lock (_cacheLock)
			{
				try
				{
					if ((DateTime.Now - _lastCacheCleanup).TotalMinutes >= 5)
					{
						var cutoffTime = DateTime.Now.AddMinutes(-10);
						var oldEntries = _cpuCounters
							.Where(x => x.Value.lastUpdate < cutoffTime)
							.Select(x => x.Key)
							.ToList();

						foreach (var pid in oldEntries)
						{
							try
							{
								_cpuCounters[pid].counter?.Dispose();
								_cpuCounters.Remove(pid);
							}
							catch { /* Ignore disposal errors */ }
						}

						_lastCacheCleanup = DateTime.Now;
					}
				}
				catch { /* Ignore cleanup errors */ }
			}
		}
		
		private double CalculateCpuUsage(Process process)
		{
			try
			{
				lock (_cacheLock)
				{
					if (!_cpuCounters.ContainsKey(process.Id) ||
						_cpuCounters[process.Id].lastUpdate < DateTime.Now.AddSeconds(-2))
					{
						var counter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true);
						_cpuCounters[process.Id] = (counter, DateTime.Now);

						counter.NextValue();
						return 0.0;
					}

					var value = _cpuCounters[process.Id].counter.NextValue();
					_cpuCounters[process.Id] = (_cpuCounters[process.Id].counter, DateTime.Now);

					return Math.Round(value / Environment.ProcessorCount, 2);
				}
			}
			catch
			{
				return 0.0;
			}
		}

		private double CalculateFreshCpuUsage(Process process)
		{
			try
			{
				var startTime = DateTime.UtcNow;
				var startCpuUsage = process.TotalProcessorTime;

				System.Threading.Thread.Sleep(50);

				var endTime = DateTime.UtcNow;
				var endCpuUsage = process.TotalProcessorTime;

				var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
				var totalMsPassed = (endTime - startTime).TotalMilliseconds;

				if (totalMsPassed > 0)
				{
					var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
					return Math.Round(cpuUsageTotal * 100, 2);
				}

				return 0.0;
			}
			catch
			{
				return 0.0;
			}
		}

		private async Task<TaskManagerModel> KillProcessAsync(int targetPid)
		{
			return await Task.Run(() =>
			{
				var model = new TaskManagerModel
				{
					ClientId = Guid.NewGuid().ToString(),
					ClientName = Environment.MachineName,
					Timestamp = DateTime.UtcNow
				};

				try
				{
					if (targetPid <= 0)
						throw new ArgumentException("Invalid Process ID");

					var process = Process.GetProcessById(targetPid);

					if (!CanKillProcess(process))
						throw new UnauthorizedAccessException($"Access denied to kill process {targetPid}");

					string processName = process.ProcessName;
					process.Kill();

					bool terminated = process.WaitForExit(5000);

					if (!terminated)
						throw new TimeoutException($"Process {targetPid} did not terminate in time");

					lock (_cacheLock)
					{
						if (_cpuCounters.ContainsKey(targetPid))
						{
							_cpuCounters[targetPid].counter?.Dispose();
							_cpuCounters.Remove(targetPid);
						}
					}

					model.OperationResult = new TaskManagerOperationResult
					{
						OperationType = TaskManagerOperationType.KillProcess,
						Success = true,
						TargetPID = targetPid,
						ProcessName = processName,
					};
				}
				catch (Exception ex)
				{
					model.OperationResult = new TaskManagerOperationResult
					{
						OperationType = TaskManagerOperationType.KillProcess,
						Success = false,
						ErrorMessage = ex.Message,
						TargetPID = targetPid
					};
				}

				return model;
			});
		}

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWow64Process2(IntPtr process, out ushort processMachine, out ushort nativeMachine);

		private bool Is64BitProcess(IntPtr handle)
		{
			try
			{
				if (Environment.Is64BitOperatingSystem)
				{
					ushort processMachine, nativeMachine;
					if (IsWow64Process2(handle, out processMachine, out nativeMachine))
					{
						// nativeMachine != 0 => 64-bit process
						return nativeMachine != 0;
					}
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		private bool CanKillProcess(Process process)
		{
			try
			{
				var criticalProcesses = new[] { "system", "svchost", "csrss", "wininit", "services", "lsass" };

				if (criticalProcesses.Contains(process.ProcessName.ToLower()))
					return false;

				if (GetProcessOwner(process).Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
					return false;

				if (process.Id == Process.GetCurrentProcess().Id)
					return false;

				try
				{
					_ = process.Handle;
					return true;
				}
				catch
				{
					return false;
				}
			}
			catch
			{
				return false;
			}
		}

		private string GetProcessOwner(Process process)
		{
			try
			{
				using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ProcessId = {process.Id}"))
				{
					using (var results = searcher.Get())
					{
						var mo = results.Cast<ManagementObject>().FirstOrDefault();
						string[] args = new string[] { string.Empty, string.Empty };
						mo?.InvokeMethod("GetOwner", args);
						return $"{args[1]}\\{args[0]}";
					}
				}
			}
			catch
			{
				return "SYSTEM";
			}
		}

		private ProcessStatus GetProcessStatus(Process process)
		{
			try
			{
				if (process.HasExited)
					return ProcessStatus.Terminated;

				var threads = process.Threads;
				foreach (ProcessThread thread in threads)
				{
					if (thread.ThreadState == System.Diagnostics.ThreadState.Wait &&
						thread.WaitReason == ThreadWaitReason.Suspended)
					{
						return ProcessStatus.Suspended;
					}
				}
				return ProcessStatus.Running;
			}
			catch
			{
				return ProcessStatus.Unknown;
			}
		}

		private Platform GetProcessPlatform(Process process)
		{
			try
			{
				if (Environment.Is64BitOperatingSystem)
				{
					bool is64Bit = Is64BitProcess(process.Handle);
					return is64Bit ? Platform.X64 : Platform.X86;
				}
				return Platform.X86;
			}
			catch
			{
				return Platform.Unknown;
			}
		}

		private UACVirtualization GetUACVirtualization(Process process)
		{
			try
			{
				using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ProcessId = {process.Id}"))
				{
					using (var results = searcher.Get())
					{
						var mo = results.Cast<ManagementObject>().FirstOrDefault();
						if (mo != null)
						{
							var virtualization = (uint)mo["VirtualizationState"];
							return virtualization switch
							{
								0 => UACVirtualization.Disabled,
								1 => UACVirtualization.Enabled,
								2 => UACVirtualization.NotAllowed,
								_ => UACVirtualization.NotAllowed,
							};
						}
					}
				}
				return UACVirtualization.NotAllowed;
			}
			catch
			{
				return UACVirtualization.NotAllowed;
			}
		}

		private string GetProcessDescription(Process process)
		{
			try
			{
				return process.MainModule?.FileVersionInfo?.FileDescription ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		public void Dispose()
		{
			ClearCpuCache();
		}

		private async Task<TaskManagerModel> SearchProcessAsync(string searchKeyword)
		{
			return await Task.Run(() =>
			{
				var model = new TaskManagerModel
				{
					ClientId = Guid.NewGuid().ToString(),
					ClientName = Environment.MachineName,
					Timestamp = DateTime.UtcNow,
					Processes = new List<ProcessInfo>()
				};

				try
				{
					if (string.IsNullOrWhiteSpace(searchKeyword))
						throw new ArgumentException("Search keyword cannot be empty");

					var processes = new List<ProcessInfo>();
					var processList = Process.GetProcesses();

					var searchResults = processList.AsParallel().Where(process =>
					{
						try
						{
							var processName = process.ProcessName ?? "";
							var description = GetProcessDescription(process) ?? "";

							return processName.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
								   description.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
								   process.Id.ToString().Contains(searchKeyword) || // Search by PID
								   GetProcessOwner(process).Contains(searchKeyword, StringComparison.OrdinalIgnoreCase); // Search by owner
						}
						catch
						{
							return false;
						}
					});

					foreach (var process in searchResults)
					{
						try
						{
							var processInfo = new ProcessInfo
							{
								Name = process.ProcessName,
								PID = process.Id,
								Status = GetProcessStatus(process),
								UserName = GetProcessOwner(process),
								CPU = CalculateCpuUsage(process),
								WorkingSet = process.WorkingSet64,
								Platform = GetProcessPlatform(process),
								UACVirtualization = GetUACVirtualization(process),
								Description = GetProcessDescription(process)
							};
							processes.Add(processInfo);
						}
						catch
						{
							continue;
						}
					}

					processes = processes.OrderBy(p =>
						p.Name.Equals(searchKeyword, StringComparison.OrdinalIgnoreCase) ? 0 :
						p.Name.StartsWith(searchKeyword, StringComparison.OrdinalIgnoreCase) ? 1 :
						p.Description.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ? 2 : 3
					).ThenBy(p => p.Name).ToList();

					model.Processes = processes;
					model.OperationResult = new TaskManagerOperationResult
					{
						OperationType = TaskManagerOperationType.SearchProcess,
						Success = true,
						ProcessesCount = processes.Count,
						SearchKeyword = searchKeyword,
					};

					return model;
				}
				catch (Exception ex)
				{
					model.OperationResult = new TaskManagerOperationResult
					{
						OperationType = TaskManagerOperationType.SearchProcess,
						Success = false,
						ErrorMessage = ex.Message,
						SearchKeyword = searchKeyword
					};
					return model;
				}
			});
		}
	}
}