using System;
using PhoVodKEdit.BasicEffects.ABS;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class LogaritmicTransformation : ScaleablePortEffect {
		public LogaritmicTransformation(AppliedSettings _applied) : base(_applied) {
			MaxScale = 100;
		}

		protected override void FillScales(out byte[] scales) {
			scales = new byte[256];
			for (int i = 0; i < scales.Length; i++) {
				scales[i] = (byte)Math.Min(255, (int)(Scaling * Math.Log10(1d + i)));
			}
		}
	}
}
