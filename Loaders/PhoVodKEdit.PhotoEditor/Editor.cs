using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.PhotoEditor
{
	public class Editor : PortScreen
	{
		public Editor(Window _mainWindow, AppliedSettings _applied) : base(_mainWindow, _applied)
		{
			OwnWindow = new EditorWindow(_mainWindow);
		}
	}
}
