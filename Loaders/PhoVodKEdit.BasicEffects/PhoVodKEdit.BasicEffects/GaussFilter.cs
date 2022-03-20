using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using PhoVodKEdit.BasicEffects.ABS;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class GaussFilter : MascingPortEffect {
		public GaussFilter(AppliedSettings _applied) : base(_applied) {
			Factor = 16;
			//NeedScaling = true;
		}

		protected override double[] CalculateMask() {
			return new double[] {
				//.0113, .0838, .0113,
				//.0838, .6193, .0838,
				//.0113, .0838, .0113
				1, 2, 1,
				2, 4, 2,
				1, 2, 1
				//.0625,  .125, .0625,
				// .125,   .25,  .125,
				//.0625,  .125, .0625
			};
		}
	}
}
