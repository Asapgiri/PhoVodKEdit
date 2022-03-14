using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
		private Bitmap image;

		public Editor(AppliedSettings _applied) : base(_applied)
		{
			OwnWindow = new EditorWindow();
		}

		public override Window CreateNewContent()
		{
			throw new NotImplementedException();
		}

		public override void SetContent(string contentPath)
		{
			image = new Bitmap(contentPath);
			(OwnWindow as EditorWindow).SetCanvas(image);
		}
	}
}
