using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhoVodKEdit.PhotoEditor {
	public class PictureCanvas : Canvas {
		BitmapImage image;
		public bool ImageLocked { get; set; } = false;

		public PictureCanvas() {
			RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
		}

		protected override void OnRender(DrawingContext dc) {
			if (image != null) dc.DrawImage(image, new Rect(0, 0, Width, Height));
		}

		public void SetImage(Bitmap _image) {
			Task.Factory.StartNew(() => {
				if (ImageLocked) return;

				ImageLocked = true;
				using (var memory = new MemoryStream()) {
					_image.Save(memory, ImageFormat.Png);
					memory.Position = 0;

					var bitmapImage = new BitmapImage();
					bitmapImage.BeginInit();
					bitmapImage.StreamSource = memory;
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.EndInit();
					bitmapImage.Freeze();

					image = bitmapImage;
				}
				ImageLocked = false;
			}).ContinueWith(task => {
				InvalidateVisual();
				GC.Collect();
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public void SetImage(BitmapImage _image) {
			image = _image;
		}
	}
}
