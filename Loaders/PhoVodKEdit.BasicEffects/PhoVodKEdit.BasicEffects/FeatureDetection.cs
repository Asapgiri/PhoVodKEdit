using PhoVodKEdit.BasicEffects.ABS;
using PhoVodKEdit.Port.APS;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PhoVodKEdit.BasicEffects {
    internal class FeatureDetection : MascingPortEffect {


		public FeatureDetection(AppliedSettings _applied) : base(_applied) {
			Offset = 0;
			Factor = 1;
		}

		protected override unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, IntPtr Scan0, IntPtr originalScan0) {
			int nextPixel = pixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;

			Bitmap x, y;
			CopyImage(image, out x, bitmapData);
			CopyImage(image, out y, bitmapData);
			var bmdX = x.LockBits(new Rectangle(0, 0, x.Width, x.Height), ImageLockMode.ReadWrite, pixelFormat);
			var bmdY = y.LockBits(new Rectangle(0, 0, y.Width, y.Height), ImageLockMode.ReadWrite, pixelFormat);

			// grads
			Implement(x, pixelFormat, bmdX, stride, bmdX.Scan0, originalScan0, new double[] {
				-1, 0, 1,
				-1, 0, 1,
				-1, 0, 1
			}, null);
			Implement(y, pixelFormat, bmdY, stride, bmdY.Scan0, originalScan0, new double[] {
				-1, -1, -1,
				 0,  0,  0,
				 1,  1,  1
            }, null);

			int picWidth = stride * image.Height;
			int[] xy = new int[picWidth];
			int[] xpow = new int[picWidth];
			int[] ypow = new int[picWidth];

			byte* px = (byte*)(void*)bmdX.Scan0;
			byte* py = (byte*)(void*)bmdY.Scan0;


			for (int i = 0; i < picWidth; i += nextPixel) {
				xy[i]	  = px[i]	  * py[i];
				xy[i + 1] = px[i + 1] * py[i + 1];
				xy[i + 2] = px[i + 2] * py[i + 2];

				xpow[i]		= px[i]		* px[i];
				xpow[i + 1] = px[i + 1] * px[i + 1];
				xpow[i + 2] = px[i + 2] * px[i + 2];

				ypow[i]		= py[i]		* py[i];
				ypow[i + 1] = py[i + 1] * py[i + 1];
				ypow[i + 2] = py[i + 2] * py[i + 2];
			}

			int cWidth = picWidth - stride  << 1 - nextPixel << 1;
			int[] lxsum = new int[cWidth];
			int[] lysum = new int[cWidth];
			int[] xysum = new int[cWidth];
			int[] filteredPoints = new int[cWidth];

			fixed (int* pxul = xpow, pyul = ypow, pxyul = xy) {
				int* pxml = pxul + stride;
				int* pxbl = pxml + stride;

				int* pyml = pyul + stride;
				int* pybl = pyml + stride;

				int* pxyml = pxyul + stride;
				int* pxybl = pxyml + stride;

				for (int i = 0; i < cWidth; i += nextPixel) {
					lxsum[i] = pxul[i] + pxul[i + nextPixel] + pxul[i + nextPixel << 1]
							 + pxml[i] + pxml[i + nextPixel] + pxml[i + nextPixel << 1]
							 + pxbl[i] + pxbl[i + nextPixel] + pxbl[i + nextPixel << 1];
					lxsum[i + 1] = pxul[i + 1] + pxul[i + 1 + nextPixel] + pxul[i + 1 + nextPixel << 1]
							 + pxml[i + 1] + pxml[i + 1 + nextPixel] + pxml[i + 1 + nextPixel << 1]
							 + pxbl[i + 1] + pxbl[i + 1 + nextPixel] + pxbl[i + 1 + nextPixel << 1];
					lxsum[i + 2] = pxul[i + 2] + pxul[i + 2 + nextPixel] + pxul[i + 2 + nextPixel << 1]
							 + pxml[i + 2] + pxml[i + 2 + nextPixel] + pxml[i + 2 + nextPixel << 1]
							 + pxbl[i + 2] + pxbl[i + 2 + nextPixel] + pxbl[i + 2 + nextPixel << 1];

					lysum[i] = pyul[i] + pyul[i + nextPixel] + pyul[i + nextPixel << 1]
							 + pyml[i] + pyml[i + nextPixel] + pyml[i + nextPixel << 1]
							 + pybl[i] + pybl[i + nextPixel] + pybl[i + nextPixel << 1];
					lysum[i + 1] = pyul[i + 1] + pyul[i + 1 + nextPixel] + pyul[i + 1 + nextPixel << 1]
							 + pyml[i + 1] + pyml[i + 1 + nextPixel] + pyml[i + 1 + nextPixel << 1]
							 + pybl[i + 1] + pybl[i + 1 + nextPixel] + pybl[i + 1 + nextPixel << 1];
					lysum[i + 2] = pyul[i + 2] + pyul[i + 2 + nextPixel] + pyul[i + 2 + nextPixel << 1]
							 + pyml[i + 2] + pyml[i + 2 + nextPixel] + pyml[i + 2 + nextPixel << 1]
							 + pybl[i + 2] + pybl[i + 2 + nextPixel] + pybl[i + 2 + nextPixel << 1];

					xysum[i] = pxyul[i] + pxyul[i + nextPixel] + pxyul[i + nextPixel << 1]
							 + pxyml[i] + pxyml[i + nextPixel] + pxyml[i + nextPixel << 1]
							 + pxybl[i] + pxybl[i + nextPixel] + pxybl[i + nextPixel << 1];
					xysum[i + 1] = pxyul[i + 1] + pxyul[i + 1 + nextPixel] + pxyul[i + 1 + nextPixel << 1]
							 + pxyml[i + 1] + pxyml[i + 1 + nextPixel] + pxyml[i + 1 + nextPixel << 1]
							 + pxybl[i + 1] + pxybl[i + 1 + nextPixel] + pxybl[i + 1 + nextPixel << 1];
					xysum[i + 2] = pxyul[i + 2] + pxyul[i + 2 + nextPixel] + pxyul[i + 2 + nextPixel << 1]
							 + pxyml[i + 2] + pxyml[i + 2 + nextPixel] + pxyml[i + 2 + nextPixel << 1]
							 + pxybl[i + 2] + pxybl[i + 2 + nextPixel] + pxybl[i + 2 + nextPixel << 1];

					filteredPoints[i] = (lxsum[i] * lysum[i] - (xysum[i] << 1)) / (lxsum[i] + lysum[i]);
					filteredPoints[i + 1] = (lxsum[i + 1] * lysum[i + 1] - (xysum[i + 1] << 1)) / (lxsum[i + 1] + lysum[i + 1]);
					filteredPoints[i + 2] = (lxsum[i + 2] * lysum[i + 2] - (xysum[i + 2] << 1)) / (lxsum[i + 2] + lysum[i + 2]);
				}

			}

			x.UnlockBits(bmdX);
			y.UnlockBits(bmdY);

		}

		protected override double[] CalculateMask() {
			return new double[] {
				1, 1, 1,
				1, 1, 1,
				1, 1, 1
			};
        }
    }
}
