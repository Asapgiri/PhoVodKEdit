using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class HistogramCorrection : PortEffect {
		Histogram Histogram { get; set; }
		double[] correctedHistogram = new double[256];
		double[] T = new double[256];

		public HistogramCorrection(AppliedSettings _applied) : base(_applied) {
		}

		public override FrameworkElement GetView() {
			Grid MainGrid = new Grid() {
				Height = 200,
				Margin = new Thickness(0, 3, 0, 0),
			};

			for (int i = 0; i < correctedHistogram.Length; i++) {
				MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
				Grid grid = new Grid {
					Background = Applied.Colors.Secondary,
					VerticalAlignment = VerticalAlignment.Bottom,
					Height = (correctedHistogram[i] / correctedHistogram.Max()) * MainGrid.Height
				};
				Grid.SetColumn(grid, i);
				MainGrid.Children.Add(grid);
			}

			return MainGrid;
		}

		protected override unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, IntPtr Scan0) {
			var main = MainWindow as MainWindow;
			if (main.Screens[main.SelectedScreen].HasEffect("Histogram", Name)) {
				Histogram = main.Screens[main.SelectedScreen].GetEffect("Histogram") as Histogram;
			}
			else {
				Histogram = new Histogram(null) { MainWindow = MainWindow };
				Histogram.ApplyFromMethod(image, pixelFormat, bitmapData, stride, Scan0);
			}

			if (!main.Screens[main.SelectedScreen].HasEffect("Grayscale", Name)) {
				(new Grayscale(null)).ApplyFromMethod(image, pixelFormat, bitmapData, stride, Scan0);
			}

			T = new double[Histogram.Values.Length];
			double[] pA = new double[Histogram.Values.Length];

			int nextPixel = pixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;

			byte* p = (byte*)(void*)Scan0;
			int nOffset = stride - image.Width * nextPixel;
			int mWidth = image.Height * image.Width + image.Height * nOffset;
			for (int i = 0; i < Histogram.Values.Length; i++) {
				pA[i] = Histogram.Values[i] / mWidth;
			}

			for (int i = 0; i < Histogram.Values.Length; i++) {
				for (int j = 0; j < i; j++) {
					T[i] += pA[j];
				}

				T[i] = T[i] * 255;
			}

			correctedHistogram = new double[Histogram.Values.Length];
			for (int y = 0; y < mWidth; ++y) {
				p[0] = p[1] = p[2] = (byte)Math.Round(T[p[0]]);
				correctedHistogram[p[0]]++;
				p += nextPixel;
			}
		}
	}
}
