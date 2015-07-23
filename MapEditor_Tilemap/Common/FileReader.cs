using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using CKResult = Common.Util.CKResult;

using ENTRY_MAP = System.Collections.Generic.Dictionary<string, string>;
using CAPTION_MAP = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;

namespace Common
{
	public class FileReader
	{
		FileInfo m_FileInfo = null;

		CAPTION_MAP m_EntryList = null;

		bool m_bIgnoreCase = true;

		public CKResult init(string file)
		{
			m_FileInfo = new FileInfo(file);
			if (m_FileInfo.Exists && m_FileInfo.Length > 0)
				return CKResult.CK_OK;

			return CKResult.CK_FNF;
		}

		public CKResult read(bool ignoreCase = true)
		{
			if (!m_FileInfo.Exists || m_FileInfo.Length <= 0)
				return CKResult.CK_FNF;

			m_EntryList = new CAPTION_MAP(5);
			m_bIgnoreCase = ignoreCase;

			int iLines = getLines();

			if (iLines <= 0)
				return CKResult.CK_FORMAT;

			string[] sFileData = getFileData(iLines);

			if (sFileData.Length <= 0)
				return CKResult.CK_FORMAT;

			for (int i = 0; i < sFileData.Length; i++)
			{
				string sLine = sFileData[i];

				if (sLine == null || sLine == "[END]")
					break;

				if (sLine == String.Empty || sLine.StartsWith("//"))
					continue;

				bool bUnderCaption = false;
				if (sLine[0] == '[' && sLine[sLine.Length - 1] == ']')
				{
					bUnderCaption = true;
					string sCaption = sLine.Substring(1, sLine.Length - 2).ToUpper();
					
					ENTRY_MAP KeyValueList = new ENTRY_MAP();

					while (bUnderCaption)
					{
						i++;
						sLine = sFileData[i];

						if (sLine == null || sLine == "[END]")
							break;

						if (sLine == String.Empty || sLine.StartsWith("//"))
							continue;

						if (sLine[0] == '[' && sLine[sLine.Length - 1] == ']')
						{
							bUnderCaption = false;
							i -= 2;
							break;
						}

						int lEqualPos = sLine.IndexOf('=');
						if (lEqualPos <= 0)
							return CKResult.CK_FORMAT;

						string sEntry = sLine.Substring(0, lEqualPos);
						if (m_bIgnoreCase)
							sEntry = sEntry.ToUpper();

						string sValue = sLine.Substring(lEqualPos + 1, sLine.Length - (lEqualPos + 1));
						sValue = sValue.Substring(1, sValue.Length - 2);

						KeyValueList.Add(sEntry, sValue);
					}

					if (KeyValueList.Count <= 0)
						return CKResult.CK_FORMAT;

					m_EntryList.Add(sCaption, KeyValueList);
					i++;
				}
			}

			if (m_EntryList.Count <= 0)
				return CKResult.CK_FORMAT;

			return CKResult.CK_OK;
		}

		private int getLines()
		{
			StreamReader reader = new StreamReader(m_FileInfo.FullName, Encoding.Default);

			int iLines = 0;
			while (!reader.EndOfStream)
			{
				reader.ReadLine();
				iLines++;
			}

			reader.Close();

			return iLines;
		}

		private string[] getFileData(int lines)
		{
			StreamReader reader = new StreamReader(m_FileInfo.FullName, Encoding.UTF8);

			string[] sFileData = new String[lines];
			int j = 0;

			while (!reader.EndOfStream)
			{
				sFileData[j] = reader.ReadLine();
				j++;
			}

			reader.Close();

			return sFileData;
		}

		public FileInfo DataFile { get { return m_FileInfo; } }


		public bool hasEntry(string caption, string entry)
		{
			if (m_EntryList == null)
				return false;

			if (m_bIgnoreCase)
			{
				caption = caption.ToUpper();
				entry = entry.ToUpper();
			}

			if (m_EntryList.Keys.Contains(caption))
			{
				if (m_EntryList[caption].Keys.Contains(entry))
				{
					return true;
				}
			}

			return false;
		}

		public int getEntryInt(string caption, string entry)
		{
			if (hasEntry(caption, entry))
			{
				if(m_bIgnoreCase)
					return Int32.Parse(m_EntryList[caption.ToUpper()][entry.ToUpper()]);
				else
					return Int32.Parse(m_EntryList[caption][entry]);
			}

			return 0;
		}
		public float getEntryFloat(string caption, string entry)
		{
			if (hasEntry(caption, entry))
			{
				if (m_bIgnoreCase)
					return Single.Parse(m_EntryList[caption.ToUpper()][entry.ToUpper()]);
				else
					return Single.Parse(m_EntryList[caption][entry]);
			}

			return 0.0f;
		}
		public bool getEntryBool(string caption, string entry)
		{
			if (hasEntry(caption, entry))
			{
				if (m_bIgnoreCase)
					return Boolean.Parse(m_EntryList[caption.ToUpper()][entry.ToUpper()]);
				else
					return Boolean.Parse(m_EntryList[caption][entry]);
			}

			return false;
		}
		public string getEntryString(string caption, string entry)
		{
			if (hasEntry(caption, entry))
			{
				if (m_bIgnoreCase)
					return m_EntryList[caption.ToUpper()][entry.ToUpper()];
				else
					return m_EntryList[caption][entry];
			}

			return String.Empty;
		}

		public ENTRY_MAP getEntryDic(string caption)
		{
			if(m_EntryList != null && m_EntryList.Keys.Contains(caption.ToUpper()))
			{
				return m_EntryList[caption.ToUpper()];
			}

			return null;
		}

	}
}
