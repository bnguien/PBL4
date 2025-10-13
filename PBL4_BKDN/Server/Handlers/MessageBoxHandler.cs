using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Networking;

namespace Server.Handlers
{
    public sealed class MessageBoxHandler
    {
        private readonly ConcurrentDictionary<string, MessageBoxResponse> _lastResponses = new ConcurrentDictionary<string, MessageBoxResponse>();

        public event EventHandler<MessageBoxResponse>? OnResponseReceived;

        public void SaveLastResponses(string clientId, MessageBoxResponse response)
        {
            _lastResponses[clientId] = response;
            OnResponseReceived?.Invoke(this, response);
        }

        public bool TryGetLastResponse(string clientId, out MessageBoxResponse? response)
        {
            return _lastResponses.TryGetValue(clientId, out response);
        }
    }
}
