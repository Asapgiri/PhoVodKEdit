using System;
using System.Drawing;
using System.Windows;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.PhotoEditor {
	public class Editor : PortScreen
	{
		private Bitmap originalImage;
		private Bitmap image;

		public Editor(AppliedSettings _applied) : base(_applied)
		{
			OwnWindow = new EditorWindow();
		}

		public override void ApplyEffects() {
			if (originalImage == null) return;

			image = new Bitmap(originalImage);
			foreach (Layer layer in Layers) {
				if (layer.Rendered) {
					foreach (PortEffect effect in layer.Effects) {
						if (effect.Rendered) effect.Apply(image);
					}
				}
			}
			(OwnWindow as EditorWindow).SetCanvas(image);
		}

		public override Window CreateNewContent()
		{
			throw new NotImplementedException();
		}

		public override void SetContent(string contentPath)
		{
			originalImage = new Bitmap(contentPath);
			image = new Bitmap(originalImage);
			(OwnWindow as EditorWindow).SetCanvas(image);
		}
	}
}
