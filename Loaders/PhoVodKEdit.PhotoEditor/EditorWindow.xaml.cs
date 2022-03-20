using System.Drawing;
using System.Windows.Controls;

namespace PhoVodKEdit.PhotoEditor {
	/// <summary>
	/// Interaction logic for EditorWindow.xaml
	/// </summary>
	public partial class EditorWindow : UserControl
	{
		private double imageHeight;
		private double imageWidth;

		public EditorWindow()
		{
			InitializeComponent();
		}

		public void SetCanvas(Bitmap image)
		{
			Canvas.Height = image.Height;
			Canvas.Width = image.Width;
			imageHeight = image.Height;
			imageWidth = image.Width;
			Canvas.SetImage(image);
			SetCanvasSize();
		}

		public void SetCanvasSize(double percentage = 100) {
			if (percentage < 20) percentage = 100;
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
	}
}
