using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.Port
{
	public abstract class PortEffect : PortingUtility
	{
		protected Stopwatch stopwatch;

		public bool Rendered { get; set; } = true;
		public bool ApplyableOnFrames { get; protected set; } = true;
		protected bool PrelockImage { get; set; } = true;

		public PortEffect(AppliedSettings _applied) : base(_applied) {
			stopwatch = new Stopwatch();
		}

		public abstract FrameworkElement GetView();
		public void Apply(Bitmap image, PixelFormat pixelFormat) {
			stopwatch.Reset();
			stopwatch.Start();

			if (PrelockImage) {
				BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, pixelFormat);
				Implement(image, pixelFormat, bitmapData, bitmapData.Stride, bitmapData.Scan0);
				image.UnlockBits(bitmapData);
			}
			else Implement(image, pixelFormat, null, 0, System.IntPtr.Zero);

			stopwatch.Stop();
		}

		protected abstract unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, System.IntPtr Scan0);

		public double GetProcessTinmeMs()
		{
			return stopwatch.Elapsed.TotalMilliseconds;
		}
	}
}
