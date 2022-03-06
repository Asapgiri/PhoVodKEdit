using System;
using System.Collections.Generic;
using PhoVodKEdit.Port;

namespace PhoVodKEdit.Loader
{
	public interface IResourceLoader
	{
		List<Type> Load<T, Other>(out List<Type> others) where T : PortingUtility where Other : PortingUtility;
		List<Type> LoadEffects(out List<Type> others);
		List<Type> LoadScreens(out List<Type> others);
		string [] GetLoadableResources();

		PortingUtility CreateInstance(Type type);
	}
}
