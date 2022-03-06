using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

		#endregion Variables

		public AppliedColors(PropertyChangedEventHandler _eventHandler)
		{
			this.PropertyChanged = _eventHandler;
		}

		#region Properties

		public Brush MainColor
		{
			get
			{
				return _mainColor;
			}
			set
			{
				_mainColor = value;
				OnPropertyChanged("MainColor");

			}
		}

		public Brush SecondaryColor
		{
			get
			{
				return _secondaryColor;
			}
			set
			{
				_secondaryColor = value;
				OnPropertyChanged("SecondaryColor");

			}
		}

		public Brush ForegroundColor
		{
			get
			{
				return _foregroundColor;
			}
			set
			{
				_foregroundColor = value;
				OnPropertyChanged("ForegroundColor");

			}
		}

		public Brush BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;
				OnPropertyChanged("BackgroundColor");

			}
		}

		public Brush BorderColor
		{
			get
			{
				return _borderColor;
			}
			set
			{
				_borderColor = value;
				OnPropertyChanged("BorderColor");

			}
		}

		#endregion Properties

		#region INotifiedProperty Block
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;

			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion
	}
}
