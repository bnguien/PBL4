using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Common.Models;
using Common.Networking;

namespace Server.Services
{
	public sealed class UdpScreenReceiver : IDisposable
	{
		private readonly UdpClient _udp;
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();
		public int Port { get; }
		public event EventHandler<ScreenControlFrame>? OnFrame;
        private readonly ConcurrentDictionary<string, Reassembly> _reassembly = new ConcurrentDictionary<string, Reassembly>();
        private sealed class Reassembly
        {
            public int Width;
            public int Height;
            public int Quality;
            public string ClientId = string.Empty;
            public ushort ChunkCount;
            public ushort ChunkPayloadSize;
            public int Received;
            public byte[] Buffer = Array.Empty<byte>();
            public DateTime Started = DateTime.UtcNow;
        }

		public UdpScreenReceiver(int port)
		{
			Port = port;
			_udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));
			// logging disabled for performance
			_ = ReceiveLoopAsync(_cts.Token);
		}

		private async System.Threading.Tasks.Task ReceiveLoopAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				UdpReceiveResult res;
				try
				{
					res = await _udp.ReceiveAsync(token);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch
				{
					continue;
				}

				var data = res.Buffer;
				// logging disabled for performance
				if (data.Length < 4 + 1 + 16 + 8 + 4 + 4 + 4 + 1 + 2) continue;
				int o = 0;
				if (data[o++] != (byte)'S' || data[o++] != (byte)'C' || data[o++] != (byte)'R' || data[o++] != (byte)'N') continue;
				var ver = data[o++]; if (ver != 1) continue;
				var sessionBytes = new byte[16]; Array.Copy(data, o, sessionBytes, 0, 16); o += 16;
				var sessionId = new Guid(sessionBytes);
				long frameNo = BitConverter.ToInt64(data, o); o += 8;
				int w = BitConverter.ToInt32(data, o); o += 4;
				int h = BitConverter.ToInt32(data, o); o += 4;
				int q = BitConverter.ToInt32(data, o); o += 4;
				byte flags = data[o++];
				ushort clientLen = BitConverter.ToUInt16(data, o); o += 2;
				if (o + clientLen > data.Length) continue;
				var clientId = Encoding.UTF8.GetString(data, o, clientLen); o += clientLen;
				if (o + 4 > data.Length) continue;
                ushort chunkPayloadSize = BitConverter.ToUInt16(data, o); o += 2;
                ushort chunkIndex = BitConverter.ToUInt16(data, o); o += 2;
                ushort chunkCount = BitConverter.ToUInt16(data, o); o += 2;
				var payloadLen = data.Length - o;
				if (payloadLen <= 0) continue;

				var key = sessionId.ToString("N") + ":" + frameNo.ToString();
				var asm = _reassembly.GetOrAdd(key, _ => new Reassembly
				{
					Width = w,
					Height = h,
					Quality = q,
					ClientId = clientId,
                    ChunkCount = chunkCount,
                    ChunkPayloadSize = chunkPayloadSize,
                    Buffer = new byte[(chunkCount - 1) * chunkPayloadSize + payloadLen]
				});
				// copy chunk
                var dstOffset = chunkIndex * asm.ChunkPayloadSize;
                if (dstOffset + payloadLen > asm.Buffer.Length) continue;
                Buffer.BlockCopy(data, o, asm.Buffer, dstOffset, payloadLen);
				asm.Received++;
				if (asm.Received >= asm.ChunkCount)
				{
					var frame = new ScreenControlFrame
					{
						ClientId = asm.ClientId,
						Frame = new ScreenFrame
						{
							ImageData = asm.Buffer,
							Width = asm.Width,
							Height = asm.Height,
							FrameNumber = frameNo,
							Timestamp = DateTime.Now,
							Quality = asm.Quality,
							IsCompressed = true
						}
					};
					OnFrame?.Invoke(this, frame);
					_reassembly.TryRemove(key, out _);
				}
			}
		}

		public void Dispose()
		{
			try { _cts.Cancel(); } catch { }
			try { _udp.Dispose(); } catch { }
		}
	}
}


