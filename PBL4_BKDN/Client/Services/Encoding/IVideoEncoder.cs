using System;
using System.Drawing;
using Common.Models;

namespace Client.Services.Encoding
{
	public interface IVideoEncoder
	{
		byte[] Encode(Bitmap bitmap, ScreenControlSettings settings, out EncodeFormat format);
	}
}


