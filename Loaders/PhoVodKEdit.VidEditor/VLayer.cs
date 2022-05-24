using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PhoVodKEdit.VidEditor {
	internal class VLayer {
		public FrameworkElement Control { get; set; }
		public FrameworkElement Panel { get; set; }
		public LayerType LayerType { get; private set; }

		public VLayer(LayerType _lt) {
			LayerType = _lt;
		}
	}
}
