using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows;
using System.Windows.Controls;

using CKResult = Common.Util.CKResult;
using ENTRY_MAP = System.Collections.Generic.Dictionary<string, string>;

namespace Common
{
	public class LanguageManager
	{
		public enum MessageType
		{
			ERROR,
			MENU,
			STATUS,
			UI,

			UTILRESULT,
			UTILEMG,
		}
		
		Dictionary<MessageType, ENTRY_MAP> m_StrMaps = null;

		ENTRY_MAP m_StrErr = null;
		ENTRY_MAP m_StrMenu = null;
		ENTRY_MAP m_StrStat = null;
		ENTRY_MAP m_StrUI = null;

		string m_sLanguage = "";

		static LanguageManager m_Instance = null;
		public static LanguageManager instance
		{
			get
			{
				if (m_Instance != null)
					return m_Instance;

				m_Instance = new LanguageManager();
				return m_Instance;
			}
		}

		/// <summary>
		/// Path to the Languages files;
		/// File names in order ERROR, MENU, STATUS, UI, UTIL_RESULT + UTIL_EMG;
		/// Captions of the Files in order ERROR, MENU, STATUS, UI, UTIL_RESULT, UTIL_EMG;
		/// current language from ini-file
		/// </summary>
		/// <param name="path"></param>
		/// Path to the Languages files
		/// <param name="files"></param>
		/// File names in order ERROR, MENU, STATUS, UI, UTIL_RESULT + UTIL_EMG
		/// <param name="captions"></param>
		/// Captions of the Files in order ERROR, MENU, STATUS, UI, UTIL_RESULT, UTIL_EMG
		/// <param name="lang"></param>
		/// current language from ini-file
		/// <returns></returns>
		public CKResult init(string path, string[] files, string[] captions, string lang)
		{
			CKResult hr = CKResult.CK_OK;

			m_sLanguage = lang;
			DirectoryInfo di = new DirectoryInfo(path);

			if (!di.Exists)
			{
				Util.errorDetail = di.FullName;
				return CKResult.CK_DNF;
			}

			FileInfo[] fInfos = new FileInfo[files.Length];
			for (int i = 0; i < fInfos.Length; i++)
			{
				fInfos[i] = new FileInfo(System.IO.Path.Combine(path, files[i]));
			}

			foreach (FileInfo fi in fInfos)
			{
				if (fi == fInfos[fInfos.Length - 1])
				{
					break;
				}

				if (!fi.Exists || fi.Length <= 0)
				{					
					Util.errorDetail = fi.FullName;
					return CKResult.CK_FNF;
				}
			}

			FileReader[] readers = new FileReader[fInfos.Length];
			for (int i = 0; i < fInfos.Length; i++)
			{
				readers[i] = new FileReader();
				
				if ((hr = readers[i].init(fInfos[i].FullName)) != CKResult.CK_OK && i < fInfos.Length - 1)
				{
					Util.errorDetail = readers[i].DataFile.FullName;
					return hr;
				}

				if ((hr = readers[i].read(false)) != CKResult.CK_OK && i < fInfos.Length - 1)
				{
					Util.errorDetail = readers[i].DataFile.FullName;
					return hr;
				}
			}

			m_StrMaps = new Dictionary<MessageType, Dictionary<string, string>>();
			int j = 0;
			for(int i = 0; i < captions.Length; i++)
			{
				m_StrMaps.Add((MessageType)i, readers[j].getEntryDic(captions[i]));
				if (i < captions.Length - 2)
					j++;
			}

			if (m_StrMaps[MessageType.UTILRESULT] == null)
				m_StrMaps.Remove(MessageType.UTILRESULT);

			if (m_StrMaps[MessageType.UTILEMG] == null)
				m_StrMaps.Remove(MessageType.UTILEMG);

			return CKResult.CK_OK;
		}

		public string Language
		{
			get
			{
				return m_sLanguage;
			}
		}

		private ENTRY_MAP selectMap(MessageType type)
		{
			if (m_StrMaps.Keys.Contains(type))
				return m_StrMaps[type];

			return null;
		}

		public void setElementText(FrameworkElement elem, MessageType type)
		{
			ENTRY_MAP strings = selectMap(type);

			if (elem is MenuItem)
				((MenuItem)elem).Header = strings.Keys.Contains(elem.Name) ? strings[elem.Name] : "NOT FOUND";
			else if (elem is GroupBox)
				((GroupBox)elem).Header = strings.Keys.Contains(elem.Name) ? strings[elem.Name] : "NOT FOUND";
			else if (elem is TabItem)
				((TabItem)elem).Header = strings.Keys.Contains(elem.Name) ? strings[elem.Name] : "NOT FOUND";

		}

		public string getMessage(string key, MessageType type)
		{
			ENTRY_MAP strings = selectMap(type);

			return strings.Keys.Contains(key) ? strings[key] : "MESSAGE NOT FOUND";
		}

		public CKResult setUtilMessages()
		{
			ENTRY_MAP map = selectMap(MessageType.UTILRESULT);
			if (map != null)
				Util.setCKResultText(map.Values.ToArray<string>());

			map = selectMap(MessageType.UTILEMG);
			if (map != null)
				Util.setEmergancyText(map.Values.ToArray<string>());

			return CKResult.CK_OK;
		}
	}
}
