using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Net;
using Server.Networking;
using Server.Services;
using Server.Handlers;
using Common.Networking;
using Common.Utils;

namespace Server.Forms
{
    public partial class MainServerForm : Form
    {
        private ServerListener? _listener;
        private readonly ConcurrentDictionary<string, ServerClientConnection> _clients = new ConcurrentDictionary<string, ServerClientConnection>();
        private readonly PacketHandler _packetHandler;
        private readonly SystemInfoHandler _systemInfoHandler = new SystemInfoHandler();
        private readonly RemoteShellHandler _remoteShellHandler = new RemoteShellHandler();
        private readonly CommandService _commandService = new CommandService();
        private readonly BindingSource _clientsBinding = new BindingSource();

        public MainServerForm()
        {
            InitializeComponent();
            _packetHandler = new PacketHandler(OnSystemInfoResponse, OnRemoteShellResponse);
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
            public string ClientId { get; set; } = string.Empty; // short display
            public string FullClientId { get; set; } = string.Empty; // for lookups
            public string Hostname { get; set; } = string.Empty;
            public string IpAddress { get; set; } = string.Empty;
            public DateTime ConnectedAt { get; set; }
            public DateTime LastSeen { get; set; }
            public string Status { get; set; } = "Online";
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
