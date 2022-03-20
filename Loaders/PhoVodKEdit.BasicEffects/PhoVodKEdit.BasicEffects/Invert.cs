using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects
{
	public class Invert : PortEffect {
		public Invert(AppliedSettings _applied) : base(_applied)
		{
		}

		protected override unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, System.IntPtr Scan0)
		{
			int nextPixel = pixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;

			byte* p = (byte*)(void*)Scan0;
			byte* q = p + 1;
			byte* r = q + 1;

			int nOffset = stride - image.Width * nextPixel;
			int mWidth = image.Height * image.Width + image.Height * nOffset;

			//Task[] tasks = new Task[3];
			//tasks[0] = Task.Factory.StartNew(() => InvertProc(p, mWidth, 3));
			//tasks[1] = Task.Factory.StartNew(() => InvertProc(q, mWidth, 3));
			//tasks[2] = Task.Factory.StartNew(() => InvertProc(r, mWidth, 3));

			//Task.WaitAll(tasks);

			for (int y = 0; y < mWidth; ++y) {
				p[0] = (byte)(255 - p[0]);
				q[0] = (byte)(255 - q[0]);
				r[0] = (byte)(255 - r[0]);
				p += nextPixel;
				q += nextPixel;
				r += nextPixel;
			}
		}

		//private unsafe void InvertProc(byte* p, int loopValue, byte nextValue = 1) {
		//	for (int i = 0; i < loopValue; i++) {
		//		p[0] = (byte)(255 - p[0]);
		//		p += nextValue;
		//	}
		//}

		public override FrameworkElement GetView()
		{
			Grid grid = new Grid();

			Label lable = new Label
			{
				Content = "No settings to show",
				Foreground = Applied.Colors.ForegroundColor,
				FontSize = Applied.Font.Size,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			grid.Children.Add(lable);
			return grid;
		}
	}
}
