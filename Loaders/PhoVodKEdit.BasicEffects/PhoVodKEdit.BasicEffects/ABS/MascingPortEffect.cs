using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects.ABS {
	internal abstract class MascingPortEffect : ScaleablePortEffect {
		protected bool NeedScaling { get; set; } = false;
		public int Factor { get; set; } = 9;
		public int Offset { get; set; } = 0;
		public int ThreadCountMultiplyer { get; set; } = 3;
		public bool IsThreadCountMultiplyerFixed { get; set; } = false;

		protected MascingPortEffect(AppliedSettings _applied) : base(_applied) {
		}

		public override FrameworkElement GetView() {
			if (NeedScaling) return base.GetView();
			else return null;
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

			#region Middle
			if (!IsThreadCountMultiplyerFixed) {
				ThreadCountMultiplyer = (int)Math.Sqrt(mWidth / 518400); // number assures that there is 4 threads on 1080p
			}

			int cWidth = (mWidth - 2 * nWidth + nextPixel) >> ThreadCountMultiplyer;
			p = p + nWidth * nextPixel + nextPixel;
			po = po + nWidth * nextPixel + nextPixel;
			if (ThreadCountMultiplyer == 0) {
				CalculateMiddleParts(p, po, nextPixel, mask, nWidth, cWidth);
			}
			else {
				Task[] tasks = new Task[(int)Math.Pow(2, ThreadCountMultiplyer)];
				{
					byte* x = p;
					byte* y = po;
					tasks[0] = new Task(() => CalculateMiddleParts(x, y, nextPixel, mask, nWidth, cWidth));
				}

				for (int i = 1; i < tasks.Length; i++) {
					p = p + cWidth * nextPixel;
					po = po + cWidth * nextPixel;
					{
						byte* x = p;
						byte* y = po;
						tasks[i] = new Task(() => CalculateMiddleParts(x, y, nextPixel, mask, nWidth, cWidth));
					}
				}

				for (int i = 0; i < tasks.Length; i++) {
					tasks[i].Start();
				}

				Task.WaitAll(tasks);
			}
			#endregion Middle

			#region Horizontal Scales
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
			#endregion Horizontal Scales

			#region Vertical Scales
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
			#endregion Vertical Scales
			
			#region Corners
			{
				#region Top Left
				p = (byte*)(void*)Scan0;
				po = (byte*)(void*)originalScan0;

				byte* left = po;
				byte* right = po + nextPixel;
				byte* up = po;
				byte* down = po + nWidth * nextPixel;

				byte* upleft = po;
				byte* upright = right;
				byte* downleft = down;
				byte* downright = down + nextPixel;

				p[0] = (byte)((double)(po[0] * mask[4] + left[0] * mask[3] + right[0] * mask[5] + up[0] * mask[1] + down[0] * mask[7] + upleft[0] * mask[0] + upright[0] * mask[2] + downleft[0] * mask[6] + downright[0] * mask[8]) / Factor + Offset);
				p[1] = (byte)((double)(po[1] * mask[4] + left[1] * mask[3] + right[1] * mask[5] + up[1] * mask[1] + down[1] * mask[7] + upleft[1] * mask[0] + upright[1] * mask[2] + downleft[1] * mask[6] + downright[1] * mask[8]) / Factor + Offset);
				p[2] = (byte)((double)(po[2] * mask[4] + left[2] * mask[3] + right[2] * mask[5] + up[2] * mask[1] + down[2] * mask[7] + upleft[2] * mask[0] + upright[2] * mask[2] + downleft[2] * mask[6] + downright[2] * mask[8]) / Factor + Offset);
				#endregion Top Left

				#region Top Right
				p = p + image.Width * nextPixel - nextPixel;
				po = po + image.Width * nextPixel - nextPixel;

				left = po - nextPixel;
				right = po;
				up = po;
				down = po + nWidth * nextPixel;

				upleft = left;
				upright = po;
				downleft = down - nextPixel;
				downright = down;

				p[0] = (byte)((double)(po[0] * mask[4] + left[0] * mask[3] + right[0] * mask[5] + up[0] * mask[1] + down[0] * mask[7] + upleft[0] * mask[0] + upright[0] * mask[2] + downleft[0] * mask[6] + downright[0] * mask[8]) / Factor + Offset);
				p[1] = (byte)((double)(po[1] * mask[4] + left[1] * mask[3] + right[1] * mask[5] + up[1] * mask[1] + down[1] * mask[7] + upleft[1] * mask[0] + upright[1] * mask[2] + downleft[1] * mask[6] + downright[1] * mask[8]) / Factor + Offset);
				p[2] = (byte)((double)(po[2] * mask[4] + left[2] * mask[3] + right[2] * mask[5] + up[2] * mask[1] + down[2] * mask[7] + upleft[2] * mask[0] + upright[2] * mask[2] + downleft[2] * mask[6] + downright[2] * mask[8]) / Factor + Offset);
				#endregion Top Right

				#region Bottom Left
				p = (byte*)(void*)Scan0 + (nWidth * (image.Height - 1) * nextPixel);
				po = (byte*)(void*)originalScan0 + (nWidth * (image.Height - 1) * nextPixel);

				left = po;
				right = po + nextPixel;
				up = po - nWidth * nextPixel;
				down = po;

				upleft = up - nextPixel;
				upright = up;
				downleft = po;
				downright = right;

				p[0] = (byte)((double)(po[0] * mask[4] + left[0] * mask[3] + right[0] * mask[5] + up[0] * mask[1] + down[0] * mask[7] + upleft[0] * mask[0] + upright[0] * mask[2] + downleft[0] * mask[6] + downright[0] * mask[8]) / Factor + Offset);
				p[1] = (byte)((double)(po[1] * mask[4] + left[1] * mask[3] + right[1] * mask[5] + up[1] * mask[1] + down[1] * mask[7] + upleft[1] * mask[0] + upright[1] * mask[2] + downleft[1] * mask[6] + downright[1] * mask[8]) / Factor + Offset);
				p[2] = (byte)((double)(po[2] * mask[4] + left[2] * mask[3] + right[2] * mask[5] + up[2] * mask[1] + down[2] * mask[7] + upleft[2] * mask[0] + upright[2] * mask[2] + downleft[2] * mask[6] + downright[2] * mask[8]) / Factor + Offset);
				#endregion Bottom Left

				#region Bottom Right
				p = p + image.Width * nextPixel - nextPixel;
				po = po + image.Width * nextPixel - nextPixel;

				left = po - nextPixel;
				right = po;
				up = po - nWidth * nextPixel;
				down = po;

				upleft = up - nextPixel;
				upright = up;
				downleft = left;
				downright = po;

				p[0] = (byte)((double)(po[0] * mask[4] + left[0] * mask[3] + right[0] * mask[5] + up[0] * mask[1] + down[0] * mask[7] + upleft[0] * mask[0] + upright[0] * mask[2] + downleft[0] * mask[6] + downright[0] * mask[8]) / Factor + Offset);
				p[1] = (byte)((double)(po[1] * mask[4] + left[1] * mask[3] + right[1] * mask[5] + up[1] * mask[1] + down[1] * mask[7] + upleft[1] * mask[0] + upright[1] * mask[2] + downleft[1] * mask[6] + downright[1] * mask[8]) / Factor + Offset);
				p[2] = (byte)((double)(po[2] * mask[4] + left[2] * mask[3] + right[2] * mask[5] + up[2] * mask[1] + down[2] * mask[7] + upleft[2] * mask[0] + upright[2] * mask[2] + downleft[2] * mask[6] + downright[2] * mask[8]) / Factor + Offset);
				#endregion Bottom Right
			}
			#endregion Corners
		}

		[HandleProcessCorruptedStateExceptions]
		private unsafe void CalculateMiddleParts(byte* p, byte* po, int nextPixel, double[] mask, int nWidth, int cWidth) {
			try {
				for (int i = 0; i < cWidth; i++) {
					//int topLeft = i * nextPixel;
					//int top = topLeft + nextPixel;
					//int topRight = top + nextPixel;

					//int left = topLeft + stride;
					//int pixel = left + nextPixel;
					//int right = pixel + nextPixel;

					//int bottomLeft = left + stride;
					//int bottom = bottomLeft + nextPixel;
					//int bottomRight = bottom + nextPixel;



					//p[pixel] = (byte)((double)(po[topLeft] * mask[0] + po[top] * mask[1] + po[topRight] * mask[2]
					//						 + po[left] * mask[3] + po[pixel] * mask[4] + po[right] * mask[5]
					//						 + po[bottomLeft] * mask[6] + po[bottom] * mask[7] + po[bottomRight] * mask[8])
					//						 / Factor + Offset);

					//p[pixel + 1] = (byte)((double)(po[topLeft + 1] * mask[0] + po[top + 1] * mask[1] + po[topRight + 1] * mask[2]
					//							 + po[left + 1] * mask[3] + po[pixel + 1] * mask[4] + po[right + 1] * mask[5]
					//							 + po[bottomLeft + 1] * mask[6] + po[bottom + 1] * mask[7] + po[bottomRight + 1] * mask[8])
					//							 / Factor + Offset);

					//p[pixel + 2] = (byte)((double)(po[topLeft + 2] * mask[0] + po[top + 2] * mask[1] + po[topRight + 2] * mask[2]
					//							 + po[left + 2] * mask[3] + po[pixel + 2] * mask[4] + po[right + 2] * mask[5]
					//							 + po[bottomLeft + 2] * mask[6] + po[bottom + 2] * mask[7] + po[bottomRight + 2] * mask[8])
					//							 / Factor + Offset);

					byte* left = po - nextPixel;
					byte* right = po + nextPixel;
					byte* up = po - nWidth * nextPixel;
					byte* down = po + nWidth * nextPixel;

					byte* upleft = up - nextPixel;
					byte* upright = up + nextPixel;
					byte* downleft = down - nextPixel;
					byte* downright = down + nextPixel;

					p[0] = (byte)((double)(po[0] * mask[4] + left[0] * mask[3] + right[0] * mask[5] + up[0] * mask[1] + down[0] * mask[7] + upleft[0] * mask[0] + upright[0] * mask[2] + downleft[0] * mask[6] + downright[0] * mask[8]) / Factor + Offset);
					p[1] = (byte)((double)(po[1] * mask[4] + left[1] * mask[3] + right[1] * mask[5] + up[1] * mask[1] + down[1] * mask[7] + upleft[1] * mask[0] + upright[1] * mask[2] + downleft[1] * mask[6] + downright[1] * mask[8]) / Factor + Offset);
					p[2] = (byte)((double)(po[2] * mask[4] + left[2] * mask[3] + right[2] * mask[5] + up[2] * mask[1] + down[2] * mask[7] + upleft[2] * mask[0] + upright[2] * mask[2] + downleft[2] * mask[6] + downright[2] * mask[8]) / Factor + Offset);

					p += nextPixel;
					po += nextPixel;
				}
			}
			catch (Exception ex) { }
		}

		protected abstract double[] CalculateMask();

		protected override void FillScales(out byte[] scales) {
			throw new System.NotImplementedException();
		}
	}
}
