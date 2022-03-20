using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using PhoVodKEdit.BasicEffects.ABS;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class SobelEdgeDetection : MascingPortEffect {
		public SobelEdgeDetection(AppliedSettings _applied) : base(_applied) {
			Factor = 1;
			Offset = 0;
		}

		protected override unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData lockedBitmapData, int stride, IntPtr Scan0, IntPtr originalScan0) {
			int nextPixel = pixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;
			lastPixelFormat = pixelFormat;

			int nOffset = stride - image.Width * nextPixel;
			int nWidth = image.Width + nOffset;
			int mWidth = image.Height * image.Width + image.Height * nOffset;

			byte* p = (byte*)(void*)Scan0;
			byte* po = (byte*)(void*)originalScan0;

			double[] mask = CalculateMask();

			int cWidth = mWidth - 2 * nWidth - nOffset - nextPixel;
			p = p + nWidth * nextPixel + nextPixel;
			po = po + nWidth * nextPixel + nextPixel;

			for (int i = 0; i < cWidth; i++) {
				byte* left = po - nextPixel;
				byte* right = po + nextPixel;
				byte* up = po - nWidth * nextPixel;
				byte* down = po + nWidth * nextPixel;

				byte* upleft = up - nextPixel;
				byte* upright = up + nextPixel;
				byte* downleft = down - nextPixel;
				byte* downright = down + nextPixel;

				p[0] = (byte)Math.Atan2((po[0] * mask[4] + left[0] * mask[1] + right[0] * mask[7] + up[0] * mask[5] + down[0] * mask[3] + upleft[0] * mask[2] + upright[0] * mask[8] + downleft[0] * mask[0] + downright[0] * mask[6]) / Factor + Offset,
										(po[0] * mask[4] + left[0] * mask[3] + right[0] * mask[5] + up[0] * mask[1] + down[0] * mask[7] + upleft[0] * mask[0] + upright[0] * mask[2] + downleft[0] * mask[6] + downright[0] * mask[8]) / Factor + Offset);
				p[1] = (byte)Math.Atan2((po[1] * mask[4] + left[1] * mask[1] + right[1] * mask[7] + up[1] * mask[5] + down[1] * mask[3] + upleft[1] * mask[2] + upright[1] * mask[8] + downleft[1] * mask[0] + downright[1] * mask[6]) / Factor + Offset,
										(po[1] * mask[4] + left[1] * mask[3] + right[1] * mask[5] + up[1] * mask[1] + down[1] * mask[7] + upleft[1] * mask[0] + upright[1] * mask[2] + downleft[1] * mask[6] + downright[1] * mask[8]) / Factor + Offset);
				p[2] = (byte)Math.Atan2((po[2] * mask[4] + left[2] * mask[1] + right[2] * mask[7] + up[2] * mask[5] + down[2] * mask[3] + upleft[2] * mask[2] + upright[2] * mask[8] + downleft[2] * mask[0] + downright[2] * mask[6]) / Factor + Offset,
										(po[2] * mask[4] + left[2] * mask[3] + right[2] * mask[5] + up[2] * mask[1] + down[2] * mask[7] + upleft[2] * mask[0] + upright[2] * mask[2] + downleft[2] * mask[6] + downright[2] * mask[8]) / Factor + Offset);

				p += nextPixel;
				po += nextPixel;
			}

			p = (byte*)(void*)Scan0;
			po = (byte*)(void*)originalScan0;
			byte* pd = p + (nWidth * (image.Height - 1) * nextPixel);
			byte* pod = po + (nWidth * (image.Height - 1) * nextPixel);

			// upper and lower corner
			for (int i = 2; i < image.Width; i++) {
				p += nextPixel;

				byte* left = po;
				po += nextPixel;
				byte* right = po + nextPixel;
				byte* down = po + nWidth * nextPixel;

				byte* downleft = down - nextPixel;
				byte* downright = down + nextPixel;

				p[0] = (byte)((double)(po[0] * mask[4] + left[0] * mask[3] + right[0] * mask[5] + po[0] * mask[1] + down[0] * mask[7] + left[0] * mask[0] + right[0] * mask[2] + downleft[0] * mask[6] + downright[0] * mask[8]) / Factor + Offset);
				p[1] = (byte)((double)(po[1] * mask[4] + left[1] * mask[3] + right[1] * mask[5] + po[1] * mask[1] + down[1] * mask[7] + left[1] * mask[0] + right[1] * mask[2] + downleft[1] * mask[6] + downright[1] * mask[8]) / Factor + Offset);
				p[2] = (byte)((double)(po[2] * mask[4] + left[2] * mask[3] + right[2] * mask[5] + po[2] * mask[1] + down[2] * mask[7] + left[2] * mask[0] + right[2] * mask[2] + downleft[2] * mask[6] + downright[2] * mask[8]) / Factor + Offset);

				pd += nextPixel;

				left = pod;
				pod += nextPixel;
				right = pod + nextPixel;
				byte* up = pod - nWidth * nextPixel;

				byte* upleft = up - nextPixel;
				byte* upright = up + nextPixel;

				pd[0] = (byte)((double)(pod[0] * mask[4] + left[0] * mask[3] + right[0] * mask[5] + up[0] * mask[1] + pod[0] * mask[7] + upleft[0] * mask[0] + upright[0] * mask[2] + left[0] * mask[6] + right[0] * mask[8]) / Factor + Offset);
				pd[1] = (byte)((double)(pod[1] * mask[4] + left[1] * mask[3] + right[1] * mask[5] + up[1] * mask[1] + pod[1] * mask[7] + upleft[1] * mask[0] + upright[1] * mask[2] + left[1] * mask[6] + right[1] * mask[8]) / Factor + Offset);
				pd[2] = (byte)((double)(pod[2] * mask[4] + left[2] * mask[3] + right[2] * mask[5] + up[2] * mask[1] + pod[2] * mask[7] + upleft[2] * mask[0] + upright[2] * mask[2] + left[2] * mask[6] + right[2] * mask[8]) / Factor + Offset);
			}

			p = (byte*)(void*)Scan0;
			po = (byte*)(void*)originalScan0;
			byte* pr = p + image.Width * nextPixel - nextPixel;
			byte* por = po + image.Width * nextPixel - nextPixel;

			// left and right corners
			for (int i = 2; i < image.Height; i++) {
				p += nWidth * nextPixel;

				byte* up = po;
				po += nWidth * nextPixel;
				byte* down = po + nWidth * nextPixel;
				byte* right = po + nextPixel;

				byte* downright = down + nextPixel;
				byte* upright = up + nextPixel;

				p[0] = (byte)((double)(po[0] * mask[4] + po[0] * mask[3] + right[0] * mask[5] + up[0] * mask[1] + down[0] * mask[7] + up[0] * mask[0] + upright[0] * mask[2] + down[0] * mask[6] + downright[0] * mask[8]) / Factor + Offset);
				p[1] = (byte)((double)(po[1] * mask[4] + po[1] * mask[3] + right[1] * mask[5] + up[1] * mask[1] + down[1] * mask[7] + up[1] * mask[0] + upright[1] * mask[2] + down[1] * mask[6] + downright[1] * mask[8]) / Factor + Offset);
				p[2] = (byte)((double)(po[2] * mask[4] + po[2] * mask[3] + right[2] * mask[5] + up[2] * mask[1] + down[2] * mask[7] + up[2] * mask[0] + upright[2] * mask[2] + down[2] * mask[6] + downright[2] * mask[8]) / Factor + Offset);

				pr += nWidth * nextPixel;

				up = por;
				por += nWidth * nextPixel;
				down = por + nWidth * nextPixel;
				byte* left = por - nextPixel;

				byte* downleft = down - nextPixel;
				byte* upleft = up - nextPixel;

				pr[0] = (byte)((double)(por[0] * mask[4] + left[0] * mask[3] + por[0] * mask[5] + up[0] * mask[1] + down[0] * mask[7] + upleft[0] * mask[0] + up[0] * mask[2] + downleft[0] * mask[6] + down[0] * mask[8]) / Factor + Offset);
				pr[1] = (byte)((double)(por[1] * mask[4] + left[1] * mask[3] + por[1] * mask[5] + up[1] * mask[1] + down[1] * mask[7] + upleft[1] * mask[0] + up[1] * mask[2] + downleft[1] * mask[6] + down[1] * mask[8]) / Factor + Offset);
				pr[2] = (byte)((double)(por[2] * mask[4] + left[2] * mask[3] + por[2] * mask[5] + up[2] * mask[1] + down[2] * mask[7] + upleft[2] * mask[0] + up[2] * mask[2] + downleft[2] * mask[6] + down[2] * mask[8]) / Factor + Offset);
			}
		}

		protected override double[] CalculateMask() {
			return new double[] {
				 3,  10,  3,
				 0,   0,  0,
				-3, -10, -3
			};
		}
	}
}
