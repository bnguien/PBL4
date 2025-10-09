using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace Common.Models
{
    public sealed class ShutdownActionModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public ShutdownAction Action { get; set; }   
        public CommandResultModel? CommandResult { get; set; }
    }

}
