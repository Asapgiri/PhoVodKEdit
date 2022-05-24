using System.Collections.Generic;
using System.Drawing;

namespace PhoVodKEdit.Port.Utilities {
	public class Layer: ILayer {
		public List<PortEffect> Effects { get; set; }
		public List<EditingUtility> Edits { get; set; }

		public string Name { get; set; }
		public bool Collapsed { get; set; } = false;
		public bool Rendered { get; set; } = true;
		public bool IsDrawableOn { get; set; } = true;

		public Layer(string name) { 
			Name = name;
			Effects = new List<PortEffect>();
			Edits = new List<EditingUtility>();
		}
	}
}
