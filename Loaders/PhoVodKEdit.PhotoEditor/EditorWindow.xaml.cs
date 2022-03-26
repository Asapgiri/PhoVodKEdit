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

		private double previousPercentage;
		private bool initialized = false;

		public EditorWindow()
		{
			InitializeComponent();
		}

		public void InitCanvas(Bitmap image) {
			//Canvas.Height = image.Height;
			//Canvas.Width = image.Width;
			imageHeight = image.Height;
			imageWidth = image.Width;
			Canvas.SetImage(image);
			if (initialized) {
				SetCanvasSize(previousPercentage, false);

			}
			else {
				SetCanvasSize();
				initialized = true;
			}
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

		}

		private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {

		}

		private void Canvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

		}
	}
}
