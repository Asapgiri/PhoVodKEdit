using System;
using System.Collections.Generic;
using PhoVodKEdit.Port;

namespace PhoVodKEdit.Loader
{
	public interface IResourceLoader
	{
		List<Type> Load();
		List<Type> LoadEffects();
		List<Type> LoadScreens();
		string [] GetLoadableResources();

		PortingUtility CreateInstance(Type type);
	}
}
