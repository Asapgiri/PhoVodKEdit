using System.Windows;
using PhoVodKEdit.BasicEffects.ABS;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects {
	internal class AverageFilter : MascingPortEffect {
		public AverageFilter(AppliedSettings _applied) : base(_applied) {
		}

		public override FrameworkElement GetView() {
			return null;
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
