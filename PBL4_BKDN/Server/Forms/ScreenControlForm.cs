using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Enums;
using Common.Models;
using Common.Networking;
using Common.Utils;
using Server.Handlers;
using Server.Networking;
using Server.Services;

namespace Server.Forms
{
    public partial class ScreenControlForm : Form
    {
        private static readonly Dictionary<string, ScreenControlForm> OpenedForms = new();

        public static ScreenControlForm CreateNewOrGetExisting(ServerClientConnection connection, ScreenControlHandler screenControlHandler)
        {
            if (OpenedForms.TryGetValue(connection.Id, out var existing) && !existing.IsDisposed)
                return existing;

            var f = new ScreenControlForm(connection, screenControlHandler);
            f.Disposed += (_, __) => OpenedForms.Remove(connection.Id);
            OpenedForms[connection.Id] = f;
            return f;
        }

        private readonly ServerClientConnection _connection;
        private readonly CommandService _commandService;
        private readonly ScreenControlHandler _screenControlHandler;
        private bool _isConnected = true;
        private bool _isStreaming = false;
        private ScreenControlSettings _currentSettings = new ScreenControlSettings();
        private int _lastFrameWidth = 0;
        private int _lastFrameHeight = 0;
        private string? _targetClientMachineId = null; // the client's machine name for this form session
        private string? _lastStartRequestId = null;
		private bool _isMouseOver = false;
		private DateTime _lastMouseMoveSent = DateTime.MinValue;
		private const int _mouseMoveThrottleMs = 15; // ~66 FPS max
        private long _lastDisplayedFrameNo = -1;
        private System.Windows.Forms.Timer _renderTimer = null!;
        private Image? _pendingImage = null;
        private long _pendingFrameNo = -1;
        private readonly object _imageLock = new object();
        private System.Collections.Concurrent.ConcurrentQueue<(byte[] data, long no, int w, int h)> _decodeQueue = new System.Collections.Concurrent.ConcurrentQueue<(byte[] data, long no, int w, int h)>();
        private System.Threading.CancellationTokenSource? _decodeCts;
        private Task? _decodeTask;

        // UI Controls
        private PictureBox screenDisplay = null!;
        private Panel controlPanel = null!;
        private TrackBar qualitySlider = null!;
        private TrackBar frameRateSlider = null!;
        private CheckBox enableMouseCheck = null!;
        private CheckBox enableKeyboardCheck = null!;
        private Button startStopButton = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;
        private ToolStripStatusLabel connectionStatusLabel = null!;
        private ToolStripStatusLabel frameInfoLabel = null!;
        private Label qualityLabel = null!;
        private Label frameRateLabel = null!;

        public ScreenControlForm(ServerClientConnection connection, ScreenControlHandler screenControlHandler)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _commandService = new CommandService();
            _screenControlHandler = screenControlHandler ?? throw new ArgumentNullException(nameof(screenControlHandler));

            InitializeComponent();
            RegisterEventHandlers();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.Text = $"Screen Control - {_connection.RemoteAddress}";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.KeyPreview = true; // ensure KeyDown/KeyUp fire on form

            CreateControls();
            SetupLayout();
        }

