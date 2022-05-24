using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.PhotoEditor {
	/// <summary>
	/// Interaction logic for PenSizerDialog.xaml
	/// </summary>
	public partial class PenSizerDialog : Window {
		float pensize;
		public AppliedSettings Applied { get; set; }
		public Action<float> ActionOnClose;
		Pen pen;

		private void Initialize(AppliedSettings _applied, float _pensize) {
			DataContext = this;
			pensize = _pensize;
			Applied = _applied;
			pen = new Pen(Color.White, _pensize);
			pen.DashStyle = DashStyle.Dot;
			InitializeComponent();
			SetPreview(pensize);
		}
		public PenSizerDialog(AppliedSettings _applied, float _pensize) {
			Initialize(_applied, _pensize);
		}
		public PenSizerDialog(AppliedSettings _applied, float _pensize, double min, double max) {
			Initialize(_applied, _pensize);
			Sizer.Minimum = min;
			Sizer.Maximum = max;
		}

		private void SetPreview(float f) {
			if (PreviewImage == null) return;
			Bitmap bmp = new Bitmap(100, 100);
			Graphics g = Graphics.FromImage(bmp);
			pen.Width = f;

			g.DrawRectangle(pen, 50, 50, 1, 1);

			var ms = new MemoryStream();
			bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

			var frame = new BitmapImage();
			frame.BeginInit();
			frame.StreamSource = ms;
			frame.CacheOption = BitmapCacheOption.OnLoad;
			frame.EndInit();
			frame.Freeze();

			PreviewImage.Source = frame;
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			pensize = (float)Sizer.Value;
			SetPreview(pensize);
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			if (ActionOnClose != null) {
				ActionOnClose(pensize);
			}
			this.Close();
		}
	}
}
