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
	class FileWriter
	{
		FileInfo m_FileInfo = null;
		bool m_bNewFile = false;
		CAPTION_MAP m_EntryList = null;

		string[] m_sFileData = null;

		public CKResult init(string file, bool newFile = false)
		{
			if (newFile)
			{
				m_FileInfo = new FileInfo(file);
				m_FileInfo.Create();
				m_bNewFile = true;
			}

			m_FileInfo = new FileInfo(file);
			if (!m_FileInfo.Exists || (m_FileInfo.Length <= 0 && !m_bNewFile))
				return CKResult.CK_FNF;

			CKResult hr = CKResult.CK_OK;
			if ((hr = read()) != CKResult.CK_OK)
				return hr;

			return CKResult.CK_OK;
		}


		private CKResult read()
		{
			if (!m_FileInfo.Exists || (m_FileInfo.Length <= 0 && !m_bNewFile))
				return CKResult.CK_FNF;

			if (!m_bNewFile)
			{
				m_EntryList = new CAPTION_MAP(5);

				int iLines = getLines();

				if (iLines <= 0)
					return CKResult.CK_FORMAT;

				m_sFileData = getFileData(iLines);

				if (m_sFileData.Length <= 0)
					return CKResult.CK_FORMAT;

				for (int i = 0; i < m_sFileData.Length; i++)
				{
					string sLine = m_sFileData[i];

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
							sLine = m_sFileData[i];

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

							string sEntry = sLine.Substring(0, lEqualPos).ToUpper();
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

			return CKResult.CK_OK;
		}

		public CKResult write()
		{
			if (!m_FileInfo.Exists || (m_FileInfo.Length <= 0 && !m_bNewFile))
				return CKResult.CK_FNF;

			StreamWriter writer = new StreamWriter(m_FileInfo.FullName, false, Encoding.Default);

			if (m_bNewFile)
			{
				List<string> fileData = new List<string>();

				foreach(var caption in m_EntryList)
				{
					fileData.Add(caption.Key.ToUpper());

					foreach(var entry in caption.Value)
					{
						fileData.Add(entry.Key + "=\"" + entry.Value + "\"");
					}

					fileData.Add("");
				}

				fileData.Add("[END]");

				foreach(string line in fileData)
				{
					writer.WriteLine(line);
				}
			}
			else
			{
				for (int i = 0; i < m_sFileData.Length; i++)
				{
					string sLine = m_sFileData[i];

					if (sLine == null || sLine == "[END]")
						break;

					if (sLine == String.Empty || sLine.StartsWith("//"))
						continue;

					string sCaption = "";
					if (sLine[0] == '[' && sLine[sLine.Length - 1] == ']')
					{
						sCaption = sLine.Substring(1, sLine.Length - 2);
					}
					else
						continue;

					if (m_EntryList.Keys.Contains(sCaption))
					{
						bool bUnderCaption = true;
						while (bUnderCaption)
						{
							sLine = m_sFileData[++i];

							if (sLine == null || sLine == "[END]")
								break;

							if (sLine == String.Empty || sLine.StartsWith("//"))
								continue;

							if (sLine[0] == '[' && sLine[sLine.Length - 1] == ']')
							{
								bUnderCaption = false;
								i--;
								break;
							}

							int iEqualPos = sLine.IndexOf('=');
							if (iEqualPos <= 0)
								return CKResult.CK_FORMAT;

							string sEntry = sLine.Substring(0, iEqualPos);
							if(m_EntryList[sCaption].Keys.Contains(sEntry))
							{
								m_sFileData[i] = sEntry + "=\"" + m_EntryList[sCaption][sEntry] + "\"";
							}
						}

					}
				}

				foreach (string line in m_sFileData)
				{
					writer.WriteLine(line);
				}
			}

			writer.Close();
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
			StreamReader reader = new StreamReader(m_FileInfo.FullName, Encoding.Default);

			m_sFileData = new String[lines];
			int j = 0;

			while (!reader.EndOfStream)
			{
				m_sFileData[j] = reader.ReadLine();
				j++;
			}

			reader.Close();

			return m_sFileData;
		}

		public FileInfo DataFile { get { return m_FileInfo; } }


		public bool hasEntry(string caption, string entry)
		{
			caption = caption.ToUpper();
			entry = entry.ToUpper();

			if (m_EntryList.Keys.Contains(caption))
			{
				if (m_EntryList[caption].Keys.Contains(entry))
				{
					return true;
				}
			}

			return false;
		}

		public CKResult setEntry(string caption, string entry, object value)
		{
			caption = caption.ToUpper();
			entry = entry.ToUpper();

			if (m_bNewFile)
			{
				if (m_EntryList.Keys.Contains(caption))
				{
					if (m_EntryList[caption].Keys.Contains(entry))
					{
						m_EntryList[caption][entry] = value.ToString();
						return CKResult.CK_OK;
					}

					m_EntryList[caption].Add(entry, value.ToString());
					return CKResult.CK_OK;
				}

				ENTRY_MAP map = new ENTRY_MAP();
				map.Add(entry, value.ToString());
				m_EntryList.Add(caption, map);
				return CKResult.CK_OK;
			}
			else
			{
				if (m_EntryList.Keys.Contains(caption))
				{
					if (m_EntryList[caption].Keys.Contains(entry))
					{
						m_EntryList[caption][entry] = value.ToString();
						return CKResult.CK_OK;
					}
				}
			}

			return CKResult.CK_IOOR;
		}
	}
}
