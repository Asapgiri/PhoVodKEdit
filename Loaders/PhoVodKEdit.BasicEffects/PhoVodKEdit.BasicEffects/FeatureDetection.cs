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
	internal class FeatureDetection : PortEffect {
		public FeatureDetection(AppliedSettings _applied) : base(_applied) {
		}

		public override FrameworkElement GetView() {
			throw new NotImplementedException();
		}

		protected override void Implement(Bitmap image, BitmapData bitmapData, int stride, IntPtr Scan0) {
			throw new NotImplementedException();
		}
	}
}
