using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Common.Models;
using Common.Networking;

namespace Client.Services
{
    public sealed class MessageBoxService
    {
        public async Task<MessageBoxModel> ShowMessageBoxAsync(MessageBoxRequest request)
        {
            MessageBoxModel model = null;

            await Task.Run(() =>
            {
                Application.OpenForms[0]?.Invoke((Action)(() =>
                {
                    var msg = request.Message;
                    MessageBoxButtons buttons = ConvertButtons(msg.Buttons);
                    MessageBoxIcon icon = ConvertIcon(msg.Icon);

                    DialogResult result = MessageBox.Show(
                        msg.Content,
                        msg.Caption,
                        buttons,
                        icon
                    );

                    model = new MessageBoxModel
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = Environment.MachineName,
                        Timestamp = DateTime.UtcNow,
                        Caption = msg.Caption,
                        Content = msg.Content,
                        Buttons = msg.Buttons,
                        Icon = msg.Icon,
                        Result = result.ToString()
                    };
                }));
            });

            return model;
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
