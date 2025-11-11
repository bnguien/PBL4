using System.Drawing;
using Common.Models;

namespace Client.Services.Capture
{
	public interface IScreenCapturer
	{
		Bitmap? Capture(ScreenControlSettings settings);
	}
}
