using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PhoVodKEdit.Port
{
    public abstract class PortingUtility
    {
		protected Window MainWindow { get; set; }
		protected UserControl OwnWindow { get; set; }

		public PortingUtility(Window _mainWindow)
		{
			MainWindow = _mainWindow;
		}

		public UserControl GetWindow()
		{
			return OwnWindow;
		}

    }
}
