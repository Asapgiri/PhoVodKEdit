using PhoVodKEdit.BasicEffects.ABS;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class LaplaceEdgeDetection : MascingPortEffect {
		public LaplaceEdgeDetection(AppliedSettings _applied) : base(_applied) {
			//Factor = 1;
		}

		protected override double[] CalculateMask() {
			return new double[] {
				 0, -1,  0,
				-1,  4, -1,
				 0, -1,  0
				//-1, -1, -1,
				//-1,  8, -1,
				//-1, -1, -1
			};
		}
	}
}