        private void CreateControls()
        {
            // Screen display area
            screenDisplay = new PictureBox
            {
                Name = "screenDisplay",
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Control panel
            controlPanel = new Panel
            {
                Name = "controlPanel",
                Dock = DockStyle.Bottom,
                Height = 120,
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Quality control
            qualityLabel = new Label
            {
                Text = "Quality: 80%",
                Location = new Point(10, 10),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9)
            };

            qualitySlider = new TrackBar
            {
                Name = "qualitySlider",
                Location = new Point(90, 10),
                Size = new Size(150, 20),
                Minimum = 10,
                Maximum = 100,
                Value = 80,
                TickFrequency = 10,
                TickStyle = TickStyle.BottomRight
            };

            // Frame rate control
            frameRateLabel = new Label
            {
                Text = "Frame Rate: 30 FPS",
                Location = new Point(10, 40),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9)
            };

            frameRateSlider = new TrackBar
            {
                Name = "frameRateSlider",
                Location = new Point(110, 40),
                Size = new Size(150, 20),
                Minimum = 5,
                Maximum = 60,
                Value = 30,
                TickFrequency = 5,
                TickStyle = TickStyle.BottomRight
            };

            // Enable controls
            enableMouseCheck = new CheckBox
            {
                Text = "Enable Mouse Control",
                Location = new Point(280, 15),
                Size = new Size(150, 20),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            enableKeyboardCheck = new CheckBox
            {
                Text = "Enable Keyboard Control",
                Location = new Point(280, 40),
                Size = new Size(150, 20),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            // Start/Stop button
            startStopButton = new Button
            {
                Text = "Start Screen Control",
                Location = new Point(450, 20),
                Size = new Size(150, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Status strip
            statusStrip = new StatusStrip
            {
                Name = "statusStrip",
                BackColor = Color.FromArgb(240, 240, 240)
            };

            statusLabel = new ToolStripStatusLabel
            {
                Text = "Ready",
                Spring = true
            };

            connectionStatusLabel = new ToolStripStatusLabel
            {
                Text = $"Connected: {_connection.RemoteAddress}",
                ForeColor = Color.Green
            };

            frameInfoLabel = new ToolStripStatusLabel
            {
                Text = "Frames: 0"
            };

            statusStrip.Items.AddRange(new ToolStripItem[]
            {
                statusLabel,
                connectionStatusLabel,
                frameInfoLabel
            });
        }

        private void SetupLayout()
        {
            // Add controls to form
            Controls.Add(screenDisplay);
            Controls.Add(controlPanel);
            Controls.Add(statusStrip);

            // Add controls to control panel
            controlPanel.Controls.AddRange(new Control[]
            {
                qualityLabel,
                qualitySlider,
                frameRateLabel,
                frameRateSlider,
                enableMouseCheck,
                enableKeyboardCheck,
                startStopButton
            });
        }

        private void RegisterEventHandlers()
        {
            // Form events
            this.Load += ScreenControlForm_Load;
            this.FormClosing += ScreenControlForm_FormClosing;

            // Connection events
            _connection.OnDisconnected += Connection_OnDisconnected;

            // Screen control events
            _screenControlHandler.OnResponseReceived += ScreenControlHandler_OnResponseReceived;
            _screenControlHandler.OnFrameReceived += ScreenControlHandler_OnFrameReceived;

			// Control events
			startStopButton.Click += StartStopButton_Click;
			qualitySlider.ValueChanged += QualitySlider_ValueChanged;
			frameRateSlider.ValueChanged += FrameRateSlider_ValueChanged;
			screenDisplay.MouseEnter += ScreenDisplay_MouseEnter;
			screenDisplay.MouseLeave += ScreenDisplay_MouseLeave;
			screenDisplay.MouseDown += ScreenDisplay_MouseDown;
			screenDisplay.MouseUp += ScreenDisplay_MouseUp;
			screenDisplay.MouseMove += ScreenDisplay_MouseMove;
			screenDisplay.MouseWheel += ScreenDisplay_MouseWheel;
			this.KeyDown += ScreenControlForm_KeyDown;
			this.KeyUp += ScreenControlForm_KeyUp;
        }

        private void UnregisterEventHandlers()
        {
            _connection.OnDisconnected -= Connection_OnDisconnected;
            _screenControlHandler.OnResponseReceived -= ScreenControlHandler_OnResponseReceived;
            _screenControlHandler.OnFrameReceived -= ScreenControlHandler_OnFrameReceived;
            this.Load -= ScreenControlForm_Load;
            this.FormClosing -= ScreenControlForm_FormClosing;
			startStopButton.Click -= StartStopButton_Click;
			qualitySlider.ValueChanged -= QualitySlider_ValueChanged;
			frameRateSlider.ValueChanged -= FrameRateSlider_ValueChanged;
			screenDisplay.MouseEnter -= ScreenDisplay_MouseEnter;
			screenDisplay.MouseLeave -= ScreenDisplay_MouseLeave;
			screenDisplay.MouseDown -= ScreenDisplay_MouseDown;
			screenDisplay.MouseUp -= ScreenDisplay_MouseUp;
			screenDisplay.MouseMove -= ScreenDisplay_MouseMove;
			screenDisplay.MouseWheel -= ScreenDisplay_MouseWheel;
			this.KeyDown -= ScreenControlForm_KeyDown;
			this.KeyUp -= ScreenControlForm_KeyUp;
        }

        private void InitializeUI()
        {
            statusLabel.Text = "Ready - Click Start to begin screen control";
            screenDisplay.Image = CreatePlaceholderImage();
            _renderTimer = new System.Windows.Forms.Timer();
            _renderTimer.Interval = 33; // ~30 FPS
            _renderTimer.Tick += (s, e) => RenderLatestFrame();
        }

        private Image CreatePlaceholderImage()
        {
            var bitmap = new Bitmap(800, 600);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                g.DrawString("Waiting for screen data...", 
                    new Font("Arial", 24), 
                    Brushes.White, 
                    new PointF(200, 280));
            }
            return bitmap;
        }

        private async void StartStopButton_Click(object? sender, EventArgs e)
        {
            if (!_isStreaming)
            {
                await StartScreenControl();
            }
            else
            {
                await StopScreenControl();
            }
        }

        private async Task StartScreenControl()
        {
            try
            {
                _currentSettings.Quality = qualitySlider.Value;
                _currentSettings.FrameRate = frameRateSlider.Value;
                _currentSettings.EnableMouse = enableMouseCheck.Checked;
                _currentSettings.EnableKeyboard = enableKeyboardCheck.Checked;

                var request = new ScreenControlStart
                {
                    Settings = _currentSettings
                };

                _lastStartRequestId = request.RequestId;
                // Provide UDP parameters (must match MainServerForm)
                request.UdpPort = 50050;
                request.SessionId = Guid.NewGuid().ToString();
                // Reset view state before starting
                _lastDisplayedFrameNo = -1;
                _pendingFrameNo = -1;
                if (_pendingImage != null && !ReferenceEquals(_pendingImage, screenDisplay.Image))
                {
                    _pendingImage.Dispose();
                }
                _pendingImage = null;
                _targetClientMachineId = null;
                screenDisplay.Image = CreatePlaceholderImage();
                Console.WriteLine($"[ScreenControlForm] Start request: UDP {request.UdpPort}, Session {request.SessionId}");
                await _commandService.SendScreenControlStartAsync(_connection, request);
                statusLabel.Text = "Starting screen control...";
                startStopButton.Text = "Stop Screen Control";
                startStopButton.BackColor = Color.FromArgb(196, 43, 28);
                _isStreaming = true;
                _renderTimer.Interval = Math.Max(10, 1000 / Math.Max(10, _currentSettings.FrameRate));
                _renderTimer.Start();
                _decodeCts = new System.Threading.CancellationTokenSource();
                _decodeTask = Task.Run(() => DecodeLoop(_decodeCts.Token));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting screen control: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task StopScreenControl()
        {
            try
            {
                var request = new ScreenControlStop();
                await _commandService.SendScreenControlStopAsync(_connection, request);
                statusLabel.Text = "Stopping screen control...";
                startStopButton.Text = "Start Screen Control";
                startStopButton.BackColor = Color.FromArgb(0, 120, 215);
                _isStreaming = false;
                _renderTimer.Stop();
                // Clear pending and keep last displayed frame as-is
                if (_pendingImage != null && !ReferenceEquals(_pendingImage, screenDisplay.Image))
                {
                    _pendingImage.Dispose();
                }
                _pendingImage = null;
                _pendingFrameNo = -1;
                try { _decodeCts?.Cancel(); } catch { }
                try { _decodeTask?.Wait(100); } catch { }
                _decodeTask = null; _decodeCts = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping screen control: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void QualitySlider_ValueChanged(object? sender, EventArgs e)
        {
            qualityLabel.Text = $"Quality: {qualitySlider.Value}%";
            _currentSettings.Quality = qualitySlider.Value;
        }

        private void FrameRateSlider_ValueChanged(object? sender, EventArgs e)
        {
            frameRateLabel.Text = $"Frame Rate: {frameRateSlider.Value} FPS";
            _currentSettings.FrameRate = frameRateSlider.Value;
        }

        private void ScreenDisplay_MouseEnter(object? sender, EventArgs e)
        {
            if (!_isStreaming) return;
            _isMouseOver = true;
            Cursor.Hide();
            screenDisplay.Focus();
        }

        private void ScreenDisplay_MouseLeave(object? sender, EventArgs e)
        {
            _isMouseOver = false;
            Cursor.Show();
        }

        private async void ScreenDisplay_MouseDown(object? sender, MouseEventArgs e)
        {
            if (!_isStreaming || !enableMouseCheck.Checked || !_isMouseOver) return;

            try
            {
                var (mx, my) = MapToFrameCoordinates(e.X, e.Y);
                var mouseEvent = new MouseEvent
                {
                    X = mx,
                    Y = my,
                    Button = e.Button switch
                    {
                        MouseButtons.Left => MouseButton.Left,
                        MouseButtons.Right => MouseButton.Right,
                        MouseButtons.Middle => MouseButton.Middle,
                        _ => MouseButton.Left
                    },
                    Action = MouseAction.Down,
                    Timestamp = DateTime.Now
                };

                var request = new ScreenControlMouseEvent
                {
                    MouseEvent = mouseEvent,
                    ClientId = _connection.Id
                };

                await _commandService.SendMouseEventAsync(_connection, request);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Mouse down error: {ex.Message}";
            }
        }

        private async void ScreenDisplay_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_isStreaming || !enableMouseCheck.Checked || !_isMouseOver) return;

            try
            {
                var (mx, my) = MapToFrameCoordinates(e.X, e.Y);
                var mouseEvent = new MouseEvent
                {
                    X = mx,
                    Y = my,
                    Button = e.Button switch
                    {
                        MouseButtons.Left => MouseButton.Left,
                        MouseButtons.Right => MouseButton.Right,
                        MouseButtons.Middle => MouseButton.Middle,
                        _ => MouseButton.Left
                    },
                    Action = MouseAction.Up,
                    Timestamp = DateTime.Now
                };

                var request = new ScreenControlMouseEvent
                {
                    MouseEvent = mouseEvent,
                    ClientId = _connection.Id
                };

                await _commandService.SendMouseEventAsync(_connection, request);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Mouse up error: {ex.Message}";
            }
        }

        private async void ScreenDisplay_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isStreaming || !enableMouseCheck.Checked || !_isMouseOver) return;

            try
            {
                var now = DateTime.UtcNow;
                if ((now - _lastMouseMoveSent).TotalMilliseconds < _mouseMoveThrottleMs)
                {
                    return;
                }
                _lastMouseMoveSent = now;

                var (mx, my) = MapToFrameCoordinates(e.X, e.Y);
                var mouseEvent = new MouseEvent
                {
                    X = mx,
                    Y = my,
                    Action = MouseAction.Move,
                    Timestamp = DateTime.Now
                };

                var request = new ScreenControlMouseEvent
                {
                    MouseEvent = mouseEvent,
                    ClientId = _connection.Id
                };

                await _commandService.SendMouseEventAsync(_connection, request);
            }
            catch (Exception)
            {
                // Ignore mouse move errors to avoid spam
            }
        }

        private async void ScreenDisplay_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (!_isStreaming || !enableMouseCheck.Checked) return;

            try
            {
                var (mx, my) = MapToFrameCoordinates(e.X, e.Y);
                var mouseEvent = new MouseEvent
                {
                    X = mx,
                    Y = my,
                    Action = MouseAction.Scroll,
                    ScrollDelta = e.Delta,
                    Timestamp = DateTime.Now
                };

                var request = new ScreenControlMouseEvent
                {
                    MouseEvent = mouseEvent,
                    ClientId = _connection.Id
                };

                await _commandService.SendMouseEventAsync(_connection, request);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Mouse wheel error: {ex.Message}";
            }
        }

        private async void ScreenControlForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!_isStreaming || !enableKeyboardCheck.Checked || !_isMouseOver) return;

            try
            {
                var keyboardEvent = new KeyboardEvent
                {
                    KeyCode = (int)e.KeyCode,
                    IsPressed = true,
                    IsCtrl = e.Control,
                    IsAlt = e.Alt,
                    IsShift = e.Shift,
                    Timestamp = DateTime.Now
                };

                var request = new ScreenControlKeyboardEvent
                {
                    KeyboardEvent = keyboardEvent,
                    ClientId = _connection.Id
                };

                await _commandService.SendKeyboardEventAsync(_connection, request);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Keyboard event error: {ex.Message}";
            }
        }

