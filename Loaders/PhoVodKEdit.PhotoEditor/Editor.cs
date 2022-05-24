using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.PhotoEditor {
	[FileTypes("PNG", "JPEG", "JPG", "BMP")]
	public class Editor : PortScreen
	{
		private Bitmap originalImage;
		private Bitmap image;

		private string PictureName = string.Empty;
		private string PictureExt = string.Empty;

		private Slider sizeSlider;
		private Label sliderPercentage;

		public Editor(AppliedSettings _applied) : base(_applied) {
			OwnWindow = new EditorWindow(this);
			Layers.Add(new Layer("Layer 1"));
			TabName = PictureName;
		}

		protected override void ApplyEffects() {
			if (originalImage == null) return;

			image = new Bitmap(originalImage);
			(OwnWindow as EditorWindow).image = image;
			Graphics g = Graphics.FromImage(image);
			(OwnWindow as EditorWindow).graphics = g;

			foreach (Layer layer in Layers) {
				if (layer.Rendered) {
					foreach (var edit in layer.Edits) {
						switch ((EditingTool)edit.Type) {
							case EditingTool.None:		break;
							case EditingTool.Pen:		g.DrawLine((Pen)edit.Data[0], (System.Drawing.Point)edit.Data[1], (System.Drawing.Point)edit.Data[2]); break;
							case EditingTool.Brush:		break;
							case EditingTool.Line:		g.DrawLine((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
							case EditingTool.Ellipse:	g.DrawEllipse((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
							case EditingTool.Rectangle: g.DrawRectangle((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
							case EditingTool.Eraser:	g.DrawLine((Pen)edit.Data[0], (System.Drawing.Point)edit.Data[1], (System.Drawing.Point)edit.Data[2]); break;
							default: break;
						}
					}
					
					foreach (PortEffect effect in layer.Effects) {
						try {
							if (effect.Rendered) effect.Apply(image, image.PixelFormat);//PictureExt == "png" ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
						}
						catch (Exception ex) { effect.CatchedException = ex; }
					}
				}
			}
		}

		public override void Refresh() {
			(OwnWindow as EditorWindow).SetCanvas(image);
		}

		public override Window CreateNewContent()
		{
			return new CreateNew(Applied, (w, h, pf) => {
				originalImage = new Bitmap(w, h, pf);
				Graphics g = Graphics.FromImage(originalImage);
				g.Clear(Color.White);
				image = new Bitmap(originalImage);

				(OwnWindow as EditorWindow).InitCanvas(image);

				ApplyEffects();
				if (sizeSlider != null && sizeSlider.Value > 100) sizeSlider.Value = 100;
			});
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

			(OwnWindow as EditorWindow).InitCanvas(image);

			ApplyEffects();
			if (sizeSlider != null && sizeSlider.Value > 100) sizeSlider.Value = 100;
		}

		public override FrameworkElement GetStatusbarContent() {
			Grid grid = new Grid() {
				//Width = 200,
				HorizontalAlignment = HorizontalAlignment.Left,
			};

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25) });

			Button btnMinus = new Button() {
				Content = "-",
				Height = 25,
				Padding = new Thickness(0)
			};
			sizeSlider = new Slider() {
				Minimum = 20,
				Maximum = 500,
				Value = 100,
				Height = 25
			};
			sliderPercentage = new Label() {
				Content = "100%",
				FontSize = Applied.Font.Size,
				Foreground = Applied.Colors.Foreground,
				Margin = new Thickness(0),
				Padding = new Thickness(0),
				//SelectedValuePath = string.Format("{0:0}%", sizeSlider.Value),
				//IsEditable = true
			};
			Button btnPlus = new Button() {
				Content = "+",
				Height = 25,
				Padding = new Thickness(0)
			};

			btnMinus.Click += (sender, e) => {
				if (sizeSlider.Value - 10 > sizeSlider.Minimum) {
					sizeSlider.Value -= 10;
				}
				else sizeSlider.Value = sizeSlider.Minimum;
			};
			sizeSlider.ValueChanged += (sender, e) => {
				sliderPercentage.Content = string.Format("{0}%", sizeSlider.Value.ToString("0.00").TrimEnd('0').TrimEnd(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]));
				(OwnWindow as EditorWindow).SetCanvasSize(sizeSlider.Value);
			};
			sliderPercentage.MouseUp += (sender, e) => {
				sizeSlider.Value = 100;
			};
			//sliderPercentage.Items.Add("20%");
			//sliderPercentage.Items.Add("30%");
			//sliderPercentage.Items.Add("40%");
			//sliderPercentage.Items.Add("50%");
			//sliderPercentage.Items.Add("60%");
			//sliderPercentage.Items.Add("75%");
			//sliderPercentage.Items.Add("90%");
			//sliderPercentage.Items.Add("100%");
			//sliderPercentage.Items.Add("150%");
			//sliderPercentage.Items.Add("200%");
			//sliderPercentage.Items.Add("250%");
			//sliderPercentage.Items.Add("300%");
			//sliderPercentage.Items.Add("400%");
			//sliderPercentage.Items.Add("500%");
			btnPlus.Click += (sender, e) => {
				if (sizeSlider.Value + 10 < sizeSlider.Maximum) {
					sizeSlider.Value += 10;
				}
				else sizeSlider.Value = sizeSlider.Maximum;
			};

			Grid.SetColumn(btnMinus, 0);
			Grid.SetColumn(sizeSlider, 1);
			Grid.SetColumn(sliderPercentage, 2);
			Grid.SetColumn(btnPlus, 3);

			grid.Children.Add(btnMinus);
			grid.Children.Add(sizeSlider);
			grid.Children.Add(sliderPercentage);
			grid.Children.Add(btnPlus);

			return grid;
		}

		public override void SaveToFile(string filePath) {
			image.Save(filePath, ImageFormat.Png);
		}
	}
}
