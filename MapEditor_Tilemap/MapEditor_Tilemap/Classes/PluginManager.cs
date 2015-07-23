using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using PluginContracts;

using CKResult = Common.Util.CKResult;

namespace MapEditor_Tilemap
{
	class PluginManager
	{
		static PluginManager m_Instance = null;

		public static PluginManager instance
		{
			get
			{
				if (m_Instance != null)
					return m_Instance;

				m_Instance = new PluginManager();
				return m_Instance;
			}
		}

		private List<IPluginBase> m_PluginList = null;
		
		public int search(string path)
		{
			DirectoryInfo di = new DirectoryInfo(path);
			if (!di.Exists || di.GetFiles("*.dll").Length <= 0)
				return 0;

			m_PluginList = new List<IPluginBase>();

			foreach(FileInfo file in di.GetFiles("*.dll"))
			{
				ICollection<IPluginBase> listTypes = null;
				if (Plugin.PluginLoader<IPluginBase>.load(file, out listTypes) != CKResult.CK_OK)
					Common.Util.showInfo("File \"" + file.Name + "\" has no plugins!");

				if (listTypes != null)
				{
					foreach (IPluginBase plug in listTypes)
					{
						m_PluginList.Add(plug);
					}
				}
			}

			return m_PluginList.Count;
		}


		public bool checkAppVersion(int index, Version appVer)
		{
			if (checkIndex(index) && checkType(index, typeof(IPluginExport)))
			{
				if (appVer >= m_PluginList[index].AppVersion)
				{
					return true;
				}
            }

			return false;
		}


		public Version getPluginVersoin(int index)
		{
			if (checkIndex(index) && checkType(index, typeof(IPluginExport)))
			{
				return m_PluginList[index].PluginVersion;
			}

			return null;
		}
		public string getPluginDesc(int index)
		{
			if (checkIndex(index) && checkType(index, typeof(IPluginExport)))
			{
				return (m_PluginList[index]).Description;
			}

			return null;
		}
		public string getPluginLangDesc(int index, string lang)
		{
			if (checkIndex(index) && checkType(index, typeof(IPluginExport)))
			{
				return ((IPluginExport)m_PluginList[index]).DescriptionForLanguage(lang);
            }

			return null;
		}

		public string getPluginMenuEntry(int index)
		{
			if (checkIndex(index) && checkType(index, typeof(IPluginExport)))
			{
				return ((IPluginExport)m_PluginList[index]).MenuEntryName;
			}

			return null;
		}
		public CKResult callExport(int index, Export.SExportData args)
		{
			if(checkIndex(index) && checkType(index, typeof(IPluginExport)))
			{
				return ((IPluginExport)m_PluginList[index]).Export(null, args);
			}

			return CKResult.CK_IOOR;
		}


		private bool checkIndex(int index)
		{
			if (index >= 0 && index < m_PluginList.Count)
				return true;

			return false;
		}
		private bool checkType(int index, Type type)
		{
			if (m_PluginList[index].GetType().GetInterface(type.FullName) != null)
				return true;

			return false;
        }
	}
}
