﻿using System.Collections.Generic;
using System.Diagnostics;
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
		protected Stopwatch stopwatch;

		#region Properties
		protected List<Layer> Layers { get; set; } = new List<Layer>();

		protected int SelectedLayer { get; set; } = 0;

		public ContentFilter ContentFilter { get; set; }
		#endregion Properties
		
		#region Ctor
		public PortScreen(AppliedSettings _applied) : base(_applied)
		{
			stopwatch = new Stopwatch();

			ContentFilter = new ContentFilter()
			{
				InitDirectory = string.Empty,
				Filter = "Pictures |*.jpg;*.png|All files |*.*",
				RestoreDirectory = false
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

		public virtual void AddEffect(PortEffect effect, int position = -1)
		{
			if (effect == null) {
				throw new EffectIsEmptyException();
			}
			if (SelectedLayer < 0 || SelectedLayer >= Layers.Count) {
				throw new LayerCounterNotSetProperlyException();
			}

			if (position >= 0) Layers[SelectedLayer].Effects.Insert(position, effect);
			else Layers[SelectedLayer].Effects.Add(effect);
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

		public virtual void RemoveEffect(int index) {
			Layers[SelectedLayer].Effects.RemoveAt(index);
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

		public bool HasEffect(string name, string before = null) {
			if (before != null) {
				foreach (PortEffect effect in GetAllEffects()) {
					if (effect.Name == before) {
						return false;
					}
					else if (effect.Name == name) {
						effect.Rendered = true;
						return true;
					}
				}
			}
			else {
				foreach (PortEffect effect in GetAllEffects()) {
					if (effect.Name == name) {
						effect.Rendered = true;
						return true;
					}
				}
			}

			return false;
		}

		public PortEffect GetEffect(string name) {
			foreach (PortEffect effect in GetAllEffects()) {
				if (effect.Name == name) {
					return effect;
				}
			}

			return null;
		}

		public UserControl GetWindow()
		{
			return OwnWindow;
		}

		public abstract void SetContent(string contentPath);

		public abstract Window CreateNewContent();

		public void Apply() {
			stopwatch.Reset();
			stopwatch.Start();

			ApplyEffects();

			stopwatch.Stop();
		}

		protected abstract void ApplyEffects();

		public abstract void Refresh();

		public virtual FrameworkElement GetStatusbarContent() { return null; }

		public double GetProcessTinmeMs() {
			return stopwatch.Elapsed.TotalMilliseconds;
		}
	}
}
