using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Common.Models;

namespace Client.Services.Capture
{
	public sealed class GdiCapturer : IScreenCapturer
	{
		public Bitmap? Capture(ScreenControlSettings settings)
		{
			try
			{
				var screenBounds = GetPrimaryScreenBounds();
				// Capture full resolution first to avoid blurry downscale by CopyFromScreen
				using var full = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format24bppRgb);
				using (var g = Graphics.FromImage(full))
				{
					g.CopyFromScreen(screenBounds.Left, screenBounds.Top, 0, 0, screenBounds.Size);
				}
				if (settings.Resolution <= 1)
				{
					return new Bitmap(full);
				}
				var targetW = Math.Max(1, screenBounds.Width / settings.Resolution);
				var targetH = Math.Max(1, screenBounds.Height / settings.Resolution);
				var resized = new Bitmap(targetW, targetH, PixelFormat.Format24bppRgb);
				using (var rg = Graphics.FromImage(resized))
				{
					rg.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
					rg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					rg.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
					rg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					rg.DrawImage(full, new Rectangle(0, 0, targetW, targetH));
				}
				return resized;
			}
			catch
			{
				return null;
			}
		}

		private Rectangle GetPrimaryScreenBounds()
		{
			return System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
		}
	}
}
