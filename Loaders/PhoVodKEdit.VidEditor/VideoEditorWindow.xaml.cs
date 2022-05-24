using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhoVodKEdit.VidEditor {
	/// <summary>
	/// Interaction logic for VideoEditorWindow.xaml
	/// </summary>
	public partial class VideoEditorWindow : UserControl {
		private VideoEditor editor;
		private bool changeFromSelf = false;

		public VideoEditorWindow(VideoEditor _editor) {
			InitializeComponent();
			editor = _editor;
			FrameBox.Source = editor.ActualFrame;
			var heightDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
			heightDescriptor.AddValueChanged(TimeControlGrid.ColumnDefinitions[0], TickChanged);
		}

		private void TickChanged(object sender, EventArgs e) {
			if (!changeFromSelf) {
				editor.SetTimespanFromControlGrid(TimeControlGrid.ColumnDefinitions[0].ActualWidth);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			if (editor.Playing) {
				editor.PauseVideo();
				(sender as Button).Content = "|>";
			}
			else {
				editor.PlayVideo();
				(sender as Button).Content = "||";
			}
		}

		internal void UpdateFrame() {
			changeFromSelf = true;
			Dispatcher.Invoke(new Action(() => FrameBox.Source = editor.ActualFrame));
			changeFromSelf = false;
		}

		internal void ReloedLayers() {
			changeFromSelf = true;
			FrameControlPanel.Children.Clear();
			FramePanel.Children.Clear();

			foreach (var item in editor.VideoLayers) {
				FrameControlPanel.Children.Add(item.Control);
				FramePanel.Children.Add(item.Panel);
			}
			changeFromSelf = false;
		}

		internal void SetPlayLine(double v) {
			changeFromSelf = true;
			Dispatcher.Invoke(new Action(() => {
				TimeControlGrid.ColumnDefinitions[0].Width = new GridLength(v);
			}));
			changeFromSelf = false;
		}
	}
}
