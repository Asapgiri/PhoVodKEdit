using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class GammaTransformation : PortEffect {
		public GammaTransformation(AppliedSettings _applied) : base(_applied) {
		}

		public override FrameworkElement GetView() {
			throw new NotImplementedException();
		}

		protected override unsafe void Implement(Bitmap image, BitmapData bitmapData, int stride, IntPtr Scan0) {
			throw new NotImplementedException();
		}
	}
}
