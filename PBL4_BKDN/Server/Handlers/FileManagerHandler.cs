using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Common.Networking;

namespace Server.Handlers
{
    public sealed class FileManagerHandler
    {
        private readonly ConcurrentDictionary<string, FileManagerResponse> _lastResponses = new ConcurrentDictionary<string, FileManagerResponse>();
        private readonly ConcurrentDictionary<string, List<FileManagerResponse>> _responseHistory = new ConcurrentDictionary<string, List<FileManagerResponse>>();

        public event EventHandler<FileManagerResponse>? OnResponseReceived;

        public void SaveLastResponse(string clientId, FileManagerResponse response)
        {
            _lastResponses[clientId] = response;
            
            // Add to history
            _responseHistory.AddOrUpdate(clientId, 
                new List<FileManagerResponse> { response },
                (key, existing) => 
                {
                    existing.Add(response);
                    // Keep only last 50 responses per client
                    if (existing.Count > 50)
                    {
                        existing.RemoveAt(0);
                    }
                    return existing;
                });

            OnResponseReceived?.Invoke(this, response);
        }

        public bool TryGetLastResponse(string clientId, out FileManagerResponse? response)
        {
            return _lastResponses.TryGetValue(clientId, out response);
        }

        public List<FileManagerResponse> GetResponseHistory(string clientId)
        {
            return _responseHistory.TryGetValue(clientId, out var history) ? new List<FileManagerResponse>(history) : new List<FileManagerResponse>();
        }

        public bool TryGetResponseByRequestId(string clientId, string requestId, out FileManagerResponse? response)
        {
            response = null;
            if (_responseHistory.TryGetValue(clientId, out var history))
            {
                response = history.Find(r => r.RequestId == requestId);
                return response != null;
            }
            return false;
        }

        public void SaveDownloadedFile(string fileName, byte[] fileData, string savePath)
        {
            try
            {
                var fullPath = Path.Combine(savePath, fileName);
                File.WriteAllBytes(fullPath, fileData);
            }
            catch (Exception ex)
            {
                // Log error or raise event for UI to handle
                throw new InvalidOperationException($"Failed to save file {fileName}: {ex.Message}", ex);
            }
        }
    }
}
