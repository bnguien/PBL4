using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace Server.Networking
{
    public sealed class ServerClientConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public string Id { get; } = Guid.NewGuid().ToString();
        public string RemoteAddress { get; }
        public DateTime ConnectedAt { get; } = DateTime.Now;
        public event Action<string, string>? OnLineReceived; // (clientId, line)
        public event Action<string, Exception>? OnReadError; // (clientId, ex)
        public event Action<string>? OnDisconnected; // (clientId)

        public ServerClientConnection(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8, false, 8192, true);
            _writer = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
            var ep = client.Client.RemoteEndPoint as IPEndPoint;
            RemoteAddress = ep?.Address.ToString() ?? string.Empty;
            _ = ReadLoopAsync(_cts.Token);
        }

        public async Task SendAsync(string line, CancellationToken cancellationToken = default)
        {
            if (line.Length > 10_000_000) throw new InvalidOperationException("Message too large");
            await _writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }

        private async Task ReadLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var line = await _reader.ReadLineAsync();
                    if (line == null) break;
                    OnLineReceived?.Invoke(Id, line);
                }
            }
            catch (Exception ex)
            {
                OnReadError?.Invoke(Id, ex);
            }
            finally
            {
                OnDisconnected?.Invoke(Id);
            }
        }
        public void Disconnect()
        {
            try { _cts.Cancel(); } catch { }
            try { _reader.Dispose(); } catch { }
            try { _writer.Dispose(); } catch { }
            try { _stream.Dispose(); } catch { }
            try { _client.Close(); } catch { }

            OnDisconnected?.Invoke(Id);
        }

        public void Dispose()
        {
            try { _cts.Cancel(); } catch { }
            try { _reader.Dispose(); } catch { }
            try { _writer.Dispose(); } catch { }
            try { _stream.Dispose(); } catch { }
            try { _client.Close(); } catch { }
        }
    }
}


