using System.Windows;

namespace PhoVodKEdit.Port
{
	public class PortScreen : PortingUtility
	{
		public PortScreen(Window _mainWindow) : base(_mainWindow)
		{
		}

		public bool DefaultBackground { get; private set; } = true;
	}
}
