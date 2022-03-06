using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.Loader
{
	public class ResourceLoader : IResourceLoader
	{
		private const string LOAD_DIR = "Loaders";

		private Window MainWindow { get; set; }
		private AppliedSettings Applied { get; set; }
		private PropertyChangedEventHandler PropertyChanged { get; set; }

		public ResourceLoader(Window _mainWindow, AppliedSettings _applied, PropertyChangedEventHandler _eventHandler)
		{
			MainWindow = _mainWindow;
			Applied = _applied;
			PropertyChanged = _eventHandler;
		}

		public ResourceLoader()
		{
			MainWindow = null;
		}

		public string [] GetLoadableResources()
		{
			if (!Directory.Exists(LOAD_DIR))
			{
				Directory.CreateDirectory(LOAD_DIR);
			}

			string [] loadableFiles = Directory.GetFiles(LOAD_DIR, "*.dll");

			return loadableFiles;
		}

		public List<Type> Load<T, Other>(out List<Type> others) where T: PortingUtility where Other: PortingUtility
		{
			string[] lf = GetLoadableResources();
			List<Type> ret = new List<Type>();
			others = new List<Type>();

			foreach (string dllPath in lf)
			{
				var DLL = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), dllPath));

				foreach (Type type in DLL.GetTypes())
				{
					if (type.IsSubclassOf(typeof(T)))
					{
						ret.Add(type);
					}
					else if (type.IsSubclassOf(typeof(Other)))
					{
						others.Add(type);
					}
				}
			}

			return ret;
		}

		public List<Type> LoadEffects(out List<Type> others) => Load<PortEffect, PortingUtility>(out others);

		public List<Type> LoadScreens(out List<Type> others) => Load<PortScreen, PortEffect>(out others);

		public List<Type> Load(out List<Type> others) => Load<PortingUtility, PortingUtility>(out others);

		public PortingUtility CreateInstance(Type type)
		{
			return Activator.CreateInstance(type, new object[] { MainWindow, Applied }) as PortingUtility;
		}
	}
}
