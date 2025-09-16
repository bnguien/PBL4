using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Enums;
using Common.Models;
using Common.Networking;
using Common.Utils;
using Server.Handlers;
using Server.Networking;
using Server.Services;

namespace Server.Forms
{
    public partial class RemoteShellForm : Form
    {
        private static readonly Dictionary<string, RemoteShellForm> OpenedForms = new();

        public static RemoteShellForm CreateNewOrGetExisting(ServerClientConnection connection, RemoteShellHandler remoteShellHandler)
        {
            if (OpenedForms.TryGetValue(connection.Id, out var existing))
                return existing;

            var f = new RemoteShellForm(connection, remoteShellHandler);
            f.Disposed += (_, __) => OpenedForms.Remove(connection.Id);
            OpenedForms[connection.Id] = f;
            return f;
        }

        private readonly ServerClientConnection _connection;
        private readonly CommandService _commandService;
        private readonly RemoteShellHandler _remoteShellHandler;
        private string _currentWorkingDirectory = string.Empty;
        private bool _isConnected = true;

        public RemoteShellForm(ServerClientConnection connection, RemoteShellHandler remoteShellHandler)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _commandService = new CommandService();
            _remoteShellHandler = remoteShellHandler ?? throw new ArgumentNullException(nameof(remoteShellHandler));

            InitializeComponent();      // tạo control + layout
            RegisterEventHandlers();    // gắn event khi control đã tồn tại
            InitializeConsole();        // hiển thị thông tin ban đầu
        }

