using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Windows;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.GifEditor {
	/// <summary>
	/// Interaction logic for CreateNew.xaml
	/// </summary>
	public partial class CreateNew : Window, INotifyPropertyChanged {
		#region Variables
		private int newWidth = 500;
		private int newHeight = 400;
		private Action<int, int, PixelFormat> callback;
		#endregion Variables

		#region Properties
		public AppliedSettings Applied { get; set; }
		public int NewWidth {
			get {
				return newWidth;
			}
			set {
				newWidth = value;
				OnPropertyChanged("NewWidth");
			}
		}
		public int NewHeight {
			get {
				return newHeight;
			}
			set {
				newHeight = value;
				OnPropertyChanged("NewHeight");
			}
		}
		#endregion Properties

		public CreateNew(AppliedSettings _applied, Action<int, int, PixelFormat> action) {
			DataContext = this;
			Applied = _applied;
			callback = action;
			InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Button_IncreaseHeightClick(object sender, RoutedEventArgs e) {
			NewHeight = (newHeight / 100 + 1) * 100;
		}

		private void Button_DecreaseHeightClick(object sender, RoutedEventArgs e) {
			if (NewHeight > 100) {
				NewHeight = (newHeight / 100 - 1) * 100; ;
			}
			else {
				NewHeight = 100;
			}
		}

		private void Button_IncreaseWidthClick(object sender, RoutedEventArgs e) {
			NewWidth = (newWidth / 100 + 1) * 100;
		}

		private void Button_DecreaseWidthClick(object sender, RoutedEventArgs e) {
			if (NewWidth > 100) {
				NewWidth = (newWidth / 100 - 1) * 100; ;
			}
			else {
				NewWidth = 100;
			}
		}

		private void Button_CloseClick(object sender, RoutedEventArgs e) {
			Close();
		}

		private void Button_AddClick(object sender, RoutedEventArgs e) {
			callback(newWidth, newHeight, PixelFormat.Format32bppArgb);
			Close();
		}
	}
}
