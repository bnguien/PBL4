using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common.Models;

namespace Client.Services
{
	public sealed class UdpScreenSender : IDisposable
	{
		private readonly UdpClient _udp;
		private readonly IPEndPoint _remote;
		private readonly Guid _sessionId;
		private const int MaxDatagram = 1200; // conservative for LAN MTU

		public UdpScreenSender(string host, int port, string sessionId)
		{
			_udp = new UdpClient();
			var addrs = Dns.GetHostAddresses(host);
			if (addrs.Length == 0) throw new Exception("Cannot resolve server host");
			_remote = new IPEndPoint(addrs[0], port);
			// logging disabled for performance
			_sessionId = Guid.Parse(sessionId);
		}

		public void SendFrame(ScreenFrame frame, string clientId)
		{
			// Header: MAGIC(4) 'SCRN', VER(1)=1, SessionId(16), FrameNo(8), W(4), H(4), Q(4), Flags(1)=0,
			//          ClientIdLen(2), ClientId(utf8), ChunkPayloadSize(2), ChunkIndex(2), ChunkCount(2)
			var clientIdBytes = System.Text.Encoding.UTF8.GetBytes(clientId);
			var headerLen = 4 + 1 + 16 + 8 + 4 + 4 + 4 + 1 + 2 + clientIdBytes.Length + 2 + 2 + 2;
			var chunkPayload = MaxDatagram - headerLen;
			if (chunkPayload < 200) throw new Exception("MaxDatagram too small for header");
			var total = frame.ImageData.Length;
			var chunkCount = (ushort)Math.Max(1, (int)Math.Ceiling(total / (double)chunkPayload));
			for (ushort idx = 0; idx < chunkCount; idx++)
			{
				var offset = idx * chunkPayload;
				var len = Math.Min(chunkPayload, total - offset);
				var buf = new byte[headerLen + len];
				var span = buf.AsSpan();
				WriteHeader(span, clientIdBytes, frame, (ushort)chunkPayload, idx, chunkCount);
				frame.ImageData.AsSpan(offset, len).CopyTo(span.Slice(headerLen));
				_udp.Send(buf, buf.Length, _remote);
			}
		}

		private void WriteHeader(Span<byte> span, byte[] clientIdBytes, ScreenFrame frame, ushort chunkPayloadSize, ushort chunkIndex, ushort chunkCount)
		{
			var o = 0;
			span[o++] = (byte)'S'; span[o++] = (byte)'C'; span[o++] = (byte)'R'; span[o++] = (byte)'N';
			span[o++] = 1; // version
			_sessionId.TryWriteBytes(span.Slice(o, 16)); o += 16;
			BitConverter.GetBytes(frame.FrameNumber).CopyTo(span.Slice(o, 8)); o += 8;
			BitConverter.GetBytes(frame.Width).CopyTo(span.Slice(o, 4)); o += 4;
			BitConverter.GetBytes(frame.Height).CopyTo(span.Slice(o, 4)); o += 4;
			BitConverter.GetBytes(frame.Quality).CopyTo(span.Slice(o, 4)); o += 4;
			span[o++] = 0; // flags
			BitConverter.GetBytes((ushort)clientIdBytes.Length).CopyTo(span.Slice(o, 2)); o += 2;
			clientIdBytes.AsSpan().CopyTo(span.Slice(o)); o += clientIdBytes.Length;
			BitConverter.GetBytes(chunkPayloadSize).CopyTo(span.Slice(o, 2)); o += 2;
			BitConverter.GetBytes(chunkIndex).CopyTo(span.Slice(o, 2)); o += 2;
			BitConverter.GetBytes(chunkCount).CopyTo(span.Slice(o, 2)); o += 2;
		}

		public void Dispose()
		{
			try { _udp?.Dispose(); } catch { }
		}
	}
}
