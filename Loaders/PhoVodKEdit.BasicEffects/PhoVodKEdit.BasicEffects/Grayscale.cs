using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class Grayscale : PortEffect {
		public Grayscale(AppliedSettings _applied) : base(_applied) {
		}

		protected override unsafe void Implement(Bitmap image, BitmapData bitmapData, int stride, IntPtr Scan0) {
			byte* p = (byte*)(void*)Scan0;
			int nOffset = stride - image.Width * 3;
			int mWidth = image.Height * image.Width + image.Height * nOffset;

			byte red, green, blue;

			for (int y = 0; y < mWidth; ++y) {
				//for (int x = 0; x < image.Width; ++x) {
					blue = p[0];
					green = p[1];
					red = p[2];

					p[0] = p[1] = p[2] = (byte)(.299 * red
						+ .587 * green
						+ .114 * blue);

					p += 3;
				//}
				//p += nOffset;
			}
		}

		public override FrameworkElement GetView() {
			Grid grid = new Grid();

			Label lable = new Label {
				Content = "No settings to show",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			grid.Children.Add(lable);
			return grid;
		}
	}
}
