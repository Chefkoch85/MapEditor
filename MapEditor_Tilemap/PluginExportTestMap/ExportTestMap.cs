using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

using System.ComponentModel;

using PluginContracts;

using CKResult = Common.Util.CKResult;

namespace PluginExportTestMap
{
	public class ExportTestMap : IPluginExport
	{
		private static readonly Version APP_VERSION = new Version(1, 0);
		private static readonly Version PLUGIN_VERSION = new Version(0, 1);
		private static readonly string PLUGIN_NAME = "ExportTestMap";
		private static readonly string MENU_ENTRY = "Test Map [*.TEM]";
		private static readonly string PLUGIN_DESC = "Used to export Editor files to TEM-format.";

		private Dictionary<string, string> m_LanguageDesc = null;

		public ExportTestMap()
		{
			m_LanguageDesc = new Dictionary<string, string>();
			m_LanguageDesc.Add("EN-UK", PLUGIN_DESC);
			m_LanguageDesc.Add("EN-US", PLUGIN_DESC);
			m_LanguageDesc.Add("DE-DE", "Damit kann die Editordatei in das TEM-Format exportiert werden.");
		}

		public Version AppVersion
		{
			get
			{
				return APP_VERSION;
			}
		}

		public string Description
		{
			get
			{
				return PLUGIN_DESC;
			}
		}

		public string MenuEntryName
		{
			get
			{
				return MENU_ENTRY;
			}
		}

		public string Name
		{
			get
			{
				return PLUGIN_NAME;
			}
		}

		public Version PluginVersion
		{
			get
			{
				return PLUGIN_VERSION;
			}
		}

		public CKResult Export(byte[] mapData, Export.SExportData args)
		{
			Util.showInfo();

			args.args.Result = CKResult.CK_PARAM;

			return CKResult.CK_PARAM;
		}

		public string DescriptionForLanguage(string lang)
		{
			return m_LanguageDesc.Keys.Contains(lang) ? m_LanguageDesc[lang] : m_LanguageDesc["EN-UK"];
		}
	}
}
