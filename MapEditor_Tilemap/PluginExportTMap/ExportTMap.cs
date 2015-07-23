using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

using PluginContracts;

using CKResult = Common.Util.CKResult;

namespace PluginExportTMap
{
	public class ExportTMap : IPluginExport
	{
		private static readonly Version APP_VERSION = new Version(1, 0);
		private static readonly Version PLUGIN_VERSION = new Version(0, 1);
		private static readonly string PLUGIN_NAME = "ExportTMap";
		private static readonly string MENU_ENTRY = "Tile Map [*.TMAP]";
		private static readonly string PLUGIN_DESC = "Used to export Editor files to TMAP-format.";

		private Dictionary<string, string> m_LanguageDesc = null;

		public ExportTMap()
		{
			m_LanguageDesc = new Dictionary<string, string>();
			m_LanguageDesc.Add("EN-UK", PLUGIN_DESC);
			m_LanguageDesc.Add("EN-US", PLUGIN_DESC);
			m_LanguageDesc.Add("DE-DE", "Damit kann die Editordatei in das TMAP-Format exportiert werden.");
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
			Common.Util.showInfo();

			BackgroundWorker w = args.worker;
			DoWorkEventArgs e = args.args;

			int old = 0;
			const int MAX_CYCLE = 20000000;
			for (int i = 0; i < MAX_CYCLE; i++)
			{
				int percent = 100 * i / MAX_CYCLE + 1;
				if (percent > old)
					w.ReportProgress(percent);
				old = percent;

				for (int j = 0; j < args.times; j++)
				{
					double sqr = Math.Sqrt(i);
				}
			}

			e.Result = CKResult.CK_OK;

			return CKResult.CK_OK;
		}

		public string DescriptionForLanguage(string lang)
		{
			return m_LanguageDesc.Keys.Contains(lang) ? m_LanguageDesc[lang] : m_LanguageDesc["EN-UK"];
		}
	}
}
