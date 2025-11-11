using System;
using System.Collections.Generic;
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
    public partial class MessageBoxForm : Form
    {
        private readonly ServerClientConnection? _connection;
        private readonly CommandService _commandService;
        private readonly MessageBoxHandler _messageBoxHandler;
        private string _currentWorkingDirectory = string.Empty;
        private static readonly Dictionary<string, MessageBoxForm> OpenedForms = new();

        public static MessageBoxForm CreateNewOrGetExisting(ServerClientConnection connection, MessageBoxHandler messageBoxHandler)
        {
            if (OpenedForms.TryGetValue(connection.Id, out var existing))
                return existing;

            var f = new MessageBoxForm(connection, messageBoxHandler);
            f.Disposed += (_, __) => OpenedForms.Remove(connection.Id);
            OpenedForms[connection.Id] = f;
            return f;
        }

        public MessageBoxForm(ServerClientConnection connection, MessageBoxHandler messageBoxHandler)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _commandService = new CommandService();
            _messageBoxHandler = messageBoxHandler ?? throw new ArgumentNullException(nameof(messageBoxHandler));

            InitializeComponent();      
            RegisterEventHandlers();   
            InitializeConsole();        
        }
        public MessageBoxForm(MessageBoxHandler messageBoxHandler)
        {
            _connection = null;  
            _commandService = new CommandService();
            _messageBoxHandler = messageBoxHandler ?? throw new ArgumentNullException(nameof(messageBoxHandler));

            InitializeComponent();
            RegisterEventHandlers();
            InitializeConsole();
        }
        private void AppendStatus(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action(() => AppendStatus(msg)));
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        }

        private void InitializeConsole()
        {
            // Bind enum vào combobox Buttons
            cmbButtons.DataSource = Enum.GetValues(typeof(MessageBoxButtonType));
            cmbButtons.SelectedItem = MessageBoxButtonType.OK; 

            // Bind enum vào combobox Icon
            cmbIcon.DataSource = Enum.GetValues(typeof(MessageBoxIconType));
            cmbIcon.SelectedItem = MessageBoxIconType.Information;  

            // Set caption và content mặc định
            txtCaption.Text = "Thông báo";
            txtContent.Text = "Nội dung thông báo sẽ hiển thị tại đây...";
        }

        private void RegisterEventHandlers()
        {
            cmbButtons.SelectedIndexChanged += (s, e) =>
            {
                AppendStatus($"Button type changed: {cmbButtons.SelectedItem}");
            };

            cmbIcon.SelectedIndexChanged += (s, e) =>
            {
                AppendStatus($"Icon type changed: {cmbIcon.SelectedItem}");
            };
        }

        private void btnPreview_Click(object? sender, EventArgs e)
        {
            var model = BuildModel();
            System.Windows.Forms.MessageBox.Show(
                model.Content,
                model.Caption,
                ConvertButtons(model.Buttons),
                ConvertIcon(model.Icon)
            );
        }

        private async void btnSend_Click(object? sender, EventArgs e)
        {
            var model = BuildModel();
            var request = new MessageBoxRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Message = model
            };

            if (_connection != null)
            {
                await _commandService.SendMessageBoxRequestAsync(_connection, request);
                AppendStatus($"Đã gửi MessageBox đến client {_connection.Id}");
            }
            else
            {
                if (Owner is MainServerForm mainForm)
                {
                    var selectedClients = mainForm.GetSelectedClients();
                    foreach (var conn in selectedClients)
                    {
                        await _commandService.SendMessageBoxRequestAsync(conn, request);
                        AppendStatus($"Đã gửi MessageBox đến client {conn.Id}");
                    }
                }
            }
        }

        public MessageBoxModel BuildModel()
        {
            return new MessageBoxModel
            {
                Caption = txtCaption.Text,
                Content = txtContent.Text,
                Buttons = (MessageBoxButtonType)(cmbButtons.SelectedItem ?? MessageBoxButtonType.OK),
                Icon = (MessageBoxIconType)(cmbIcon.SelectedItem ?? MessageBoxIconType.Information)
            };
        }

        private MessageBoxButtons ConvertButtons(MessageBoxButtonType type)
        {
            return type switch
            {
                MessageBoxButtonType.OK => MessageBoxButtons.OK,
                MessageBoxButtonType.OKCancel => MessageBoxButtons.OKCancel,
                MessageBoxButtonType.YesNo => MessageBoxButtons.YesNo,
                MessageBoxButtonType.YesNoCancel => MessageBoxButtons.YesNoCancel,
                MessageBoxButtonType.RetryCancel => MessageBoxButtons.RetryCancel,
                MessageBoxButtonType.AbortRetryIgnore => MessageBoxButtons.AbortRetryIgnore,
                _ => MessageBoxButtons.OK,
            };
        }

        private MessageBoxIcon ConvertIcon(MessageBoxIconType type)
        {
            return type switch
            {
                MessageBoxIconType.Information => MessageBoxIcon.Information,
                MessageBoxIconType.Warning => MessageBoxIcon.Warning,
                MessageBoxIconType.Error => MessageBoxIcon.Error,
                MessageBoxIconType.Question => MessageBoxIcon.Question,
                _ => MessageBoxIcon.None,
            };
        }
    }
}
