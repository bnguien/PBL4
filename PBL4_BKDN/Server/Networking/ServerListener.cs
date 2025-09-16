using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Networking
{
    public sealed class ServerListener
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public ConcurrentDictionary<string, ServerClientConnection> Clients { get; } = new ConcurrentDictionary<string, ServerClientConnection>();

        public event Action<ServerClientConnection>? OnClientConnected;

        public ServerListener(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);
        }

        public void Start()
        {
            _listener.Start();
            _ = AcceptLoopAsync(_cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
            foreach (var kv in Clients)
            {
                kv.Value.Dispose();
            }
            Clients.Clear();
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient? client = null;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                var conn = new ServerClientConnection(client);
                if (Clients.TryAdd(conn.Id, conn))
                {
                    OnClientConnected?.Invoke(conn);
                }
            }
        }
    }
}


