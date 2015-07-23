using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CKResult = Common.Util.CKResult;

namespace PluginContracts
{
    public interface IPluginExport : IPluginBase
    {
		string MenuEntryName { get; }
		CKResult Export(byte[] mapData, Export.SExportData args);
		string DescriptionForLanguage(string lang);
    }
}
