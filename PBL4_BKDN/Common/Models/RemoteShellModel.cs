using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public sealed class RemoteShellModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        public CommandResultModel? CommandResult { get; set; }
    }
    public sealed class CommandResultModel
    {
        public string Command { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public int ExitCode { get; set; }
        public string StdOutput { get; set; } = string.Empty;
        public string StdError { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
