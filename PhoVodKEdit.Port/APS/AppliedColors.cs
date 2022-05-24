using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PhoVodKEdit.Port.APS
{
	public class AppliedColors : INotifyPropertyChanged
	{
		#region Variables
		private Brush _mainColor;
		private Brush _secondaryColor;
		private Brush _foregroundColor;
		private Brush _backgroundColor;
		private Brush _borderColor;
		private Brush _dangerColor;
		#endregion Variables

		public AppliedColors(PropertyChangedEventHandler _eventHandler)
		{
			this.PropertyChanged = _eventHandler;
		}

		#region Properties
		public Brush Main
		{
			get
			{
				return _mainColor;
			}
			set
			{
				_mainColor = value;
				OnPropertyChanged("Main");

			}
		}

		public Brush Secondary {
			get
			{
				return _secondaryColor;
			}
			set
			{
				_secondaryColor = value;
				OnPropertyChanged("Secondary");

			}
		}

		public Brush Foreground {
			get
			{
				return _foregroundColor;
			}
			set
			{
				_foregroundColor = value;
				OnPropertyChanged("Foreground");

			}
		}

		public Brush Background {
			get
			{
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;
				OnPropertyChanged("Background");

			}
		}

		public Brush Border {
			get
			{
				return _borderColor;
			}
			set
			{
				_borderColor = value;
				OnPropertyChanged("Border");

			}
		}

		public Brush Danger {
			get {
				return _dangerColor;
			}
			set {
				_dangerColor = value;
				OnPropertyChanged("Danger");

			}
		}
		#endregion Properties

		#region INotifiedProperty Block
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null) {
			if (!Equals(field, newValue)) {
				field = newValue;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
				return true;
			}

			return false;
		}
		#endregion
	}
}
