using System.Collections.Concurrent;
using Common.Networking;

namespace Server.Handlers
{
    public sealed class SystemInfoHandler
    {
        private readonly ConcurrentDictionary<string, SystemInfoResponse> _lastResponses = new ConcurrentDictionary<string, SystemInfoResponse>();

        public void SaveLastResponse(string clientId, SystemInfoResponse response)
        {
            _lastResponses[clientId] = response;
        }

        public bool TryGetLastResponse(string clientId, out SystemInfoResponse? response)
        {
            return _lastResponses.TryGetValue(clientId, out response);
        }
    }
}


