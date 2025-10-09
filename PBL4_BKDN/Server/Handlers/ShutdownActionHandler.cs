using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Networking;

namespace Server.Handlers
{
    public sealed class ShutdownActionHandler
    {

        private readonly ConcurrentDictionary<string, ShutdownActionResponse> _lastResponses = new ConcurrentDictionary<string, ShutdownActionResponse>();

        public event EventHandler<ShutdownActionResponse>? OnResponseReceived;

        public void SaveLastResponses(string clientId, ShutdownActionResponse response)
        {
            _lastResponses[clientId] = response;
            OnResponseReceived?.Invoke(this, response);
        }

        public bool TryGetLastResponse(string clientId, out ShutdownActionResponse? response)
        {
            return _lastResponses.TryGetValue(clientId, out response);
        }
    }
}
