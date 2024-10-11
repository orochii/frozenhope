using Godot;
using System.Collections.Generic;

public static class OZResourceLoader {
	public static T Load<T>(string resPath) where T : Resource 
	{
		if(!loadedResources.TryGetValue(resPath, out var loadedRes))
		{
			if(!ResourceLoader.Exists(resPath))
				return null;
			loadedRes = ResourceLoader.Load<Resource>(resPath);
			loadedResources[resPath] = loadedRes;
		}
		return loadedRes as T;
	}
	readonly private static Dictionary<string, Resource> loadedResources = new Dictionary<string, Resource>();
}
