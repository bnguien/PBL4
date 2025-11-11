using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Networking;

namespace Server.Handlers
{
    public sealed class ScreenControlHandler
    {
        private readonly ConcurrentDictionary<string, ScreenControlResponse> _lastResponses = new ConcurrentDictionary<string, ScreenControlResponse>();
        private readonly ConcurrentDictionary<string, List<ScreenControlFrame>> _frameBuffers = new ConcurrentDictionary<string, List<ScreenControlFrame>>();
        private readonly object _lockObject = new object();

        public event EventHandler<ScreenControlResponse>? OnResponseReceived;
        public event EventHandler<ScreenControlFrame>? OnFrameReceived;

        public void SaveLastResponse(string clientId, ScreenControlResponse response)
        {
            _lastResponses[clientId] = response;
            OnResponseReceived?.Invoke(this, response);
        }

        public void SaveFrame(string clientId, ScreenControlFrame frame)
        {
            lock (_lockObject)
            {
                if (!_frameBuffers.ContainsKey(clientId))
                {
                    _frameBuffers[clientId] = new List<ScreenControlFrame>();
                }

                var frames = _frameBuffers[clientId];
                frames.Add(frame);

                // Giới hạn buffer size để tránh memory leak
                if (frames.Count > 10)
                {
                    frames.RemoveAt(0);
                }
            }

            OnFrameReceived?.Invoke(this, frame);
        }

        public bool TryGetLastResponse(string clientId, out ScreenControlResponse? response)
        {
            return _lastResponses.TryGetValue(clientId, out response);
        }

        public List<ScreenControlFrame> GetRecentFrames(string clientId, int count = 5)
        {
            lock (_lockObject)
            {
                if (_frameBuffers.TryGetValue(clientId, out var frames))
                {
                    return frames.TakeLast(count).ToList();
                }
                return new List<ScreenControlFrame>();
            }
        }

        public void ClearFrames(string clientId)
        {
            lock (_lockObject)
            {
                if (_frameBuffers.TryGetValue(clientId, out var frames))
                {
                    frames.Clear();
                }
            }
        }

        public void RemoveClient(string clientId)
        {
            _lastResponses.TryRemove(clientId, out _);
            lock (_lockObject)
            {
                _frameBuffers.TryRemove(clientId, out _);
            }
        }
    }
}