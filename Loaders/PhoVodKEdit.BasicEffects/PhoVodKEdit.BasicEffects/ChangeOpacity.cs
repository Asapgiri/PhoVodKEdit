using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	public class ChangeOpacity : PortEffect {
		private double targetOpacity = 1;

		public double TargetOpacity {
			get { return targetOpacity; }
			set {
				if (value >= 0 && value <= 1)
					targetOpacity = value;
				else
					throw new ArgumentOutOfRangeException();
			}
		}

		public ChangeOpacity(AppliedSettings _applied) : base(_applied) {
			Rendered = false;
		}

		public override FrameworkElement GetView() {
			return null;
		}

		protected override unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, IntPtr Scan0) {
			int nextPixel = 4;
			byte* p = (byte*)(void*)Scan0 + 3;
			var lt = new byte[256];

			for (int i = 0; i < lt.Length; i++) {
				lt[i] = (byte)(i * targetOpacity);
			}

			int nOffset = stride - image.Width * nextPixel;
			int mWidth = image.Height * image.Width + image.Height * nOffset;

			for (int y = 0; y < mWidth; ++y) {
				p[0] = lt[p[0]];
				p += nextPixel;
			}
		}
	}
}
