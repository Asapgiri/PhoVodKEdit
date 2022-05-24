using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoVodKEdit.Port.Utilities {
	public interface ILayer {
		List<PortEffect> Effects { get; set; }
		List<EditingUtility> Edits { get; set; }
		string Name { get; set; }
		bool Collapsed { get; set; }
		bool Rendered { get; set; }
		bool IsDrawableOn { get; set; }
	}
}
