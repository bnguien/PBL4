using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Enums;
using Common.Models;
using Common.Networking;
using Server.Handlers;
using Server.Networking;
using Common.Utils;

namespace Server.Forms
{
	public partial class TaskManagerForm : Form
	{
		private static readonly Dictionary<string, TaskManagerForm> OpenedForms = new();

		public static TaskManagerForm CreateNewOrGetExisting(ServerClientConnection connection, TaskManagerHandler taskManagerHandler)
		{
			if (OpenedForms.TryGetValue(connection.Id, out var existing))
			{
				existing.BringToFront();
				return existing;
			}

			var form = new TaskManagerForm(connection, taskManagerHandler);
			form.FormClosed += (sender, e) => OpenedForms.Remove(connection.Id);
			OpenedForms[connection.Id] = form;

			return form;
		}

		private readonly ServerClientConnection _connection;
		private readonly TaskManagerHandler _taskManagerHandler;
		private bool _isConnected = true;

		public TaskManagerForm(ServerClientConnection connection, TaskManagerHandler taskManagerHandler)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_taskManagerHandler = taskManagerHandler ?? throw new ArgumentNullException(nameof(taskManagerHandler));

			InitializeComponent();
			SetupUI();
			RegisterEvents();
			LoadProcessList();
		}

		private void SetupUI()
		{
			this.Text = $"Task Manager - {_connection.RemoteAddress}";
			this.StartPosition = FormStartPosition.CenterScreen;

			// Configure DataGridView
			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dataGridView1.AutoGenerateColumns = false; // dùng cột tùy chỉnh đã khai báo trong Designer
			dataGridView1.RowHeadersVisible = false;
			dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dataGridView1.ReadOnly = true;

			// Map cột -> thuộc tính của ProcessInfo
			if (colName != null) colName.DataPropertyName = nameof(ProcessInfo.Name);
			if (colPID != null) colPID.DataPropertyName = nameof(ProcessInfo.PID);
			if (colStatus != null) colStatus.DataPropertyName = nameof(ProcessInfo.Status);
			if (colUserName != null) colUserName.DataPropertyName = nameof(ProcessInfo.UserName);
			if (colCPU != null) colCPU.DataPropertyName = nameof(ProcessInfo.CPU);
			if (colWorkingSet != null) colWorkingSet.DataPropertyName = nameof(ProcessInfo.WorkingSet);
			if (colPlatform != null) colPlatform.DataPropertyName = nameof(ProcessInfo.Platform);
			if (colUACVirtualization != null) colUACVirtualization.DataPropertyName = nameof(ProcessInfo.UACVirtualization);
		}

		private void RegisterEvents()
		{
			_taskManagerHandler.OnResponseReceived += OnProcessListReceived;
			_taskManagerHandler.OnOperationCompleted += OnOperationCompleted;
			_connection.OnDisconnected += OnDisconnected;

			btnRefresh.Click += BtnRefresh_Click;
			btnSearch.Click += BtnSearch_Click;
			btnEndtask.Click += BtnEndtask_Click;

			txtSearch.KeyDown += TxtSearch_KeyDown;

			// Placeholder handling
			txtSearch.Enter += TxtSearch_Enter;
			txtSearch.Leave += TxtSearch_Leave;
		}

		private async void LoadProcessList()
		{
			if (!_isConnected)
			{
				UpdateStatus("Not connected to client");
				return;
			}

			dataGridView1.DataSource = null;
			UpdateStatus("Loading processes...");

			var request = new TaskManagerRequest
			{
				ClientId = _connection.Id,
				OperationType = TaskManagerOperationType.GetProcessList,
				RequestId = Guid.NewGuid().ToString(),
				Timestamp = DateTime.Now
			};

			try
			{
				// Dùng JsonHelper để đảm bảo camelCase và enum converter phù hợp với client
				var json = JsonHelper.Serialize(request);
				await _connection.SendAsync(json);
			}
			catch (Exception ex)
			{
				UpdateStatus($"Error: {ex.Message}");
			}
		}