        // ================== UI Creation ==================
        private void CreateControls()
        {
            // Console output area
            txtConsoleOutput = new RichTextBox
            {
                Name = "txtConsoleOutput",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(0, 0, 0),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Consolas", 10, FontStyle.Regular),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                DetectUrls = false,
                WordWrap = false
            };

            // Input area panel
            pnlInputArea = new Panel
            {
                Name = "pnlInputArea",
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(0, 0, 0),
                Padding = new Padding(5)
            };

            // Prompt label
            lblPrompt = new Label
            {
                Name = "lblPrompt",
                Text = ">",
                ForeColor = Color.FromArgb(100, 200, 100),
                Font = new Font("Consolas", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(5, 8)
            };

            // Console input
            txtConsoleInput = new TextBox
            {
                Name = "txtConsoleInput",
                Location = new Point(25, 5),
                Size = new Size(750, 20),
                BackColor = Color.FromArgb(0, 0, 0),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Consolas", 10),
                BorderStyle = BorderStyle.None
            };

            // Status strip
            statusStrip = new StatusStrip
            {
                Name = "statusStrip",
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(200, 200, 200)
            };

            statusLabel = new ToolStripStatusLabel
            {
                Name = "statusLabel",
                Text = "Ready",
                ForeColor = Color.FromArgb(200, 200, 200)
            };

            connectionStatusLabel = new ToolStripStatusLabel
            {
                Name = "connectionStatusLabel",
                Text = $"Connected: {_connection.RemoteAddress}",
                ForeColor = Color.FromArgb(100, 200, 100)
            };

            statusStrip.Items.AddRange(new ToolStripItem[]
            {
                statusLabel,
                connectionStatusLabel
            });
        }

        private void SetupLayout()
        {
            // Add controls to form
            Controls.Add(txtConsoleOutput);
            Controls.Add(pnlInputArea);
            Controls.Add(statusStrip);

            // Add controls to input panel
            pnlInputArea.Controls.Add(lblPrompt);
            pnlInputArea.Controls.Add(txtConsoleInput);

            // anchor
            txtConsoleInput.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            // một chút tối ưu vẽ
            this.DoubleBuffered = true;
        }

        // ================== Event Wiring ==================
        private void RegisterEventHandlers()
        {
            // Form events
            this.Load += RemoteShellForm_Load;
            this.FormClosing += RemoteShellForm_FormClosing;

            // Connection events
            _connection.OnDisconnected += Connection_OnDisconnected;

            // RemoteShell events
            _remoteShellHandler.OnResponseReceived += RemoteShellHandler_OnResponseReceived;

            // Control events
            txtConsoleInput.KeyDown += TxtConsoleInput_KeyDown;
            txtConsoleInput.KeyPress += TxtConsoleInput_KeyPress;
            txtConsoleOutput.KeyPress += TxtConsoleOutput_KeyPress;
        }

        private void UnregisterEventHandlers()
        {
            _connection.OnDisconnected -= Connection_OnDisconnected;
            _remoteShellHandler.OnResponseReceived -= RemoteShellHandler_OnResponseReceived;
            this.Load -= RemoteShellForm_Load;
            this.FormClosing -= RemoteShellForm_FormClosing;
            txtConsoleInput.KeyDown -= TxtConsoleInput_KeyDown;
            txtConsoleInput.KeyPress -= TxtConsoleInput_KeyPress;
            txtConsoleOutput.KeyPress -= TxtConsoleOutput_KeyPress;
        }

        // ================== Console Helpers ==================
        private void InitializeConsole()
        {
            AppendToConsole($"Connected to: {_connection.RemoteAddress}", Color.FromArgb(200, 200, 200));
            AppendToConsole($"Session ID: {_connection.Id}", Color.FromArgb(200, 200, 200));
            AppendToConsole("Type 'help' for available commands, 'exit' to close session", Color.FromArgb(255, 255, 100));
            AppendToConsole(string.Empty, Color.FromArgb(200, 200, 200));
        }

        private void AppendToConsole(string text, Color color)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendToConsole(text, color)));
                return;
            }

            txtConsoleOutput.SelectionStart = txtConsoleOutput.TextLength;
            txtConsoleOutput.SelectionLength = 0;
            txtConsoleOutput.SelectionColor = color;
            txtConsoleOutput.AppendText(text + Environment.NewLine);
            txtConsoleOutput.SelectionColor = txtConsoleOutput.ForeColor;
            txtConsoleOutput.ScrollToCaret();
        }

        private async void SendCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command) || !_isConnected)
                return;

            AppendToConsole($"> {command}", Color.FromArgb(100, 200, 100));

            try
            {
                var request = new RemoteShellRequest
                {
                    Command = command,
                    WorkingDirectory = _currentWorkingDirectory,
                    TimeoutSeconds = 30,
                    IncludeStdErr = true,
                    UseShell = true
                };

                await _commandService.SendRemoteShellRequestAsync(_connection, request);
                statusLabel.Text = "Command sent...";
            }
            catch (Exception ex)
            {
                AppendToConsole($"Error sending command: {ex.Message}", Color.FromArgb(255, 100, 100));
                statusLabel.Text = "Error sending command";
            }
        }

        // ================== Handlers ==================
        private void RemoteShellForm_Load(object? sender, EventArgs e)
        {
            txtConsoleInput.Focus();
            statusLabel.Text = "Ready";
        }

        private void RemoteShellForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_isConnected)
            {
                try { SendCommand("exit"); } catch { /* ignore */ }
            }
            UnregisterEventHandlers();
        }

        private void Connection_OnDisconnected(string clientId)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Connection_OnDisconnected(clientId)));
                return;
            }

            _isConnected = false;
            AppendToConsole("=== Connection Lost ===", Color.FromArgb(255, 100, 100));
            connectionStatusLabel.Text = "Disconnected";
            connectionStatusLabel.ForeColor = Color.FromArgb(255, 100, 100);
            txtConsoleInput.Enabled = false;
        }

        private void RemoteShellHandler_OnResponseReceived(object? sender, RemoteShellResponse response)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => RemoteShellHandler_OnResponseReceived(sender, response)));
                return;
            }

            // Chỉ xử lý response từ client hiện tại
            if (response.ClientId != _connection.Id)
                return;

            statusLabel.Text = "Response received";

            if (response.Status == ResponseStatusType.Ok && response.Payload?.CommandResult != null)
            {
                var result = response.Payload.CommandResult;

                // Hiển thị output
                if (!string.IsNullOrEmpty(result.StdOutput))
                {
                    AppendToConsole(result.StdOutput, Color.FromArgb(200, 200, 200));
                }

                // Hiển thị error nếu có
                if (!string.IsNullOrEmpty(result.StdError))
                {
                    AppendToConsole(result.StdError, Color.FromArgb(255, 100, 100));
                }

                // Cập nhật working directory
                if (!string.IsNullOrEmpty(result.WorkingDirectory))
                {
                    _currentWorkingDirectory = result.WorkingDirectory;
                }   
            }
            else
            {
                AppendToConsole($"Error: {response.ErrorMessage ?? "Unknown error"}", Color.FromArgb(255, 100, 100));
            }
        }

        private void TxtConsoleInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(txtConsoleInput.Text))
            {
                var command = txtConsoleInput.Text.Trim();
                txtConsoleInput.Clear();

                if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Close();
                }
                else if (command.Equals("cls", StringComparison.OrdinalIgnoreCase))
                {
                    txtConsoleOutput.Clear();
                    InitializeConsole();
                }
                else if (command.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    ShowHelp();
                }
                else
                {
                    SendCommand(command);
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void TxtConsoleInput_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Autocomplete/điều khiển ký tự đặc biệt
        }

        private void TxtConsoleOutput_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Redirect gõ nhầm vào output sang input
            if (e.KeyChar != (char)2)
            {
                txtConsoleInput.Text += e.KeyChar;
                txtConsoleInput.Focus();
                txtConsoleInput.SelectionStart = txtConsoleInput.Text.Length;
                txtConsoleInput.ScrollToCaret();
            }
        }

        private void ShowHelp()
        {
            AppendToConsole("For more information on a specific command, type HELP command-name", Color.FromArgb(100, 200, 100));

            string[] commands =
            {
                "DIR        - List files and subdirectories in a directory.",
                "CD <dir>   - Change the current directory.",
                "CLS        - Clear the screen.",
                "COPY       - Copy one or more files to another location.",
                "DEL        - Delete one or more files.",
                "MOVE       - Move one or more files to another directory.",
                "MKDIR      - Create a new directory.",
                "RMDIR      - Remove a directory.",
                "TYPE       - Display the contents of a text file.",
                "ECHO       - Display messages or turn echo on/off.",
                "EXIT       - Quit the command interpreter."
            };
            foreach (var cmd in commands)
            {
                AppendToConsole(cmd, Color.FromArgb(200, 200, 200));
            }
            AppendToConsole("", Color.FromArgb(200, 200, 200));
            AppendToConsole("For more information on tools see the command-line reference in the online help.", Color.FromArgb(100, 200, 100));
        }
    }
}
