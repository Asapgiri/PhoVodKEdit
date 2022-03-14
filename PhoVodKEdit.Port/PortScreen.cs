using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.Port
{
	/// <summary>
	/// Porting class for the screens.
	/// </summary>
	public abstract class PortScreen : PortingUtility
	{
		protected List<Effect> Effects = new List<Effect>();

		protected ContentFilter ContentFilter { get; set; }

		public PortScreen(AppliedSettings _applied) : base(_applied)
		{
			ContentFilter = new ContentFilter()
			{
				InitDirectory = Directory.GetCurrentDirectory(),
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
				RestoreDirectory = true
			};
		}

		public bool DefaultBackground { get; private set; } = true;

		public virtual IList<Effect> GetEffects()
		{
			return Effects;
		}
		public UserControl GetWindow()
		{
			return OwnWindow;
		}

		public abstract void SetContent(string contentPath);
		public abstract Window CreateNewContent();
	}
}