		private void OnProcessListReceived(object? sender, TaskManagerResponse response)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<object?, TaskManagerResponse>(OnProcessListReceived), sender, response);
				return;
			}

			if (response.ClientId == _connection.Id)
			{
				try
				{
					// Ưu tiên đọc từ Payload, fallback sang Processes nếu cần
					var allProcesses = response.Payload?.Processes
						?? (response.Processes != null ? response.Processes.SelectMany(tm => tm.Processes).ToList() : new System.Collections.Generic.List<ProcessInfo>());

					dataGridView1.DataSource = allProcesses;
					// Không gọi SaveLastResponse ở đây để tránh loop sự kiện
					UpdateStatus($"{allProcesses.Count} processes loaded");
					var stats = _taskManagerHandler.CalculateProcessStatistics(_connection.Id);
					UpdateStatistics(stats);
				}
				catch (Exception ex)
				{
					UpdateStatus($"Bind error: {ex.Message}");
				}
			}
		}

		private void OnOperationCompleted(object? sender, TaskManagerOperationResult operation)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<object?, TaskManagerOperationResult>(OnOperationCompleted), sender, operation);
				return;
			}

			switch (operation.OperationType)
			{
				case TaskManagerOperationType.KillProcess:
					if (operation.Success)
					{
						UpdateStatus($"Process {operation.TargetPID} terminated successfully");
						LoadProcessList();
					}
					else
					{
						UpdateStatus($"Failed to terminate process: {operation.ErrorMessage}");
					}
					break;

				case TaskManagerOperationType.SearchProcess:
					UpdateStatus($"Search completed: {operation.ProcessesCount} processes found");
					break;
			}
		}

		private void BtnRefresh_Click(object? sender, EventArgs e)
		{
			LoadProcessList();
		}

		private void BtnSearch_Click(object? sender, EventArgs e)
		{
			SearchProcesses();
		}

		private void BtnEndtask_Click(object? sender, EventArgs e)
		{
			EndSelectedProcess();
		}

		private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				SearchProcesses();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void TxtSearch_Enter(object? sender, EventArgs e)
		{
			if (txtSearch.Text == "Search process...")
			{
				txtSearch.Text = "";
				txtSearch.ForeColor = SystemColors.WindowText;
			}
		}

		private void TxtSearch_Leave(object? sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(txtSearch.Text))
			{
				txtSearch.Text = "Search process...";
				txtSearch.ForeColor = SystemColors.GrayText;
			}
		}

		private void SearchProcesses()
		{
			var searchTerm = txtSearch.Text.Trim();
			if (string.IsNullOrEmpty(searchTerm) || searchTerm == "Search process...")
			{
				LoadProcessList();
				return;
			}

			try
			{
				var filtered = _taskManagerHandler.SearchProcesses(_connection.Id, searchTerm);
				dataGridView1.DataSource = filtered;
				UpdateStatus($"Found {filtered.Count} processes matching '{searchTerm}'");
			}
			catch (Exception ex)
			{
				UpdateStatus($"Search error: {ex.Message}");
			}
		}

		private async void EndSelectedProcess()
		{
			if (dataGridView1.SelectedRows.Count == 0)
			{
				UpdateStatus("Please select a process to end");
				return;
			}

			if (!_isConnected)
			{
				UpdateStatus("Not connected to client");
				return;
			}

			var selectedProcess = dataGridView1.SelectedRows[0].DataBoundItem as ProcessInfo;
			if (selectedProcess != null)
			{
				var result = MessageBox.Show(
					$"Are you sure you want to end process '{selectedProcess.Name}' (PID: {selectedProcess.PID})?",
					"Confirm End Process",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning);

				if (result == DialogResult.Yes)
				{
					var request = new TaskManagerRequest
					{
						ClientId = _connection.Id,
						OperationType = TaskManagerOperationType.KillProcess,
						TargetPID = selectedProcess.PID,
						RequestId = Guid.NewGuid().ToString(),
						Timestamp = DateTime.Now
					};

					try
					{
						// Dùng JsonHelper để đồng bộ định dạng với client
						var json = JsonHelper.Serialize(request);
						await _connection.SendAsync(json);
						UpdateStatus($"Sending kill command for PID: {selectedProcess.PID}");
					}
					catch (Exception ex)
					{
						UpdateStatus($"Error sending command: {ex.Message}");
					}
				}
			}
		}

		private void OnDisconnected(string clientId)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(OnDisconnected), clientId);
				return;
			}

			_isConnected = false;
			UpdateStatus("Client disconnected");

			btnRefresh.Enabled = false;
			btnSearch.Enabled = false;
			btnEndtask.Enabled = false;
			txtSearch.Enabled = false;
		}

		private void UpdateStatus(string message)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(UpdateStatus), message);
				return;
			}

			// SỬA: Nếu không có statusStrip, dùng Text của Form hoặc Console
			this.Text = $"Task Manager - {_connection.RemoteAddress} - {message}";

			// Hoặc có thể thêm Label vào form để hiển thị status
			// Nếu có statusLabel control, dùng: statusLabel.Text = message;
		}

		private void UpdateStatistics(ProcessStatistics stats)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<ProcessStatistics>(UpdateStatistics), stats);
				return;
			}

			var statsText = $"Total: {stats.TotalProcesses} | Running: {stats.RunningProcesses} | Memory: {stats.TotalMemoryUsage / 1024 / 1024}MB | CPU: {stats.AverageCPUUsage:F1}%";

			// SỬA: Hiển thị statistics trong title hoặc tooltip
			this.Text = $"Task Manager - {_connection.RemoteAddress} - {statsText}";

			// Hoặc có thể thêm Label vào form để hiển thị statistics
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_taskManagerHandler.OnResponseReceived -= OnProcessListReceived;
			_taskManagerHandler.OnOperationCompleted -= OnOperationCompleted;
			_connection.OnDisconnected -= OnDisconnected;
			base.OnFormClosing(e);
		}
	}
}