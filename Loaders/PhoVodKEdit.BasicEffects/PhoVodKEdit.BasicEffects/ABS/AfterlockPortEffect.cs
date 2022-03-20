using System;
using System.Drawing;
using System.Drawing.Imaging;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects.ABS {
	public abstract class AfterlockPortEffect : PortEffect {
		protected Bitmap originalImage;
		protected Bitmap scaledImage;

		public AfterlockPortEffect(AppliedSettings _applied) : base(_applied) {
			PrelockImage = false;
		}

		protected override void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, IntPtr Scan0) {
			originalImage = new Bitmap(image);
			scaledImage = image;
			BitmapData lockedBitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, pixelFormat);
			BitmapData originalBitmapData = originalImage.LockBits(new Rectangle(0, 0, originalImage.Width, originalImage.Height), ImageLockMode.ReadWrite, pixelFormat);
			Implement(image, pixelFormat, lockedBitmapData, lockedBitmapData.Stride, lockedBitmapData.Scan0, originalBitmapData.Scan0);
			originalImage.UnlockBits(originalBitmapData);
			image.UnlockBits(lockedBitmapData);
		}

		protected abstract unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData lockedBitmapData, int stride, IntPtr Scan0, IntPtr originalScan0);
	}
}
