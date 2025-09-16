using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Networking;

namespace Server.Handlers
{
    public sealed class RemoteShellHandler
    {
        private readonly ConcurrentDictionary<string, RemoteShellResponse> _lastResponses = new ConcurrentDictionary<string, RemoteShellResponse>();

        public event EventHandler<RemoteShellResponse>? OnResponseReceived;

        public void SaveLastResponses(string clientId, RemoteShellResponse response)
        {
            _lastResponses[clientId] = response;
            OnResponseReceived?.Invoke(this, response);
        }

        public bool TryGetLastResponse(string clientId, out RemoteShellResponse? response)
        {
            return _lastResponses.TryGetValue(clientId, out response);
        }
    }
}
