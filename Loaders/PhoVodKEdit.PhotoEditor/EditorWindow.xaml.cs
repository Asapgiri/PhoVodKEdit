using System.Drawing;
using System.Windows.Controls;

namespace PhoVodKEdit.PhotoEditor {
	/// <summary>
	/// Interaction logic for EditorWindow.xaml
	/// </summary>
	public partial class EditorWindow : UserControl
	{
		public EditorWindow()
		{
			InitializeComponent();
		}

		public void SetCanvas(Bitmap image)
		{
			Canvas.Height = image.Height;
			Canvas.Width = image.Width;
			Canvas.SetImage(image);
		}
	}
}
