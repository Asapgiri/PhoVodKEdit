using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects
{
	public class Invert : PortEffect
	{
		public Invert(AppliedSettings _applied) : base(_applied)
		{
		}

		protected override unsafe void Implement(Bitmap image, BitmapData bitmapData, int stride, System.IntPtr Scan0)
		{
			byte* p = (byte*)(void*)Scan0;
			byte* q = p + 1;
			byte* r = q + 1;

			int nOffset = stride - image.Width * 3;
			int mWidth = image.Height * image.Width + image.Height * nOffset;
			int i = 0;
			for (int y = 0; y < mWidth; ++y)
			{
				//for (int x = 0; x < nWidth; ++x)
				//{
					p[0] = (byte)(255 - p[0]);
					q[0] = (byte)(255 - q[0]);
					r[0] = (byte)(255 - r[0]);
					p += 3;
					q += 3;
					r += 3;
				//if (i >= image.Width) {
				//	i = 0;
				//	p += nOffset;
				//	q += nOffset;
				//	r += nOffset;
				//}
				//}
				//p += nOffset;
			}
		}

		public override FrameworkElement GetView()
		{
			Grid grid = new Grid();

			Label lable = new Label
			{
				Content = "No settings to show",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			grid.Children.Add(lable);
			return grid;
		}
	}
}
