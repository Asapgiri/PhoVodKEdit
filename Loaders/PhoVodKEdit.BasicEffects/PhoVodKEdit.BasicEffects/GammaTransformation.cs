using System;
using PhoVodKEdit.BasicEffects.ABS;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class GammaTransformation : ScaleablePortEffect {
		public GammaTransformation(AppliedSettings _applied) : base(_applied) {
			MaxScale = 5;
		}

		protected override void FillScales(out byte[] scales) {
			scales = new byte[256];
			for (int i = 0; i < scales.Length; i++) {
				scales[i] = (byte)Math.Min(255, (int)((255d * Math.Pow(i / 255d, 1d / Scaling)) + .5));
			}
		}
	}
}
