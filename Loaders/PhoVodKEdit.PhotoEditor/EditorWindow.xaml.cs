using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.PhotoEditor {
	/// <summary>
	/// Interaction logic for EditorWindow.xaml
	/// </summary>
	public partial class EditorWindow : System.Windows.Controls.UserControl {
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

		Editor editor;
		ILayer SelectedLayer;

		public EditorWindow(Editor _editor)
		{
			InitializeComponent();
			SizeEcsetPamel();
			editor = _editor;
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

		private void InitDraw(Bitmap image) {
			this.image = image;
			if (graphics != null) {
				graphics.Dispose();
			}
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

		public void SetCanvas(Bitmap image)
		{
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
				case EditingTool.Ellipse:	graphics.DrawEllipse(Pen, icX, icY, isX, isY);
					SelectedLayer.Edits.Add(new EditingUtility(editingTool, new List<object>() { tempPen, icX, icY, isX, isY })); break;
				case EditingTool.Rectangle: graphics.DrawRectangle(Pen, icX, icY, isX, isY);
					SelectedLayer.Edits.Add(new EditingUtility(editingTool, new List<object>() { tempPen, icX, icY, isX, isY })); break;
				case EditingTool.Line:		graphics.DrawLine(Pen, cX, cY, x, y);
					SelectedLayer.Edits.Add(new EditingUtility(editingTool, new List<object>() { tempPen, cX, cY, x, y })); break;
			}
			Canvas.SetImage(image);
			GC.Collect();
		}

		private void Border_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
			SizeEcsetPamel();
		}

		private void Move_Click(object sender, RoutedEventArgs e) {
			editingTool = EditingTool.None;
		}

		private void Ecset_Click(object sender, RoutedEventArgs e) {
			editingTool = EditingTool.Pen;
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

		private void SizeEcsetPamel() {
			foreach(FrameworkElement cucc in EcsetPanel.Children) {
				cucc.Height = cucc.ActualWidth;
			}
		}
	}
}
