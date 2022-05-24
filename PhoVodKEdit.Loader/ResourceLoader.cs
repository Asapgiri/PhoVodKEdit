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

		public Dictionary<string, List<Type>> Load<T, Other>(out List<Type> others) where T: PortingUtility where Other: PortingUtility
		{
			string[] lf = GetLoadableResources();
			Dictionary<string, List<Type>> ret = new Dictionary<string, List<Type>>();
			others = new List<Type>();

			foreach (string dllPath in lf) {
				var DLL = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), dllPath));
				Type[] types;
				try {
					types = DLL.GetTypes();
				}
				catch (System.Reflection.ReflectionTypeLoadException ex) {
					types = ex.Types;
				}

				foreach (Type type in types) {
					if (type != null && !type.IsAbstract) {
						if (type.IsSubclassOf(typeof(T))) {
							FileTypesAttribute extsAttr = type.GetCustomAttribute(typeof(FileTypesAttribute)) as FileTypesAttribute;
                            foreach (string ext in extsAttr.exts) {
								string extUp = ext.ToUpper();
								if (!ret.ContainsKey(extUp)) {
									ret[extUp] = new List<Type>();
                                }
								ret[extUp].Add(type);
                            }
						}
						else if (type.IsSubclassOf(typeof(Other))) {
							others.Add(type);
						}
					}
				}
			}

			return ret;
		}

		public List<Type> LoadEffects(out List<Type> others) => throw new NotImplementedException(); // Load<PortEffect, PortingUtility>(out others);

		public Dictionary<string, List<Type>> LoadScreens(out List<Type> others) => Load<PortScreen, PortEffect>(out others);

		public Dictionary<string, List<Type>> Load(out List<Type> others) => Load<PortingUtility, PortingUtility>(out others);

		public PortingUtility CreateInstance(Type type)
		{
			PortingUtility portedClass = Activator.CreateInstance(type, new object[] { Applied }) as PortingUtility;

			portedClass.MainWindow = MainWindow;

			return portedClass;
		}
	}
}
