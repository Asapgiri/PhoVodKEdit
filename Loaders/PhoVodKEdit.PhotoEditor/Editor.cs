using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PhoVodKEdit.Port;

namespace PhoVodKEdit.PhotoEditor
{
	public class Editor : PortScreen
	{
		public Editor(Window _mainWindow) : base(_mainWindow)
		{
			OwnWindow = new PhotoEditorWindow();
		}
	}
}
