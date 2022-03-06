using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.Port
{
	public abstract class PortingUtility
    {
		protected Window MainWindow { get; set; }
		protected UserControl OwnWindow { get; set; }

		public AppliedSettings Applied { get; set; }

		public string Name { get; private set; }
		public string PublicName { get; private set; }
		public string Description { get; private set; } = string.Empty;

		public PortingUtility(Window _mainWindow, AppliedSettings _applied)
		{
			MainWindow = _mainWindow;
			Applied = _applied;
			Name = GetType().Name;
			PublicName = GetType().Name;
		}
    }
}
