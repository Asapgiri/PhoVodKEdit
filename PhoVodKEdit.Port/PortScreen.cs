using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.EXCP;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.Port
{
	/// <summary>
	/// Porting class for the screens.
	/// </summary>
	public abstract class PortScreen : PortingUtility
	{
		#region Properties
		protected List<Layer> Layers { get; set; } = new List<Layer>();

		protected int SelectedLayer { get; set; } = 0;

		protected ContentFilter ContentFilter { get; set; }
		#endregion Properties
		
		#region Ctor
		public PortScreen(AppliedSettings _applied) : base(_applied)
		{
			ContentFilter = new ContentFilter()
			{
				InitDirectory = Directory.GetCurrentDirectory(),
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
				RestoreDirectory = true
			};

			Layers.Add(new Layer("Layer 1"));
		}
		#endregion Ctor


		public bool DefaultBackground { get; private set; } = true;

		public virtual IList<PortEffect> GetEffects() {
			if (SelectedLayer < 0 || SelectedLayer >= Layers.Count) {
				throw new LayerCounterNotSetProperlyException();
			}

			return Layers[SelectedLayer].Effects;
		}

		public virtual void AddEffect(PortEffect effect)
		{
			if (effect == null) {
				throw new EffectIsEmptyException();
			}
			if (SelectedLayer < 0 || SelectedLayer >= Layers.Count) {
				throw new LayerCounterNotSetProperlyException();
			}

			Layers[SelectedLayer].Effects.Add(effect);
		}

		public virtual IList<PortEffect> GetAllEffects(bool onlyActive = true) {
			List<PortEffect> effects = new List<PortEffect>();

			foreach (Layer layer in Layers) {
				foreach (PortEffect effect in layer.Effects) {
					if (effect.Rendered) effects.Add(effect);
				}
			}

			return effects;
		}

		public virtual IList<Layer> GetAllLayers() {
			return Layers;
		}

		public virtual void AddLayer() {
			SelectedLayer = Layers.Count;
			Layers.Add(new Layer($"Layer {Layers.Count + 1}"));
		}

		public virtual void SelectLayer(int index) {
			if (index >= 0 && index < Layers.Count) {
				SelectedLayer = index;
			}
			else if (index < 0) {
				SelectedLayer = 0;
			}
			else {
				SelectedLayer = Layers.Count - 1;
			}
		}

		public virtual void EditLayer(int index, Layer layer) {
			if (index >= 0 && index < Layers.Count) {
				Layers[index] = layer;
			}
		}


		public UserControl GetWindow()
		{
			return OwnWindow;
		}

		public abstract void SetContent(string contentPath);

		public abstract Window CreateNewContent();

	}
}
