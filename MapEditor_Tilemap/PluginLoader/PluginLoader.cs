using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using CKResult = Common.Util.CKResult;

namespace Plugin
{
    public class PluginLoader<T>
    {
		public static CKResult load(FileInfo pluginFile, out ICollection<T> plugins)
		{
			plugins = null;

			if (pluginFile == null)
				return CKResult.CK_PARAM;
			
			AssemblyName an = AssemblyName.GetAssemblyName(pluginFile.FullName);
			Assembly assembly = Assembly.Load(an);
			
			Type pluginType = typeof(T);
			ICollection<Type> pluginTypes = new List<Type>();

			if (assembly != null)
			{
				Type[] types = assembly.GetTypes();

				foreach (Type type in types)
				{
					if (type.IsInterface || type.IsAbstract)
					{
						continue;
					}
					else
					{
						if (type.GetInterface(pluginType.FullName) != null)
						{
							pluginTypes.Add(type);
						}
					}
				}
			}

			if (pluginTypes.Count <= 0)
				return CKResult.CK_ASMLOAD;
			
			plugins = new List<T>(pluginTypes.Count);

			foreach (Type type in pluginTypes)
			{
				T plugin = (T)Activator.CreateInstance(type);
				plugins.Add(plugin);
			}

			return CKResult.CK_OK;
		}
	}
}
