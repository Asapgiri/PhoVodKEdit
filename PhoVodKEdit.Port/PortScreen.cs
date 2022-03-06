using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.Port
{
	public abstract class PortScreen : PortingUtility
	{
		protected List<Effect> Effects = new List<Effect>();

		public PortScreen(Window _mainWindow, AppliedSettings _applied) : base(_mainWindow, _applied)
		{
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
	}
}
