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

		public PortEffect(AppliedSettings _applied) : base(_applied) {
			stopwatch = new Stopwatch();
		}

		public abstract FrameworkElement GetView();
		public abstract void Apply(Bitmap image, PixelFormat pixelFormat = PixelFormat.Format24bppRgb);

		public long GetProcessTinmeMs()
		{
			return stopwatch.ElapsedMilliseconds;
		}
	}
}
