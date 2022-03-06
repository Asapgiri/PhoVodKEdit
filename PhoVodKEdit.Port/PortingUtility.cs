using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public PortingUtility(Window _mainWindow, AppliedSettings _applied)
		{
			MainWindow = _mainWindow;
			Applied = _applied;
		}

		public UserControl GetWindow()
		{
			return OwnWindow;
		}

    }
}
