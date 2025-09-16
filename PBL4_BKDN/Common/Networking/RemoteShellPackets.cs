using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Common.Models;

namespace Common.Networking
{
    public sealed class RemoteShellRequest : BasePacket
    {
        public RemoteShellRequest() 
        { 
            PacketType = PacketType.RemoteShellRequest;
        }
        public string Command { get; set; } = string.Empty;
        public string? WorkingDirectory { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public bool IncludeStdErr { get; set; } = true;
        public bool UseShell { get; set; } = true;
    }
    public sealed class RemoteShellResponse : BasePacket
    {    
        public RemoteShellResponse()
        {
            PacketType = PacketType.RemoteShellResponse;
        }

         public ResponseStatusType Status { get; set; } = ResponseStatusType.Unknown;
         public RemoteShellModel? Payload { get; set; }
         public int? PayloadSizeBytes { get; set; }
         public string? ErrorMessage { get; set; }
    }
}
