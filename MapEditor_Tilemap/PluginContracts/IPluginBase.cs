using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginContracts
{
	public interface IPluginBase
	{
		Version PluginVersion { get; }
		Version AppVersion { get; }
		string Name { get; }
		string Description { get; }
	}
}
