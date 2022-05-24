using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.GifEditor {
	public class PhotoLayer : ILayer {
		private int actualLayer;
		public List<List<PortEffect>> ListLayerEffects { get; set; }
		public List<List<EditingUtility>> ListLayerEdits { get; set; }
		public Bitmap Image { get; set; }
		public Bitmap RenderedImage { get; set; }

		public int ActualLayer {
			get { return actualLayer; }
			set {
				if (value >= 0 && value < ListLayerEffects.Count) {
					actualLayer = value;
				}
				else {
					throw new ArgumentOutOfRangeException(nameof(value));
				}
			}
		}
		public List<PortEffect> Effects {
			get { return ListLayerEffects[actualLayer]; }
			set { ListLayerEffects[actualLayer] = value; }
		}
		public List<EditingUtility> Edits {
			get { return ListLayerEdits[actualLayer]; }
			set { ListLayerEdits[actualLayer] = value; }
		}

		public string Name { get; set; }
		public bool Collapsed { get; set; } = false;
		public bool Rendered { get; set; } = true;
		public bool IsDrawableOn { get; set; } = true;

		public PhotoLayer(string name) {
			Name = name;
			ListLayerEffects = new List<List<PortEffect>>() {
				new List<PortEffect>()
			};
			ListLayerEdits = new List<List<EditingUtility>>() {
				new List<EditingUtility>()
			};
		}
	}
}
