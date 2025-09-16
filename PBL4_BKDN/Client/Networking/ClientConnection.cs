using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Utils;

namespace Client.Networking
{
    public sealed class ClientConnection : IDisposable
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private CancellationTokenSource? _cts;

        public event Action<string>? OnLineReceived;
        public event Action<Exception>? OnReadError;
        public event Action? OnDisconnected;

        public bool IsConnected => _tcpClient?.Connected == true;

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port, cancellationToken);
            _stream = _tcpClient.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8, false, 8192, true);
            _writer = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
            _cts = new CancellationTokenSource();
            _ = ReadLoopAsync(_cts.Token);
        }

        public async Task SendAsync(string line, CancellationToken cancellationToken = default)
        {
            if (_writer == null) throw new InvalidOperationException("Not connected");
            if (line.Length > 10_000_000) throw new InvalidOperationException("Message too large");
            await _writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }

        private async Task ReadLoopAsync(CancellationToken token)
        {
            try
            {
                if (_reader == null) return;
                while (!token.IsCancellationRequested)
                {
                    var line = await _reader.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }
                    OnLineReceived?.Invoke(line);
                }
            }
            catch (Exception ex)
            {
                OnReadError?.Invoke(ex);
            }
            finally
            {
                OnDisconnected?.Invoke();
            }
        }

        public void Dispose()
        {
            try { _cts?.Cancel(); } catch { }
            try { _reader?.Dispose(); } catch { }
            try { _writer?.Dispose(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _tcpClient?.Close(); } catch { }
        }
    }
}


