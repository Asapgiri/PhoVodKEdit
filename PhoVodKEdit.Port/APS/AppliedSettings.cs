using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PhoVodKEdit.Port.APS
{
	public class AppliedSettings
	{
		public struct Fonts
		{
			public double Size { get; set; }
		}

		public AppliedLanguage Language { get; set; }

		public AppliedColors Colors { get; set; }
		public Fonts Font { get; set; }

		public AppliedSettings(PropertyChangedEventHandler _eventHandler, bool _setDefaults = true)
		{
			if (_setDefaults)
			{
				this.Colors = new AppliedColors(_eventHandler)
				{
					MainColor = new SolidColorBrush(Settings.Colors.Dark.Main),
					SecondaryColor = new SolidColorBrush(Settings.Colors.Dark.Secondary),
					ForegroundColor = new SolidColorBrush(Settings.Colors.Dark.Foreground),
					BackgroundColor = new SolidColorBrush(Settings.Colors.Dark.Background),
					BorderColor = new SolidColorBrush(Settings.Colors.Dark.Border)
				};

				this.Font = new Fonts()
				{
					Size = Settings.Font.Size
				};

				this.Language = new AppliedLanguage("hu");
			}
		}

		
	}
}
