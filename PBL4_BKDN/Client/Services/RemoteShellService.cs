using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Common.Models;
using Common.Networking;

namespace Client.Services
{
    public sealed class RemoteShellService
    {
        public async Task<RemoteShellModel> ExecuteCommandAsync(RemoteShellRequest request)
        {
            var model = new RemoteShellModel
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
                var processInfo = new ProcessStartInfo
                {
                    FileName = request.UseShell ? "cmd.exe" : request.Command.Split(' ')[0],
                    Arguments = request.UseShell ? $"/c \"{request.Command}\"" : string.Join(" ", request.Command.Split(' ').Skip(1)),
                    WorkingDirectory = commandResult.WorkingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                var completed = await Task.WhenAny(
                    Task.Run(() => process.WaitForExit()),
                    Task.Delay(TimeSpan.FromSeconds(request.TimeoutSeconds))
                );

                if (!process.HasExited)
                {
                    process.Kill();
                    commandResult.ErrorMessage = "Command timed out";
                }
                else
                {
                    commandResult.ExitCode = process.ExitCode;
                    commandResult.StdOutput = await outputTask;
                    commandResult.StdError = await errorTask;
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
