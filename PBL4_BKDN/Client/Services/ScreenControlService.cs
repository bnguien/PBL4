using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Models;
using Common.Networking;
using Common.Utils;
using Client.Networking;
using Client.Services.Capture;
using Client.Services.Encoding;

namespace Client.Services
{
    public class ScreenControlService
    {
        private System.Threading.Timer? _captureTimer; // legacy
        private CancellationTokenSource? _loopCts;
        private Task? _captureTask;
        private bool _isCapturing = false;
        private ScreenControlSettings _settings = new ScreenControlSettings();
        private long _frameNumber = 0;
        private readonly ClientConnection _connection;
        private UdpScreenSender? _udpSender;
        private IScreenCapturer? _capturer;
        private IVideoEncoder? _encoder;
        private int _slowFrames = 0;
        private int _fastFrames = 0;
        private const int AdaptThreshold = 12; // frames

        public event EventHandler<ScreenFrame>? OnFrameCaptured;
        public event EventHandler<ScreenControlOperationResult>? OnOperationCompleted;

        public ScreenControlService(ClientConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public void StartCapture(ScreenControlSettings settings)
        {
            if (_isCapturing) 
            {
                Console.WriteLine("[ScreenControl] Already capturing, ignoring start request");
                return;
            }

            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _isCapturing = true;
            _frameNumber = 0;

            // Select capturer
            _capturer = _settings.Capture == CaptureMethod.Dxgi ? new GdiCapturer() : new GdiCapturer();
            // Select encoder
            _encoder = _settings.Encode == EncodeFormat.H264 ? new JpegEncoder() : new JpegEncoder();

            // Bắt đầu vòng lặp capture pacing theo Stopwatch (mượt hơn Timer)
            var interval = Math.Max(5, 1000 / Math.Max(1, _settings.FrameRate));
            Console.WriteLine($"[ScreenControl] Starting capture with {_settings.FrameRate} FPS, interval: {interval}ms; UDP: {(_udpSender != null ? "ON" : "OFF")}");
            _loopCts = new CancellationTokenSource();
            _captureTask = Task.Run(async () => await CaptureLoopAsync(interval, _loopCts.Token));

            OnOperationCompleted?.Invoke(this, new ScreenControlOperationResult
            {
                OperationType = ScreenControlOperationType.Start,
                Success = true,
                Timestamp = DateTime.Now
            });
        }

        public void StopCapture()
        {
            if (!_isCapturing) return;

            _isCapturing = false;
            _captureTimer?.Dispose();
            _captureTimer = null;
            try { _loopCts?.Cancel(); } catch { }
            try { awaitEnd(_captureTask); } catch { }
            _loopCts = null;
            _captureTask = null;
            _udpSender?.Dispose();
            _udpSender = null;

            OnOperationCompleted?.Invoke(this, new ScreenControlOperationResult
            {
                OperationType = ScreenControlOperationType.Stop,
                Success = true,
                Timestamp = DateTime.Now
            });
        }

        public void UpdateSettings(ScreenControlSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            if (_isCapturing && _captureTimer != null)
            {
                var interval = 1000 / _settings.FrameRate;
                _captureTimer.Change(0, interval);
            }
        }

        private async Task CaptureLoopAsync(int intervalMs, CancellationToken token)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (_isCapturing && !token.IsCancellationRequested)
            {
                var loopStart = sw.ElapsedMilliseconds;
                try
                {
                    using var bitmap = _capturer?.Capture(_settings);
                    if (bitmap != null)
                    {
                        EncodeFormat fmt;
                        var t0 = sw.ElapsedMilliseconds;
                        var encoded = _encoder!.Encode(bitmap, _settings, out fmt);
                        var t1 = sw.ElapsedMilliseconds;
                var frame = new ScreenFrame
                {
                            ImageData = encoded,
                            Width = bitmap.Width,
                            Height = bitmap.Height,
                    FrameNumber = Interlocked.Increment(ref _frameNumber),
                    Timestamp = DateTime.Now,
                    Quality = _settings.Quality,
                            IsCompressed = true,
                            Format = fmt
                        };
                        if (_udpSender != null)
                        {
                            try { _udpSender.SendFrame(frame, Environment.MachineName); } catch { }
                        }
                        else
                        {
                            _ = _connection.SendAsync(JsonHelper.Serialize(new ScreenControlFrame { Frame = frame, ClientId = Environment.MachineName }));
                        }
                        OnFrameCaptured?.Invoke(this, frame);

                        // Adaptive bitrate (simple): compare encode+send time to budget
                        var t2 = sw.ElapsedMilliseconds;
                        var encodeAndSendMs = (int)(t2 - t0);
                        var budgetMs = Math.Max(5, intervalMs);
                        if (encodeAndSendMs > budgetMs * 12 / 10) // >120% budget
                        {
                            _slowFrames++;
                            _fastFrames = 0;
                        }
                        else if (encodeAndSendMs < budgetMs * 7 / 10) // <70% budget
                        {
                            _fastFrames++;
                            _slowFrames = 0;
                        }
                        else
                        {
                            _slowFrames = 0; _fastFrames = 0;
                        }

                        if (_slowFrames >= AdaptThreshold)
                        {
                            _slowFrames = 0;
                            // Prefer lowering resolution before quality
                            if (_settings.Resolution < 4)
                            {
                                _settings.Resolution *= 2; // 1->2->4
                                Console.WriteLine($"[ScreenControl][ABR] Increase Resolution factor to {_settings.Resolution}");
                            }
                            else if (_settings.Quality > 70)
                            {
                                _settings.Quality = Math.Max(70, _settings.Quality - 5);
                                Console.WriteLine($"[ScreenControl][ABR] Decrease Quality to {_settings.Quality}");
                            }
                        }
                        else if (_fastFrames >= AdaptThreshold)
                        {
                            _fastFrames = 0;
                            // Prefer raising quality before resolution
                            if (_settings.Quality < 85)
                            {
                                _settings.Quality = Math.Min(85, _settings.Quality + 5);
                                Console.WriteLine($"[ScreenControl][ABR] Increase Quality to {_settings.Quality}");
                            }
                            else if (_settings.Resolution > 1)
                            {
                                _settings.Resolution = Math.Max(1, _settings.Resolution / 2);
                                Console.WriteLine($"[ScreenControl][ABR] Decrease Resolution factor to {_settings.Resolution}");
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                OnOperationCompleted?.Invoke(this, new ScreenControlOperationResult
                {
                    OperationType = ScreenControlOperationType.FrameCapture,
                    Success = false,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
                var elapsed = (int)(sw.ElapsedMilliseconds - loopStart);
                var delay = intervalMs - elapsed;
                if (delay > 0)
                {
                    try { await Task.Delay(delay, token); } catch { }
                }
            }
        }

        public void ConfigureUdp(string serverHost, int udpPort, string sessionId)
        {
            if (udpPort <= 0 || string.IsNullOrEmpty(serverHost) || string.IsNullOrEmpty(sessionId))
            {
                _udpSender?.Dispose();
                _udpSender = null;
                return;
            }
            Console.WriteLine($"[ScreenControl] Configure UDP host={serverHost} port={udpPort} session={sessionId}");
            _udpSender = new UdpScreenSender(serverHost, udpPort, sessionId);
        }

        public void Dispose()
        {
            StopCapture();
        }

        private void awaitEnd(Task? t)
        {
            try { if (t != null) t.Wait(100); } catch { }
        }
    }
}