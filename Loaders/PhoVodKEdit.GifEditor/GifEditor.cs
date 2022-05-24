using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.GifEditor {
	[FileTypes("GIF")]
	public class GifEditor : PortScreen {
		//private List<Bitmap> originalImages;
		private Bitmap background;
		private Bitmap renderedImage;
		private int previousImagesCount;
		private double fps = 10;
		private double[] previousImageSuppressions;

		private string PictureName = string.Empty;
		private string PictureExt = string.Empty;

		private Slider sizeSlider;
		private Label sliderPercentage;
		private PortEffect co;

		private System.Timers.Timer timer;

		public bool RenderImageLocked { get; private set; } = false;
		public double FPS { 
			get {
				return fps;
			}
			set {
				if (value < 1) {
					fps = 1;
				}
				else {
					fps = value;
				}
				timer = new System.Timers.Timer(1 / fps * 1000);
			}
		}

		public GifEditor(AppliedSettings _applied) : base(_applied) {
			OwnWindow = new GifEditorWindow(this);
			Layers = new List<ILayer>();
			Layers.Add(new PhotoLayer("Layer 1"));
			TabName = PictureName;
			previousImagesCount = 2;
			previousImageSuppressions = new double[2] { .2, .5 };
			timer = new System.Timers.Timer(1 / fps * 1000);
			timer.Elapsed += Timer_Elapsed;
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			throw new NotImplementedException();
		}

		public override Window CreateNewContent() {
			return new CreateNew(Applied, (w, h, pf) => {
				PictureName = "New Picture 1";
				TabName = PictureName;
				Layers = new List<ILayer>();
				Layers.Add(new PhotoLayer("Layer 1"));
				(Layers[0] as PhotoLayer).Image = new Bitmap(w, h, pf);
				//originalImages = new List<Bitmap>() {
				//	new Bitmap(w, h, pf)
				//};
				background = new Bitmap(w, h, pf);
				Graphics g = Graphics.FromImage((Layers[0] as PhotoLayer).Image);
				g.Clear(Color.White);
				renderedImage = new Bitmap((Layers[0] as PhotoLayer).Image);

				ApplyEffects();
				if (sizeSlider != null && sizeSlider.Value > 100) sizeSlider.Value = 100;

				(OwnWindow as GifEditorWindow).InitCanvas(renderedImage);
				(OwnWindow as GifEditorWindow).ReloadLayers();
			});
		}

		public void RenderAll(bool withBG = false) {
			for (int i = 0; i < Layers.Count; i++) {
				RenderOne(i, withBG);
			}
		}

		public Bitmap RenderOne(int i, bool withBG = false) {
			(Layers[i] as PhotoLayer).RenderedImage = new Bitmap((Layers[i] as PhotoLayer).Image.Width, (Layers[i] as PhotoLayer).Image.Height, PixelFormat.Format32bppArgb);
			Graphics tg = Graphics.FromImage((Layers[i] as PhotoLayer).RenderedImage);
			if (withBG) tg.DrawImage(background, 0, 0);
			foreach (var edit in Layers[i].Edits) {
				switch ((EditingTool)edit.Type) {
					case EditingTool.None: break;
					case EditingTool.Pen: tg.DrawLine((Pen)edit.Data[0], (System.Drawing.Point)edit.Data[1], (System.Drawing.Point)edit.Data[2]); break;
					case EditingTool.Brush: break;
					case EditingTool.Line: tg.DrawLine((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
					case EditingTool.Ellipse: tg.DrawEllipse((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
					case EditingTool.Rectangle: tg.DrawRectangle((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
					case EditingTool.Eraser: tg.DrawLine((Pen)edit.Data[0], (System.Drawing.Point)edit.Data[1], (System.Drawing.Point)edit.Data[2]); break;
					default: break;
				}
			}

			foreach (PortEffect effect in Layers[i].Effects) {
				try {
					if (effect.Rendered) effect.Apply((Layers[i] as PhotoLayer).RenderedImage, (Layers[i] as PhotoLayer).RenderedImage.PixelFormat);//PictureExt == "png" ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
				}
				catch (Exception ex) { effect.CatchedException = ex; }
			}
			return (Layers[i] as PhotoLayer).RenderedImage;
		}

		public override void Refresh() {
			(OwnWindow as GifEditorWindow).SetCanvas(renderedImage);
		}

		public override void SetContent(string contentPath) {
			string[] strl = contentPath.Split(Path.PathSeparator);
			PictureName = strl[strl.Length - 1];
			strl = strl[strl.Length - 1].Split('.');
			PictureExt = strl[strl.Length - 1].ToLower();

			var gifimg = System.Drawing.Image.FromFile(contentPath);
			var dims = new FrameDimension(gifimg.FrameDimensionsList[0]);
			int fc = gifimg.GetFrameCount(dims);
			gifimg.SelectActiveFrame(dims, 0);
			(Layers[0] as PhotoLayer).Image = new Bitmap(gifimg);
			var graphic = Graphics.FromImage(new Bitmap((Layers[0] as PhotoLayer).Image.Width, (Layers[0] as PhotoLayer).Image.Height, PixelFormat.Format32bppArgb));
			graphic.DrawImage((Layers[0] as PhotoLayer).Image, new System.Drawing.Point(0, 0));
			renderedImage = new Bitmap((Layers[0] as PhotoLayer).Image);
			SelectedLayer = 0;

			for (int i = 1; i < fc; i++) {
				gifimg.SelectActiveFrame(dims, i);
				(Layers[i] as PhotoLayer).Image = new Bitmap(gifimg);
				graphic = Graphics.FromImage(new Bitmap((Layers[i] as PhotoLayer).Image.Width, (Layers[i] as PhotoLayer).Image.Height, PixelFormat.Format32bppArgb));
				graphic.DrawImage((Layers[i] as PhotoLayer).Image, new System.Drawing.Point(0, 0));
			}

			ApplyEffects();
			if (sizeSlider != null && sizeSlider.Value > 100) sizeSlider.Value = 100;
		}

		protected override void ApplyEffects() {
			RenderImageLocked = true;
			if ((Layers[0] as PhotoLayer).Image == null) return;
			renderedImage = new Bitmap(background);
			(OwnWindow as GifEditorWindow).image = renderedImage;

			Graphics g = Graphics.FromImage(renderedImage);
			g.Clear(Color.White);
			(OwnWindow as GifEditorWindow).graphics = g;
			int startIndex = SelectedLayer - previousImagesCount;

			if (startIndex < 0) {
				startIndex = 0;
			}
			int prevStartCount = 2 - (SelectedLayer - startIndex);
			if (co == null) {
				co = (MainWindow as MainWindow).RequestPortedTypeGeneration("ChangeOpacity") as PortEffect;
			}

			for (int i = startIndex; i <= SelectedLayer; i++) {
				if (Layers[i].Rendered) {
					co.GetType().GetProperty("TargetOpacity").SetValue(co, prevStartCount < previousImagesCount ? previousImageSuppressions[prevStartCount] : 1.0);
					if (co.GetType().GetProperty("TargetOpacity").GetValue(co) as double? > 0) {
						Bitmap tmpImg = new Bitmap((Layers[i] as PhotoLayer).Image.Width, (Layers[i] as PhotoLayer).Image.Height, PixelFormat.Format32bppArgb);
						Graphics tg = Graphics.FromImage(tmpImg);
						foreach (var edit in Layers[i].Edits) {
							switch ((EditingTool)edit.Type) {
								case EditingTool.None: break;
								case EditingTool.Pen: tg.DrawLine((Pen)edit.Data[0], (System.Drawing.Point)edit.Data[1], (System.Drawing.Point)edit.Data[2]); break;
								case EditingTool.Brush: break;
								case EditingTool.Line: tg.DrawLine((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
								case EditingTool.Ellipse: tg.DrawEllipse((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
								case EditingTool.Rectangle: tg.DrawRectangle((Pen)edit.Data[0], (int)edit.Data[1], (int)edit.Data[2], (int)edit.Data[3], (int)edit.Data[4]); break;
								case EditingTool.Eraser: tg.DrawLine((Pen)edit.Data[0], (System.Drawing.Point)edit.Data[1], (System.Drawing.Point)edit.Data[2]); break;
								default: break;
							}
						}

						foreach (PortEffect effect in Layers[i].Effects) {
							try {
								if (effect.Rendered) effect.Apply(tmpImg, tmpImg.PixelFormat);//PictureExt == "png" ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
							}
							catch (Exception ex) { effect.CatchedException = ex; }
						}
						co.Apply(tmpImg, tmpImg.PixelFormat);

						g.DrawImage(tmpImg, new System.Drawing.Point(0, 0));
					}
					prevStartCount++;
				}
			}
			RenderImageLocked = false;
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
				(OwnWindow as GifEditorWindow).SetCanvasSize(sizeSlider.Value);
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

		public BitmapImage GetImageUIFrom(Bitmap bmp) {
			var ms = new MemoryStream();
			bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

			var frame = new BitmapImage();
			frame.BeginInit();
			frame.StreamSource = ms;
			frame.CacheOption = BitmapCacheOption.OnLoad;
			frame.EndInit();
			frame.Freeze();
			return frame;
		}

		public override void AddLayer() {
			base.AddLayer();
			var prevImg = (Layers[SelectedLayer - 1] as PhotoLayer).Image;
			Layers[SelectedLayer] = new PhotoLayer(Layers[SelectedLayer].Name);
			(Layers[SelectedLayer] as PhotoLayer).Image =
				new Bitmap(prevImg.Width,
				prevImg.Height, PixelFormat.Format32bppArgb);
		}

		public override void SaveToFile(string filePath) {
			RenderAll(true);

			FileStream stream = new FileStream(filePath, FileMode.Create);
			GifBitmapEncoder encoder = new GifBitmapEncoder();

		//using (var gif = AnimatedGif.AnimatedGif.Create(filePath, (int)(1 / fps * 1000))) {
			foreach (PhotoLayer layer in Layers) {
				var ms = new MemoryStream();
				layer.RenderedImage.Save(ms, ImageFormat.Png);
				//var img = System.Drawing.Image.FromStream(ms);
				encoder.Frames.Add(BitmapFrame.Create(ms));
			}
		//}

			encoder.Save(stream);
		}
	}
}