        private async void ScreenControlForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (!_isStreaming || !enableKeyboardCheck.Checked || !_isMouseOver) return;

            try
            {
                var keyboardEvent = new KeyboardEvent
                {
                    KeyCode = (int)e.KeyCode,
                    IsPressed = false,
                    IsCtrl = e.Control,
                    IsAlt = e.Alt,
                    IsShift = e.Shift,
                    Timestamp = DateTime.Now
                };

                var request = new ScreenControlKeyboardEvent
                {
                    KeyboardEvent = keyboardEvent,
                    ClientId = _connection.Id
                };

                await _commandService.SendKeyboardEventAsync(_connection, request);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Keyboard event error: {ex.Message}";
            }
        }

        // Event handlers
        private void ScreenControlForm_Load(object? sender, EventArgs e)
        {
            screenDisplay.Focus();
            statusLabel.Text = "Ready - Click Start to begin screen control";
        }

        private void ScreenControlForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_isStreaming)
            {
                _ = Task.Run(async () => await StopScreenControl());
            }
            UnregisterEventHandlers();
        }

        private void Connection_OnDisconnected(string clientId)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Connection_OnDisconnected(clientId)));
                return;
            }

            _isConnected = false;
            _isStreaming = false;
            statusLabel.Text = "=== Connection Lost ===";
            connectionStatusLabel.Text = "Disconnected";
            connectionStatusLabel.ForeColor = Color.Red;
            startStopButton.Enabled = false;
        }

        private void ScreenControlHandler_OnResponseReceived(object? sender, ScreenControlResponse response)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ScreenControlHandler_OnResponseReceived(sender, response)));
                return;
            }

            // Bind this form to the client that responded to our last Start request
            if (!string.IsNullOrEmpty(_lastStartRequestId) && response.RequestId == _lastStartRequestId)
            {
                _targetClientMachineId = response.ClientId;
                statusLabel.Text = $"Screen control active - {_targetClientMachineId}";
                return;
            }

            // Ignore unrelated responses (from other clients or other sessions)
        }

        private void ScreenControlHandler_OnFrameReceived(object? sender, ScreenControlFrame frame)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ScreenControlHandler_OnFrameReceived(sender, frame)));
                return;
            }

            // If target not set yet, bind lazily to first arriving frame
            if (string.IsNullOrEmpty(_targetClientMachineId))
            {
                _targetClientMachineId = frame.ClientId;
                statusLabel.Text = $"Screen control active - {_targetClientMachineId}";
            }
            // Only process frames that match this form's target client
            if (!string.Equals(frame.ClientId, _targetClientMachineId, StringComparison.Ordinal)) return;

            // logging disabled for performance

            try
            {
                // Drop out-of-order
                if (frame.Frame.FrameNumber <= _lastDisplayedFrameNo) return;

                // Enqueue for background decode
                _decodeQueue.Enqueue((frame.Frame.ImageData, frame.Frame.FrameNumber, frame.Frame.Width, frame.Frame.Height));
                // Bounded queue: keep small buffer to avoid backlog (latest-wins)
                while (_decodeQueue.Count > 2) { _decodeQueue.TryDequeue(out _); }
                frameInfoLabel.Text = $"Frames: {frame.Frame.FrameNumber} | {frame.Frame.Width}x{frame.Frame.Height}";
                _lastFrameWidth = frame.Frame.Width;
                _lastFrameHeight = frame.Frame.Height;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Frame display error: {ex.Message}";
            }
        }

        private void RenderLatestFrame()
        {
            if (!_isStreaming) return;
            Image? next = null; long nextNo = -1;
            lock (_imageLock)
            {
                if (_pendingImage == null) return;
                if (_pendingFrameNo <= _lastDisplayedFrameNo) return;
                next = _pendingImage;
                nextNo = _pendingFrameNo;
                _pendingImage = null; // transfer ownership
            }
            if (next == null) return;
            var oldImage = screenDisplay.Image;
            screenDisplay.Image = next;
            _lastDisplayedFrameNo = nextNo;
            try { oldImage?.Dispose(); } catch { }
        }

        private void DecodeLoop(System.Threading.CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!_decodeQueue.TryDequeue(out var item))
                    {
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }
                    // Skip stale frames
                    if (item.no <= _lastDisplayedFrameNo) { continue; }
                    using var ms = new MemoryStream(item.data);
                    // Disable validation to reduce overhead
                    using var decoded = Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: false);
                    var bmp = new Bitmap(decoded);
                    lock (_imageLock)
                    {
                        var oldPending = _pendingImage;
                        _pendingImage = bmp;
                        _pendingFrameNo = item.no;
                        if (oldPending != null && !ReferenceEquals(oldPending, screenDisplay.Image))
                        {
                            try { oldPending.Dispose(); } catch { }
                        }
                    }
                }
                catch { }
            }
        }

        private (int x, int y) MapToFrameCoordinates(int px, int py)
        {
            try
            {
                if (_lastFrameWidth <= 0 || _lastFrameHeight <= 0 || screenDisplay.Image == null)
                {
                    return (px, py);
                }

                var pbWidth = screenDisplay.ClientSize.Width;
                var pbHeight = screenDisplay.ClientSize.Height;
                if (pbWidth <= 0 || pbHeight <= 0) return (px, py);

                var imgW = _lastFrameWidth;
                var imgH = _lastFrameHeight;

                double scale = Math.Min((double)pbWidth / imgW, (double)pbHeight / imgH);
                var drawW = (int)Math.Round(imgW * scale);
                var drawH = (int)Math.Round(imgH * scale);
                var offsetX = (pbWidth - drawW) / 2;
                var offsetY = (pbHeight - drawH) / 2;

                var withinX = Math.Clamp(px - offsetX, 0, drawW);
                var withinY = Math.Clamp(py - offsetY, 0, drawH);

                var fx = (int)Math.Round(withinX / scale);
                var fy = (int)Math.Round(withinY / scale);

                fx = Math.Clamp(fx, 0, imgW - 1);
                fy = Math.Clamp(fy, 0, imgH - 1);
                return (fx, fy);
            }
            catch
            {
                return (px, py);
            }
        }
    }
}