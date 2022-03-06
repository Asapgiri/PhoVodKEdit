using System.Windows;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.Port
{
	public class PortScreen : PortingUtility
	{
		public PortScreen(Window _mainWindow, AppliedSettings _applied) : base(_mainWindow, _applied)
		{
		}

		public bool DefaultBackground { get; private set; } = true;
	}
}
