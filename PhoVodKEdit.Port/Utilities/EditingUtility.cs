using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoVodKEdit.Port.Utilities {
    public class EditingUtility {
		public object Type { get; set; }
		public List<object> Data { get; set; }
		public EditingUtility() { }
		public EditingUtility(object type, List<object> data) {
			Type = type;
			Data = data;
		}
    }
}
