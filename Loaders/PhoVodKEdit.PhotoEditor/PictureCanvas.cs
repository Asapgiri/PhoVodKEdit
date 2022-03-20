using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhoVodKEdit.PhotoEditor {
	internal class PictureCanvas : Canvas {
		BitmapImage image;

		public PictureCanvas() {
			RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
		}

		protected override void OnRender(DrawingContext dc) {
			if (image != null) dc.DrawImage(image, new Rect(0, 0, Width, Height));
		}

		public void SetImage(Bitmap _image) {
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
			InvalidateVisual();
		}
	}
}
