using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace Common.Models
{
    public sealed class MessageBoxModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Caption { get; set; } = "Thông báo"; 
        public string Content { get; set; } = string.Empty; 
        public MessageBoxButtonType Buttons { get; set; } = MessageBoxButtonType.OK;
        public MessageBoxIconType Icon { get; set; } = MessageBoxIconType.Information;
        public string Result { get; set; } = string.Empty;
    }
}
