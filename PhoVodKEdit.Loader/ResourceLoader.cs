using System;
using System.Collections.Generic;
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

		public ResourceLoader(Window _mainWindow, AppliedSettings _applied)
		{
			MainWindow = _mainWindow;
			Applied = _applied;
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

		public List<Type> Load<T>() where T: PortingUtility
		{
			string[] lf = GetLoadableResources();
			List<Type> ret = new List<Type>();

			foreach (string dllPath in lf)
			{
				var DLL = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), dllPath));

				foreach (Type type in DLL.GetTypes().Where(x => x.IsSubclassOf(typeof(T))))
				{
					ret.Add(type);
				}
			}

			return ret;
		}

		public List<Type> LoadEffects() => Load<PortEffect>();

		public List<Type> LoadScreens() => Load<PortScreen>();

		public List<Type> Load() => Load<PortingUtility>();

		public PortingUtility CreateInstance(Type type)
		{
			return Activator.CreateInstance(type, new object[] { MainWindow, Applied }) as PortingUtility;
		}
	}
}
