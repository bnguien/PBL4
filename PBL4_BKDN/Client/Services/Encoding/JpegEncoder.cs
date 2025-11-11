using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Common.Models;

namespace Client.Services.Encoding
{
	public sealed class JpegEncoder : IVideoEncoder
	{
		public byte[] Encode(Bitmap bitmap, ScreenControlSettings settings, out EncodeFormat format)
		{
			format = EncodeFormat.Jpeg;
			using var ms = new MemoryStream();
			var minQuality = 70; // avoid blurry output
			var quality = Math.Clamp(Math.Max(settings.Quality, minQuality), 10, 100);
			var encoder = GetEncoder(ImageFormat.Jpeg);
			var encParams = new EncoderParameters(1);
			encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
			bitmap.Save(ms, encoder, encParams);
			return ms.ToArray();
		}

		private ImageCodecInfo GetEncoder(ImageFormat format)
		{
			var codecs = ImageCodecInfo.GetImageDecoders();
			foreach (var c in codecs)
			{
				if (c.FormatID == format.Guid) return c;
			}
			return codecs[0];
		}
	}
}


