using System.Windows;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.Port
{
	public abstract class PortEffect : PortingUtility
	{
		public PortEffect(Window _mainWindow, AppliedSettings _applied) : base(_mainWindow, _applied) { }

		public abstract FrameworkElement GetView();
	}
}
