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

		public PortEffect(AppliedSettings _applied) : base(_applied) {
			stopwatch = new Stopwatch();
		}

		public abstract FrameworkElement GetView();
		public void Apply(Bitmap image, PixelFormat pixelFormat = PixelFormat.Format24bppRgb) {
			BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, pixelFormat);
			int stride = bitmapData.Stride;
			System.IntPtr Scan0 = bitmapData.Scan0;

			stopwatch.Reset();
			stopwatch.Start();

			Implement(image, bitmapData, bitmapData.Stride, bitmapData.Scan0);

			stopwatch.Stop();

			image.UnlockBits(bitmapData);
		}

		protected abstract unsafe void Implement(Bitmap image, BitmapData bitmapData, int stride, System.IntPtr Scan0);

		public double GetProcessTinmeMs()
		{
			return stopwatch.Elapsed.TotalMilliseconds;
		}
	}
}
