using System.Collections.Generic;
using PhoVodKEdit.Port;

namespace PhoVodKEdit.Loader
{
	public interface IResourceLoader
	{
		List<PortingUtility> Load();
		List<PortEffect> LoadEffects();
		List<PortScreen> LoadScreens();
		string [] GetLoadableResources();
	}
}
