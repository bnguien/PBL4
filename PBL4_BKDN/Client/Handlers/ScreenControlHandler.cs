using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Models;
using Common.Networking;
using Common.Utils;
using Client.Services;
using Client.Networking;

namespace Client.Handlers
{
    public class ScreenControlHandler
    {
        private readonly ScreenControlService _service;
        private readonly ClientConnection _connection;

        public ScreenControlHandler(ScreenControlService service, ClientConnection connection)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task HandleStartAsync(ScreenControlStart request)
        {
            try
            {
                Console.WriteLine($"[ScreenControl] Received start request with settings: Quality={request.Settings?.Quality}, FrameRate={request.Settings?.FrameRate}");
                if (request.Settings != null)
                {
                    // Configure UDP if provided
                    if (!string.IsNullOrEmpty(request.SessionId) && request.UdpPort > 0 && !string.IsNullOrEmpty(_connection.RemoteHost))
                    {
                        _service.ConfigureUdp(_connection.RemoteHost!, request.UdpPort, request.SessionId);
                    }
                    _service.StartCapture(request.Settings);
                }

                // Gửi response về server
                var response = new ScreenControlResponse
                {
                    Status = Common.Enums.ResponseStatusType.Ok,
                    ClientId = Environment.MachineName,
                    RequestId = request.RequestId,
                    OperationResult = new ScreenControlOperationResult
                    {
                        OperationType = ScreenControlOperationType.Start,
                        Success = true,
                        ClientId = Environment.MachineName,
                        Timestamp = DateTime.Now
                    }
                };

                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
            catch (Exception ex)
            {
                var response = new ScreenControlResponse
                {
                    Status = Common.Enums.ResponseStatusType.Error,
                    ErrorMessage = ex.Message,
                    ClientId = Environment.MachineName,
                    RequestId = request.RequestId,
                    OperationResult = new ScreenControlOperationResult
                    {
                        OperationType = ScreenControlOperationType.Start,
                        Success = false,
                        ErrorMessage = ex.Message,
                        ClientId = Environment.MachineName,
                        Timestamp = DateTime.Now
                    }
                };

                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
        }

        public async Task HandleStopAsync(ScreenControlStop request)
        {
            try
            {
                _service.StopCapture();

                // Gửi response về server
                var response = new ScreenControlResponse
                {
                    Status = Common.Enums.ResponseStatusType.Ok,
                    ClientId = Environment.MachineName,
                    RequestId = request.RequestId,
                    OperationResult = new ScreenControlOperationResult
                    {
                        OperationType = ScreenControlOperationType.Stop,
                        Success = true,
                        ClientId = Environment.MachineName,
                        Timestamp = DateTime.Now
                    }
                };

                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
            catch (Exception ex)
            {
                var response = new ScreenControlResponse
                {
                    Status = Common.Enums.ResponseStatusType.Error,
                    ErrorMessage = ex.Message,
                    ClientId = Environment.MachineName,
                    RequestId = request.RequestId,
                    OperationResult = new ScreenControlOperationResult
                    {
                        OperationType = ScreenControlOperationType.Stop,
                        Success = false,
                        ErrorMessage = ex.Message,
                        ClientId = Environment.MachineName,
                        Timestamp = DateTime.Now
                    }
                };

                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
        }

        public async Task HandleMouseEventAsync(ScreenControlMouseEvent request)
        {
            try
            {
                if (request.MouseEvent != null)
                {
                    await SimulateMouseEvent(request.MouseEvent);
                }

                // Gửi response về server
                var response = new ScreenControlResponse
                {
                    Status = Common.Enums.ResponseStatusType.Ok,
                    ClientId = Environment.MachineName,
                    RequestId = request.RequestId,
                    OperationResult = new ScreenControlOperationResult
                    {
                        OperationType = ScreenControlOperationType.MouseEvent,
                        Success = true,
                        ClientId = Environment.MachineName,
                        Timestamp = DateTime.Now
                    }
                };

                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
            catch (Exception ex)
            {
                var response = new ScreenControlResponse
                {
                    Status = Common.Enums.ResponseStatusType.Error,
                    ErrorMessage = ex.Message,
                    ClientId = Environment.MachineName,
                    RequestId = request.RequestId,
                    OperationResult = new ScreenControlOperationResult
                    {
                        OperationType = ScreenControlOperationType.MouseEvent,
                        Success = false,
                        ErrorMessage = ex.Message,
                        ClientId = Environment.MachineName,
                        Timestamp = DateTime.Now
                    }
                };

                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
        }

        public async Task HandleKeyboardEventAsync(ScreenControlKeyboardEvent request)
        {
            try
            {
                if (request.KeyboardEvent != null)
                {
                    await SimulateKeyboardEvent(request.KeyboardEvent);
                }

                // Gửi response về server
                var response = new ScreenControlResponse
                {
                    Status = Common.Enums.ResponseStatusType.Ok,
                    ClientId = Environment.MachineName,
                    RequestId = request.RequestId,
                    OperationResult = new ScreenControlOperationResult
                    {
                        OperationType = ScreenControlOperationType.KeyboardEvent,
                        Success = true,
                        ClientId = Environment.MachineName,
                        Timestamp = DateTime.Now
                    }
                };

                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
            catch (Exception ex)
            {
                var response = new ScreenControlResponse
                {
                    Status = Common.Enums.ResponseStatusType.Error,
                    ErrorMessage = ex.Message,
                    ClientId = Environment.MachineName,
                    RequestId = request.RequestId,
                    OperationResult = new ScreenControlOperationResult
                    {
                        OperationType = ScreenControlOperationType.KeyboardEvent,
                        Success = false,
                        ErrorMessage = ex.Message,
                        ClientId = Environment.MachineName,
                        Timestamp = DateTime.Now
                    }
                };

                var json = JsonHelper.Serialize(response);
                await _connection.SendAsync(json);
            }
        }

        private async Task SimulateMouseEvent(MouseEvent mouseEvent)
        {
            await Task.Run(() =>
            {
                try
                {
                    switch (mouseEvent.Action)
                    {
                        case MouseAction.Move:
                            Cursor.Position = new System.Drawing.Point(mouseEvent.X, mouseEvent.Y);
                            break;

                        case MouseAction.Down:
                            Cursor.Position = new System.Drawing.Point(mouseEvent.X, mouseEvent.Y);
                            MouseSimulator.MouseDown(mouseEvent.Button);
                            break;

                        case MouseAction.Up:
                            Cursor.Position = new System.Drawing.Point(mouseEvent.X, mouseEvent.Y);
                            MouseSimulator.MouseUp(mouseEvent.Button);
                            break;

                        case MouseAction.Click:
                            Cursor.Position = new System.Drawing.Point(mouseEvent.X, mouseEvent.Y);
                            MouseSimulator.MouseClick(mouseEvent.Button);
                            break;

                        case MouseAction.DoubleClick:
                            Cursor.Position = new System.Drawing.Point(mouseEvent.X, mouseEvent.Y);
                            MouseSimulator.MouseDoubleClick(mouseEvent.Button);
                            break;

                        case MouseAction.Scroll:
                            Cursor.Position = new System.Drawing.Point(mouseEvent.X, mouseEvent.Y);
                            MouseSimulator.MouseScroll(mouseEvent.ScrollDelta);
                            break;
                    }
                }
                catch (Exception)
                {
                    // Ignore mouse simulation errors
                }
            });
        }

        private async Task SimulateKeyboardEvent(KeyboardEvent keyboardEvent)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (keyboardEvent.IsPressed)
                    {
                        KeyboardSimulator.KeyDown((Keys)keyboardEvent.KeyCode, keyboardEvent.IsCtrl, keyboardEvent.IsAlt, keyboardEvent.IsShift);
                    }
                    else
                    {
                        KeyboardSimulator.KeyUp((Keys)keyboardEvent.KeyCode, keyboardEvent.IsCtrl, keyboardEvent.IsAlt, keyboardEvent.IsShift);
                    }
                }
                catch (Exception)
                {
                    // Ignore keyboard simulation errors
                }
            });
        }
    }

    // Helper classes for input simulation
    public static class MouseSimulator
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        public static void MouseDown(MouseButton button)
        {
            uint flags = button switch
            {
                MouseButton.Left => MOUSEEVENTF_LEFTDOWN,
                MouseButton.Right => MOUSEEVENTF_RIGHTDOWN,
                MouseButton.Middle => MOUSEEVENTF_MIDDLEDOWN,
                _ => 0
            };
            if (flags != 0)
                mouse_event(flags, 0, 0, 0, 0);
        }

        public static void MouseUp(MouseButton button)
        {
            uint flags = button switch
            {
                MouseButton.Left => MOUSEEVENTF_LEFTUP,
                MouseButton.Right => MOUSEEVENTF_RIGHTUP,
                MouseButton.Middle => MOUSEEVENTF_MIDDLEUP,
                _ => 0
            };
            if (flags != 0)
                mouse_event(flags, 0, 0, 0, 0);
        }

        public static void MouseClick(MouseButton button)
        {
            MouseDown(button);
            System.Threading.Thread.Sleep(50);
            MouseUp(button);
        }

        public static void MouseDoubleClick(MouseButton button)
        {
            MouseClick(button);
            System.Threading.Thread.Sleep(100);
            MouseClick(button);
        }

        public static void MouseScroll(int delta)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, 0);
        }
    }

    public static class KeyboardSimulator
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static void KeyDown(Keys key, bool ctrl = false, bool alt = false, bool shift = false)
        {
            if (ctrl) keybd_event((byte)Keys.ControlKey, 0, 0, 0);
            if (alt) keybd_event((byte)Keys.Menu, 0, 0, 0);
            if (shift) keybd_event((byte)Keys.ShiftKey, 0, 0, 0);
            keybd_event((byte)key, 0, 0, 0);
        }

        public static void KeyUp(Keys key, bool ctrl = false, bool alt = false, bool shift = false)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, 0);
            if (shift) keybd_event((byte)Keys.ShiftKey, 0, KEYEVENTF_KEYUP, 0);
            if (alt) keybd_event((byte)Keys.Menu, 0, KEYEVENTF_KEYUP, 0);
            if (ctrl) keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}
