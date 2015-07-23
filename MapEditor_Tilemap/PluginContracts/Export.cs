using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

namespace PluginContracts
{
	public class Export
	{
		public struct SExportData
		{
			public int witch;
			public int times;
			public BackgroundWorker worker;
			public DoWorkEventArgs args;
		}
	}
}
