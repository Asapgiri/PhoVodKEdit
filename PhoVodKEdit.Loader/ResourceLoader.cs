using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PhoVodKEdit.Port;

namespace PhoVodKEdit.Loader
{
	public class ResourceLoader : IResourceLoader
	{
		private const string LOAD_DIR = "Loaders";

		private Window MainWindow { get; set; }

		public ResourceLoader(Window _mainWindow)
		{
			MainWindow = _mainWindow;
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

		public List<T> Load<T>() where T: PortingUtility
		{
			string[] lf = GetLoadableResources();
			List<T> ret = new List<T>();

			foreach (string dllPath in lf)
			{
				var DLL = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), dllPath));

				foreach (Type type in DLL.GetTypes().Where(x => x.IsSubclassOf(typeof(T))))
				{
					ret.Add(Activator.CreateInstance(type, new object[] { MainWindow }) as T);
				}
			}

			return ret;
		}

		public List<PortEffect> LoadEffects() => Load<PortEffect>();

		public List<PortScreen> LoadScreens() => Load<PortScreen>();

		public List<PortingUtility> Load() => Load<PortingUtility>();
	}
}
