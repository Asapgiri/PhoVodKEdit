using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.PhotoEditor {
	public class Editor : PortScreen
	{
		private Bitmap originalImage;
		private Bitmap image;

		private string PictureName = string.Empty;
		private string PictureExt = string.Empty;

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
						try {
							if (effect.Rendered) effect.Apply(image, image.PixelFormat);//PictureExt == "png" ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
						}
						catch { }
					}
				}
			}

			Refresh();
		}

		public override void Refresh() {
			(OwnWindow as EditorWindow).SetCanvas(image);
		}

		public override Window CreateNewContent()
		{
			throw new NotImplementedException();
		}

		public override void SetContent(string contentPath)
		{
			string[] strl = contentPath.Split(Path.PathSeparator);
			PictureName = strl[strl.Length - 1];
			strl = strl[strl.Length - 1].Split('.');
			PictureExt = strl[strl.Length-1].ToLower();

			originalImage = new Bitmap(contentPath);
			var graphic = Graphics.FromImage(new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format32bppArgb));
			graphic.DrawImage(originalImage, new System.Drawing.Point(0, 0));
			image = new Bitmap(originalImage);
			(OwnWindow as EditorWindow).SetCanvas(image);
		}

		public override FrameworkElement GetStatusbarContent() {
			Grid grid = new Grid() {
				Width = 200,
				HorizontalAlignment = HorizontalAlignment.Left,
			};

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25) });
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25) });

			Button btnMinus = new Button() {
				Content = "-",
				Height = 25
			};
			Grid.SetColumn(btnMinus, 0);
			grid.Children.Add(btnMinus);

			Slider slider = new Slider() {
				Minimum = 0,
				Maximum = 500,
				Value = 100,
				Height = 25
			};
			slider.ValueChanged += (sender, e) => {
				(OwnWindow as EditorWindow).SetCanvasSize(slider.Value);
			};
			Grid.SetColumn(slider, 1);
			grid.Children.Add(slider);

			Button btnPlus = new Button() {
				Content = "+",
				Height = 25
			};
			Grid.SetColumn(btnPlus, 2);
			grid.Children.Add(btnPlus);

			return grid;
		}
	}
}
