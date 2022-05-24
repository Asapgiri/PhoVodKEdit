using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.GifEditor {
	/// <summary>
	/// Interaction logic for GifEditorWindow.xaml
	/// </summary>
	public partial class GifEditorWindow : System.Windows.Controls.UserControl {
		private double imageHeight;
		private double imageWidth;
		private double previousPercentage;

		private bool initialized = false;
		private bool paint = false;

		public Bitmap image;
		public Graphics graphics;

		private Pen Pen, Eraser;
		private Pen tempPen;
		private Color actualColor;
		private Color secondaryColor;
		private Color eraserColor;
		private System.Drawing.Point px, py;
		private EditingTool editingTool = EditingTool.None;
		private float penWidth = 5;
		private float eraserWidth = 15;

		int x, y, sX, sY, cX, cY;

		GifEditor editor;
		ILayer SelectedLayer;

		public GifEditorWindow(GifEditor _editor) {
			InitializeComponent();
			SizeEcsetPanel();
			editor = _editor;
			FPSBox.Text = "  " + editor.FPS;
		}

		public void InitCanvas(Bitmap image) {
			//Canvas.Height = image.Height;
			//Canvas.Width = image.Width;
			imageHeight = image.Height;
			imageWidth = image.Width;
			if (initialized) {
				SetCanvasSize(previousPercentage, false);
			}
			else {
				SetCanvasSize();
				initialized = true;
			}
			InitDraw(image);
			Canvas.SetImage(image);
		}

		private void InitDraw(Bitmap _image) {
			this.image = _image;
			if (graphics != null) {
				graphics.Dispose();
			}
			while (editor.RenderImageLocked) { }
			graphics = Graphics.FromImage(image);

			//TODO: Do it from settings [...]
			actualColor = Color.Black;
			secondaryColor = Color.Blue;
			eraserColor = Color.Empty;
			ColorPicker.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(actualColor.A, actualColor.R, actualColor.G, actualColor.B));
			Pen = new Pen(actualColor, penWidth);
			Eraser = new Pen(eraserColor, eraserWidth);
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			Pen.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
			Eraser.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
		}

		public void SetCanvas(Bitmap image) {
			imageHeight = image.Height;
			imageWidth = image.Width;
			Canvas.SetImage(image);
			SetCanvasSize(previousPercentage, false);
		}

		public void SetCanvasSize(double percentage = 100, bool fromInit = true) {
			if (!fromInit && previousPercentage != 0 && previousPercentage != 100 && percentage == previousPercentage) return;
			if (percentage < 20) percentage = 100;
			previousPercentage = percentage;
			percentage *= .01;

			double heightC = imageHeight / ScrollViewer.ActualHeight;
			double widthC = imageWidth / ScrollViewer.ActualWidth;

			if (heightC > widthC) {
				Canvas.Height = percentage * ScrollViewer.ActualHeight - 5;
				Canvas.Width = imageWidth * Canvas.Height / imageHeight;
			}
			else {
				Canvas.Width = percentage * ScrollViewer.ActualWidth - 5;
				Canvas.Height = imageHeight * Canvas.Width / imageWidth;
			}
		}

		public void ReloadLayers() {
			LayerPanel.Children.Clear();
			editor.RenderAll();

			foreach (PhotoLayer player in editor.GetAllLayers()) {
				var btn = new System.Windows.Controls.Button() {
					Content = new System.Windows.Controls.Image() {
						VerticalAlignment = VerticalAlignment.Stretch,
						HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
						Source = editor.GetImageUIFrom(player.RenderedImage)
					},
				};
				btn.Click += (sender, e) => {
					var ownbrd = (sender as System.Windows.Controls.Button).Parent as Border;
					editor.SelectLayer(LayerPanel.Children.IndexOf(ownbrd));
					foreach (Border brd in LayerPanel.Children) {
						brd.BorderThickness = new Thickness(0);
					}
					ownbrd.BorderThickness = new Thickness(5);
					(editor.MainWindow as MainWindow).ApplyEffects();
				};

				LayerPanel.Children.Add(new Border() { 
					Child = btn, 
					BorderBrush = editor.Applied.Colors.Secondary,
					Margin = new Thickness(0, 0, 3, 0),
				});
			}

			var btn_add = new System.Windows.Controls.Button() {
				Content = "Add Frame",
				Height = 40,
				Width = 80,
				VerticalAlignment = VerticalAlignment.Center,
			};
			btn_add.Click += (s, e) => {
				editor.AddLayer();
				ReloadLayers();
				(editor.MainWindow as MainWindow).ApplyEffects();
			};
			LayerPanel.Children.Add(new Border() { Child = btn_add });

			RoutedEventHandler handler = null;
			handler = (sender, e) => {
				ResizeLayers();
				this.Loaded -= handler;
			};
			this.Loaded += handler;
		}

		private void ReloadLayer(int i) {
			if (i >= 0 && i < LayerPanel.Children.Count - 1) {
				foreach (Border brd in LayerPanel.Children) {
					brd.BorderThickness = new Thickness(0);
				}
				(LayerPanel.Children[i] as Border).BorderThickness = new Thickness(5);
				Bitmap ri = editor.RenderOne(i);
				(((LayerPanel.Children[i] as Border).Child as System.Windows.Controls.Button)
					.Content as System.Windows.Controls.Image).Source = editor.GetImageUIFrom(ri);
				(editor.MainWindow as MainWindow).ApplyEffects();
			}
		}

		private void ResizeLayers() {
			double height = (LayerPanel.Children[0] as FrameworkElement).ActualHeight;
			foreach (FrameworkElement cucc in LayerPanel.Children) {
				cucc.Width = height;
			}
		}

		private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			if (editingTool != EditingTool.None) {
				paint = true;
				px = new System.Drawing.Point((int)(Mouse.GetPosition(Canvas).X / Canvas.ActualWidth * image.Width), (int)(Mouse.GetPosition(Canvas).Y / Canvas.ActualHeight * image.Height));
				py = new System.Drawing.Point(px.X + 1, px.Y);
				if (EditingTool.Pen == editingTool) graphics.DrawLine(Pen, px, py);
				if (EditingTool.Eraser == editingTool) graphics.DrawLine(Eraser, px, py);
				Canvas.SetImage(image);

				cX = px.X;
				cY = px.Y;
				SelectedLayer = editor.GetSelectedLayer();
				tempPen = new Pen(actualColor, penWidth);
			}
		}

		private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
			if (paint && !Canvas.ImageLocked) {
				px = new System.Drawing.Point((int)(e.GetPosition(Canvas).X / Canvas.ActualWidth * image.Width), (int)(e.GetPosition(Canvas).Y / Canvas.ActualHeight * image.Height));
				Bitmap img = new Bitmap(image);
				GC.KeepAlive(img);
				Graphics g = Graphics.FromImage(img);
				int icX = cX;
				int icY = cY;
				int isX = sX;
				int isY = sY;
				Task.Factory.StartNew(() => {
					if (EditingTool.Pen != editingTool) {
						x = px.X;
						y = px.Y;
						sX = x - cX;
						sY = y - cY;
						icX = cX;
						icY = cY;
						isX = sX;
						isY = sY;
						if (isX < 0 && isY < 0) {
							icX = x;
							icY = y;
							isX = -isX;
							isY = -isY;
						}
						else if (isX < 0) {
							icX = x;
							isX = -isX;
						}
						else if (isY < 0) {
							icY = y;
							isY = -isY;
						}
					}
					switch (editingTool) {
						case EditingTool.Pen:
							graphics.DrawLine(Pen, px, py);
							g.DrawLine(Pen, px, py);
							SelectedLayer.Edits.Add(new EditingUtility(editingTool, new List<object>() { tempPen, px, py })); break;
						case EditingTool.Eraser:
							graphics.DrawLine(Eraser, px, py);
							g.DrawLine(Eraser, px, py);
							SelectedLayer.Edits.Add(new EditingUtility(editingTool, new List<object>() { Eraser, px, py })); break;
						case EditingTool.Ellipse: g.DrawEllipse(Pen, icX, icY, isX, isY); break;
						case EditingTool.Rectangle: g.DrawRectangle(Pen, icX, icY, isX, isY); break;
						case EditingTool.Line: g.DrawLine(Pen, cX, cY, x, y); break;
					}
					py = px;
				}).ContinueWith(task => Canvas.SetImage(img), TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		private void Canvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			paint = false;

			sX = x - cX;
			sY = y - cY;
			int icX = cX;
			int icY = cY;
			int isX = sX;
			int isY = sY;
			if (isX < 0 && isY < 0) {
				icX = x;
				icY = y;
				isX = -isX;
				isY = -isY;
			}
			else if (isX < 0) {
				icX = x;
				isX = -isX;
			}
			else if (isY < 0) {
				icY = y;
				isY = -isY;
			}

			switch (editingTool) {
				case EditingTool.Ellipse:
					graphics.DrawEllipse(Pen, icX, icY, isX, isY);
					SelectedLayer.Edits.Add(new EditingUtility(editingTool, new List<object>() { tempPen, icX, icY, isX, isY })); break;
				case EditingTool.Rectangle:
					graphics.DrawRectangle(Pen, icX, icY, isX, isY);
					SelectedLayer.Edits.Add(new EditingUtility(editingTool, new List<object>() { tempPen, icX, icY, isX, isY })); break;
				case EditingTool.Line:
					graphics.DrawLine(Pen, cX, cY, x, y);
					SelectedLayer.Edits.Add(new EditingUtility(editingTool, new List<object>() { tempPen, cX, cY, x, y })); break;
			}
			Canvas.SetImage(image);
			ReloadLayer(editor.SelectedLayer);
			GC.Collect();
		}

		private void Border_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
			SizeEcsetPanel();
		}

		private void Move_Click(object sender, RoutedEventArgs e) {
			editingTool = EditingTool.None;
		}

		private void Ecset_Click(object sender, RoutedEventArgs e) {
			editingTool = EditingTool.Pen;
		}

		private void SetFPS_Click(object sender, RoutedEventArgs e) {
			editor.FPS = double.Parse(Regex.Match(FPSBox.Text, @"\d+").Value, NumberFormatInfo.InvariantInfo);
			FPSBox.Text = "  " + editor.FPS;
		}

		private void FPSBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				editor.FPS = double.Parse(Regex.Match(FPSBox.Text, @"\d+").Value, NumberFormatInfo.InvariantInfo);
				FPSBox.Text = "  " + editor.FPS;
			}
		}

		private void Erase_Click(object sender, RoutedEventArgs e) {
			editingTool = EditingTool.Eraser;
		}

		private void Line_Click(object sender, RoutedEventArgs e) {
			editingTool = EditingTool.Line;
		}

		private void Ellipse_Click(object sender, RoutedEventArgs e) {
			editingTool = EditingTool.Ellipse;
		}

		private void Kocka_Click(object sender, RoutedEventArgs e) {
			editingTool = EditingTool.Rectangle;
		}

		private void Szinvalto_Click(object sender, RoutedEventArgs e) {
			ColorDialog colorDlg = new ColorDialog();
			colorDlg.FullOpen = true;
			colorDlg.AllowFullOpen = true;
			colorDlg.AnyColor = true;
			colorDlg.SolidColorOnly = false;
			colorDlg.Color = Pen.Color;
			if (DialogResult.OK == colorDlg.ShowDialog()) {
				actualColor = colorDlg.Color;
				Pen.Color = colorDlg.Color;
				ColorPicker.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(colorDlg.Color.A, colorDlg.Color.R, colorDlg.Color.G, colorDlg.Color.B));
			}
		}

		private void PenSizer_Click(object sender, RoutedEventArgs e) {
			PenSizerDialog psd = new PenSizerDialog(editor.Applied, penWidth);
			psd.ActionOnClose = new Action<float>((f) => {
				penWidth = f;
				Pen.Width = penWidth;
				PenSize.Content = "S = " + Math.Round(penWidth, 2);
			});
			psd.Show();

		}

		private void SizeEcsetPanel() {
			foreach (FrameworkElement cucc in EcsetPanel.Children) {
				cucc.Height = cucc.ActualWidth;
			}
		}


	}
}
