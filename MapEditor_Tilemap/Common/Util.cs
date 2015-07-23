using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace Common
{
	public class Util
	{
		public enum CKResult
		{
			/// <summary>
			/// Result was OK
			/// </summary>
			CK_OK,
			/// <summary>
			/// Result was Enum.Result.Length != Result.Text.Length 
			/// </summary>
			CK_ERLRTL,
			/// <summary>
			/// Result was DirectoryNotFound
			/// </summary>
			CK_DNF,         // FileNotFound
			/// <summary>
			/// Result was FileNotFound
			/// </summary>
			CK_FNF,         // FileNotFound
			/// <summary>
			/// Result was error because of data/file format
			/// </summary>
			CK_FORMAT,
			/// <summary>
			/// Result was error with PARAM
			/// </summary>
			CK_PARAM,
			/// <summary>
			/// Result was IndexOutOfRange
			/// </summary>
			CK_IOOR,        // IndexOutOfRange
			/// <summary>
			/// Result was error by loading assembly
			/// </summary>				
			CK_ASMLOAD,     // ASsaMblyLoad	
		}

		public static int CKResultTextCount
		{
			get
			{
				return CKResultText.Length;
			}
		}

		private static readonly string[] CKResultText = {
			"Result was OK!",
			"Result was Enum.Result.Length != Text.Result.Length!",
			"Result was directory not found!",
			"Result was file not found!",
			"Result was file format unexpected!",
			"Result was wrong parameter for method!",
			"Result was index is out of range!",
			"Result was not able to load assambly/plugin",
		};
		private static string[] CKResultTextLang = null;
		public static void setCKResultText(string[] messages)
		{
			CKResultTextLang = messages;
		}

		private static string[] EmergancyMessage = {
			"FATAL ERROR!",
			"ERROR: ",
			"DESCRIPTION: ",
			"WHERE: ",
			"no detail informations!",
		};
		private static string[] EmergancyMessageLang = null;
		public static void setEmergancyText(string[] messages)
		{
			EmergancyMessageLang = messages;
		}


		public static string errorDetail { get; set; }
		public static void EmergancyClose(CKResult result, string msg = null)
		{
			Util.showInfo();

			string detail = EmergancyMessageLang != null ? EmergancyMessageLang[4] : EmergancyMessage[4];
			if (msg == null && errorDetail != null)
				detail = errorDetail;
			else if (msg != null && errorDetail == null)
				detail = msg;
			else if (msg != null && errorDetail != null)
				detail = msg + "\n" + errorDetail;
			try
			{
				string sMessage = (EmergancyMessageLang != null ? EmergancyMessageLang[1] : EmergancyMessage[1]) + "\n" + result.ToString() + "\n\n";
				sMessage += (EmergancyMessageLang != null ? EmergancyMessageLang[2] : EmergancyMessage[2]) + "\n";
				sMessage += (CKResultTextLang != null ? CKResultTextLang[(int)result] : CKResultText[(int)result]) + "\n" + detail + "\n\n";
				sMessage += (EmergancyMessageLang != null ? EmergancyMessageLang[3] : EmergancyMessage[3]) + Util.CurrentMethodName(2);

				MessageBox.Show(sMessage, EmergancyMessageLang != null ? EmergancyMessageLang[0] : EmergancyMessage[0], MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				showInfo(ex.Message);
			}

			//Application.Current.Shutdown();   // normal method to close a programm
			Environment.Exit(0x01);             // hard mode to close a programm it is like kill the process 
		}

		public static string CurrentMethodName(int index = 1)
		{
			StackTrace stackTrace = new StackTrace();           // get call stack
			StackFrame stackFrame = stackTrace.GetFrame(index);		// get second method from the stack (frame)(first method on stack ist this one)

			return stackFrame.GetMethod().ReflectedType + "." + stackFrame.GetMethod().Name + "()";
		}

		public static string InfoMessage(object arg, int index = 2)
		{
			return CurrentMethodName(index) + " : " + arg.ToString();
		}
		public static string InfoMessage(object arg1, object arg2, int index = 2)
		{
			return CurrentMethodName(index) + " : " + arg1.ToString() + " = " + arg2.ToString();
		}

		public static void showInfo()
		{
			Debug.WriteLine(CurrentMethodName(2));
		}
		public static void showInfo(object arg)
		{
			Debug.WriteLine(InfoMessage(arg, 3));
		}
		public static void showInfo(object arg1, object arg2)
		{
			Debug.WriteLine(InfoMessage(arg1, arg2, 3));
		}
	}
}
