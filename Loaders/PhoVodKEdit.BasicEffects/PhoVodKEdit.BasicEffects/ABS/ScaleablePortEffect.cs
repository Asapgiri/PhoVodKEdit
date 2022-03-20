using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port.APS;

/// <summary>
/// Image left unlocked inside Implementation for scalable images. 
/// </summary>
namespace PhoVodKEdit.BasicEffects.ABS {
	public abstract class ScaleablePortEffect : AfterlockPortEffect {
		protected Label ScaleLabel { get; set; }

		protected bool lockInPlace = false;

		protected PixelFormat lastPixelFormat;

		public double Scaling { get; private set; } = 1;
		public double MinScale { get; protected set; } = 0;
		public double MaxScale { get; protected set; } = 2;

		public ScaleablePortEffect(AppliedSettings _applied) : base(_applied) {
		}

		public override FrameworkElement GetView() {
			Grid grid = new Grid() {
				Height = 50
			};
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });

			Label lable0 = new Label {
				Content = "Value: ",
				Foreground = Applied.Colors.ForegroundColor,
				FontSize = (double)Applied.Font.Size,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(5)
			};

			Slider slider = new Slider {
				Minimum = MinScale,
				Maximum = MaxScale,
				Value = Scaling,
				Height = 25,
				IsMoveToPointEnabled = true
			};
			slider.ValueChanged += Slider_ValueChanged;

			ScaleLabel = new Label {
				Content = string.Format("{0:0.00}", Scaling),
				Foreground = Applied.Colors.ForegroundColor,
				FontSize = (double)Applied.Font.Size,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(5)
			};

			Grid.SetColumn(lable0, 0);
			Grid.SetColumn(slider, 1);
			Grid.SetColumn(ScaleLabel, 2);

			grid.Children.Add(lable0);
			grid.Children.Add(slider);
			grid.Children.Add(ScaleLabel);
			return grid;
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			Scaling = (sender as Slider).Value;
			ScaleLabel.Content = string.Format("{0:0.00}", Scaling);

			if (scaledImage != null && !lockInPlace) {
				lockInPlace = true;

				BitmapData bitmapData = scaledImage.LockBits(new Rectangle(0, 0, scaledImage.Width, scaledImage.Height), ImageLockMode.ReadWrite, lastPixelFormat);
				BitmapData originalBitmapData = originalImage.LockBits(new Rectangle(0, 0, originalImage.Width, originalImage.Height), ImageLockMode.ReadWrite, lastPixelFormat);
				int stride = bitmapData.Stride;
				IntPtr Scan0 = bitmapData.Scan0;

				stopwatch.Reset();
				stopwatch.Start();
				Implement(scaledImage, lastPixelFormat, bitmapData, stride, Scan0, originalBitmapData.Scan0);
				stopwatch.Stop();

				originalImage.UnlockBits(originalBitmapData);
				scaledImage.UnlockBits(bitmapData);

				var main = MainWindow as MainWindow;
				main.Screens[main.SelectedScreen].Refresh();

				lockInPlace = false;
			}
		}

		protected override unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData lockedBitmapData, int stride, IntPtr Scan0, IntPtr originalScan0) {
			int nextPixel = pixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;
			byte[] scales;
			lastPixelFormat = pixelFormat;

			FillScales(out scales);

			byte* p = (byte*)(void*)Scan0;
			byte* po = (byte*)(void*)originalScan0;

			int nOffset = stride - image.Width * nextPixel;
			int mWidth = image.Height * image.Width + image.Height * nOffset;

			for (int i = 0; i < mWidth; i++) {
				p[0] = scales[po[0]];
				p[1] = scales[po[1]];
				p[2] = scales[po[2]];
				p += nextPixel;
				po += nextPixel;
			}
		}

		protected abstract void FillScales(out byte[] scales);
	}
}
