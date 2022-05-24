using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.Port
{
	public abstract class PortEffect : PortingUtility
	{
		protected Stopwatch stopwatch;

		public Exception CatchedException { get; set; }
		
		public bool Rendered { get; set; } = true;
		public bool ApplyableOnFrames { get; protected set; } = true;
		protected bool PrelockImage { get; set; } = true;

		public PortEffect(AppliedSettings _applied) : base(_applied) {
			stopwatch = new Stopwatch();
		}

		public abstract FrameworkElement GetView();

		public FrameworkElement GetPublicView() {
			if (CatchedException == null) {
				return GetView();
			}
			else {
				Grid grid = new Grid();
				var view = GetView();

				Label label = new Label() {
					Content = CatchedException.ToString()
				};
				grid.Children.Add(label);

				if (view != null) {
					grid.RowDefinitions.Add(new RowDefinition());
					grid.RowDefinitions.Add(new RowDefinition());
					grid.Children.Add(view);
					Grid.SetRow(label, 1);
				}

				Border border = new Border() {
					BorderBrush = Applied.Colors.Danger,
					BorderThickness = new Thickness(1),
					Child = grid
				};

				return border;
			}
		}

		public void Apply(Bitmap image, PixelFormat pixelFormat) {
			CatchedException = null;
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
