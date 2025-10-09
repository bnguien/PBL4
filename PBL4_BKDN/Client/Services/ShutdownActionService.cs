using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Networking;
using Common.Enums;
using Common.Models;
using Common.Networking;

namespace Client.Services
{
    public sealed class ShutdownActionService
    {
        private readonly ClientConnection _client;
        public async Task<ShutdownActionModel> ExecuteCommandAsync(ShutdownActionRequest request)
        {
            var model = new ShutdownActionModel
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientName = Environment.MachineName,
                Timestamp = DateTime.UtcNow
            };

            var commandResult = new CommandResultModel
            {
                Command = request.Command,
                ExecutedAt = DateTime.UtcNow,
                WorkingDirectory = request.WorkingDirectory ?? Environment.CurrentDirectory
            };

            var stopwatch = Stopwatch.StartNew();
            try
            {
                switch (request.Action)
                {
                    case ShutdownAction.Shutdown:
                        var shutdownInfo = new ProcessStartInfo("shutdown", "/s /t 0")
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true
                        };
                        Process.Start(shutdownInfo);
                        break;

                    case ShutdownAction.Restart:
                        var restartInfo = new ProcessStartInfo("shutdown", "/r /t 0")
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true
                        };
                        Process.Start(restartInfo);
                        break;

                    case ShutdownAction.Standby:
                        Application.SetSuspendState(PowerState.Suspend, true, true);
                        break;

                    case ShutdownAction.Elevate:
                        var elevateInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe", 
                            Verb = "runas",
                            Arguments = "/k START \"\" \"" + Application.ExecutablePath + "\" & EXIT",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true
                        };
                        Process.Start(elevateInfo);
                        break;
                }
            }
            catch (Exception ex)
            {
                commandResult.ErrorMessage = ex.Message;
            }
            finally
            {
                commandResult.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                model.CommandResult = commandResult;
            }

            return model;
        }
    }
}
