using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class Histogram : PortEffect {
		double[] histogram = new double[256];

		public double[] Values {
			get { return histogram; }
			private set { histogram = value; }
		}

		public Histogram(AppliedSettings _applied) : base(_applied) {
			ApplyableOnFrames = false;
		}

		public override FrameworkElement GetView() {
			Grid MainGrid = new Grid() {
				Height = 200,
				Margin = new Thickness(0, 3, 0, 0),
			};

			for (int i = 0; i < histogram.Length; i++) {
				MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
				Grid grid = new Grid {
					Background = Applied.Colors.SecondaryColor,
					VerticalAlignment = VerticalAlignment.Bottom,
					Height = (histogram[i] / histogram.Max()) * MainGrid.Height
				};
				Grid.SetColumn(grid, i);
				MainGrid.Children.Add(grid);
			}

			return MainGrid;
		}

		protected override unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, IntPtr Scan0) {
			int nextPixel = pixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;

			byte* p = (byte*)(void*)Scan0;
			histogram = new double[256];

			var main = MainWindow as MainWindow;

			int nOffset = stride - image.Width * nextPixel;
			int mWidth = image.Height * image.Width + image.Height * nOffset;

			if (main.Screens[main.SelectedScreen].HasEffect("Grayscale", Name)) {
				for (int y = 0; y < mWidth; ++y) {
					histogram[p[0]]++;
					p += nextPixel;
				}
			}
			else {
				for (int y = 0; y < mWidth; ++y) {
					histogram[(p[0] >> 2) + (p[1] >> 1) + (p[2] >> 3)]++;
					p += nextPixel;
				}
			}
		}

		internal unsafe void ApplyFromMethod(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, IntPtr Scan0) {
			Implement(image, pixelFormat, bitmapData, stride, Scan0);
		}
	}
}
