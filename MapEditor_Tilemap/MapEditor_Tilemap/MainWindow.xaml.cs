using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;

using Common;

using CKResult = Common.Util.CKResult;
using LM = Common.LanguageManager;

namespace MapEditor_Tilemap
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private static readonly Version APP_VERSION = new Version(1, 0);
		
		DispatcherTimer m_Timer = null;

		bool m_bDirty = false;
		bool m_bDoingExport = false;
		
		readonly int TILE_SIZE = 64;
		int m_iPosX = 0;
		int m_iPosY = 0;



		public MainWindow()
        {
            InitializeComponent();

			m_Timer = new DispatcherTimer();
			m_Timer.Tick += Tick;

			ResetUI();
        }



		private void ResetUI()
		{
			sbiCenter.Background = null;
			sbiProgress.Background = null;
			sbiRight.Background = null;

			sbiCenter.Foreground = Brushes.Black;
			sbiRight.Foreground = Brushes.Black;

			txbCursorX.Text = "";
			txbCursorY.Text = "";
			txbStatusCenter.Text = "";
			txbStatusRight.Text = "";
		}



		private void Tick(object sender, EventArgs e)
		{
			if (m_Timer.Tag.ToString() == "EXPORT")
			{
				if (txbStatusRight.Foreground == Brushes.Black)
				{
					sbiRight.Foreground = Brushes.Red;
					sbiRight.Background = null;

					sbiProgress.Background = null;
				}
				else
				{
					sbiRight.Foreground = Brushes.Black;
					sbiRight.Background = Brushes.Red;

					sbiProgress.Background = Brushes.Red;
				}
			}
		}



		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Util.showInfo();

			CKResult hr = CKResult.CK_OK;
			// check Util.CKResult has enouth Textentry in the transltion string array
			if(Util.CKResultTextCount != Enum.GetValues(typeof(Util.CKResult)).Length)
				Util.EmergancyClose(CKResult.CK_ERLRTL);

			// start / read Ini-file reader
			FileReader fileReader = new FileReader();
			if ((hr = fileReader.init(@"..\..\..\RES\CONFIG.INF")) != CKResult.CK_OK)
				Util.EmergancyClose(hr, fileReader.DataFile.FullName);
			if ((hr = fileReader.read()) != CKResult.CK_OK)
				Util.EmergancyClose(hr, fileReader.DataFile.FullName);

			// prepare Language infos
			string lang = fileReader.getEntryString("GENERAL", "LANGUAGE");
			string path = System.IO.Path.Combine(fileReader.getEntryString("PATH", "STRING"), lang);
			List<string> files = new List<string>();
			files.Add(fileReader.getEntryString("PATH", "STRERR"));
			files.Add(fileReader.getEntryString("PATH", "STRMENU"));
			files.Add(fileReader.getEntryString("PATH", "STRSTAT"));
			files.Add(fileReader.getEntryString("PATH", "STRUI"));
			files.Add(fileReader.getEntryString("PATH", "STRUTIL"));
			// start Language file input
			if ((hr = LM.instance.init(path, files.ToArray(), fileReader.getEntryDic("LANGCAPS").Values.ToArray<string>(), lang)) != CKResult.CK_OK)
                Util.EmergancyClose(hr);

			// set static text of application
			setText();
			// set language for Util
			LM.instance.setUtilMessages();

			// use Ini-file options from WINDOW
			Title = fileReader.getEntryString("WINDOW", "TITLE");
			Width = fileReader.getEntryInt("WINDOW", "WIDTH");
			Height = fileReader.getEntryInt("WINDOW", "HEIGHT");

			// check / load plugins
			int PluginCount = 0;
			PluginManager pm = PluginManager.instance;
			if ((PluginCount = pm.search(fileReader.getEntryString("PATH", "PLUGIN"))) <= 0)
				Util.showInfo("no plugins!");
			
			// add plugins to menu export
			AddExportPlugins(PluginCount);

			// load / add images to the listviews
			if ((hr = loadListViewContent(livTerrain, fileReader.getEntryString("PATH", "TERRAIN"))) != CKResult.CK_OK)
                Util.EmergancyClose(hr);
			if ((hr = loadListViewContent(livRessource, fileReader.getEntryString("PATH", "RESSOURCE"))) != CKResult.CK_OK)
				Util.EmergancyClose(hr);
			if ((hr = loadListViewContent(livDecal, fileReader.getEntryString("PATH", "DECAL"))) != CKResult.CK_OK)
				Util.EmergancyClose(hr);
			if ((hr = loadListViewContent(livModification, fileReader.getEntryString("PATH", "MODIFY"))) != CKResult.CK_OK)
				Util.EmergancyClose(hr);

		}
		
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Util.showInfo();
			
			e.Cancel = CloseApp();
		}



		private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
			Util.showInfo(((MenuItem)sender).Header + " : " + ((MenuItem)sender).Tag);

			if (sender.GetType() != typeof(MenuItem))
				return;

			MenuItem item = sender as MenuItem;
			
			if(item == mitNew)
			{
				newMap();
			}
			else if(item == mitClose)
			{
				Close();
			}
			else if(item == mitSave)
			{
				m_bDirty = false;
			}

			if(item.Tag is int)
			{
				startExport((int)item.Tag);
			}

        }
		
		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.OriginalSource.GetType() != typeof(TabControl))
				return;

			Util.showInfo(((TabControl)sender).Tag);
			
			TabItem item = ((TabControl)sender).SelectedItem as TabItem;

			foreach(TabItem i in ((TabControl)sender).Items)
			{
				i.Foreground = Brushes.White;
				i.Background = Brushes.Black;
			}

			item.Foreground = Brushes.Black;
			item.Background = Brushes.White;

		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Util.showInfo();
		}
		
		// for Scrollviewer to scroll with mouse wheel
		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			Util.showInfo();
			ScrollViewer scrollviewer = sender as ScrollViewer;
			scrollviewer.ScrollToVerticalOffset(scrollviewer.VerticalOffset - e.Delta);
			e.Handled = true;
		}



		private CKResult loadListViewContent(ListView target, string path)
		{
			Util.showInfo();

			DirectoryInfo di = new DirectoryInfo(path);
			if (!di.Exists || di.GetFiles("*", SearchOption.AllDirectories).Length <= 0)
			{
				Util.errorDetail = di.FullName;
				return CKResult.CK_DNF;
			}
			
			FileInfo[] fis = di.GetFiles("*", SearchOption.AllDirectories);

			BitmapImage[] bmps = ImageLoader.loadDirectory(path);
			
			target.SelectionMode = SelectionMode.Single;

			for (int i = 0; i < bmps.Length; i++)
			{
				Image img = new Image();
				Label l = new Label();

				img.Source = bmps[i];
				l.Content = fis[i].Name;
				l.Foreground = Brushes.White;

				DockPanel panel = new DockPanel();
				panel.Margin = new Thickness(2);
				panel.Children.Add(img);
				DockPanel.SetDock(img, Dock.Left);
				panel.Children.Add(l);
				DockPanel.SetDock(l, Dock.Right);

				ListViewItem item = new ListViewItem();
				item.Content = panel;

				target.Items.Add(item);
			}

			return CKResult.CK_OK;
		}

		private void newMap()
		{
			Util.showInfo();

			int rows = 8, cols = 8;
			
			grdMap.ShowGridLines = true;
			grdMap.Width = cols * TILE_SIZE;
			grdMap.Height = rows * TILE_SIZE;
			grdMap.Background = Brushes.Red;
			grdMap.MouseDown += new MouseButtonEventHandler(onMouseDown);

			for (int i = 0; i < rows; i++)
			{
				RowDefinition rd = new RowDefinition();
				rd.Height = new GridLength(TILE_SIZE, GridUnitType.Pixel);
				grdMap.RowDefinitions.Add(rd);
			}
			for (int i = 0; i < cols; i++)
			{
				ColumnDefinition cd = new ColumnDefinition();
				cd.Width = new GridLength(TILE_SIZE, GridUnitType.Pixel);
				grdMap.ColumnDefinitions.Add(cd);
			}

			txbCursorX.Text = String.Format("X: {0,4:0000}", m_iPosX);
			txbCursorY.Text = String.Format("{0,4:0000} :Y", m_iPosY);

			grbMap.Visibility = Visibility.Visible;
			m_bDirty = true;
		}

		private bool CloseApp()
		{
			Util.showInfo();
			if (m_bDirty)
			{
				MessageBoxResult hr = MessageBox.Show("Unsaved changes.\nWant to save them?", "QUESTION?", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                if (hr == MessageBoxResult.Cancel)
				{
					return true;
				}
				else if(hr == MessageBoxResult.Yes)
				{
					//save
				}
			}
			else if(m_bDoingExport)
			{
				MessageBoxResult hr = MessageBox.Show("Export in progress!\nDo you want to leave anyway?", "QUESTION?", MessageBoxButton.YesNo, MessageBoxImage.Information);
				if (hr == MessageBoxResult.No)
				{
					return true;
				}
			}

			return false;
		}

		private void AddExportPlugins(int count)
		{
			string sWelcome = LM.instance.getMessage("statCenterW1", LM.MessageType.STATUS);

			if (count <= 0)
				sWelcome += LM.instance.getMessage("statCenterW2", LM.MessageType.STATUS);
			else if (count == 1)
				sWelcome += LM.instance.getMessage("statCenterW3", LM.MessageType.STATUS);
			else
				sWelcome += String.Format(LM.instance.getMessage("statCenterW4", LM.MessageType.STATUS), count);

			txbStatusCenter.Text = sWelcome;

			if (count <= 0)
			{
				return;
			}

			int pluginActive = 0;
			for (int i = 0; i < count; i++)
			{
				if (PluginManager.instance.checkAppVersion(i, APP_VERSION))
				{
					MenuItem mit = new MenuItem();
					mit.Header = PluginManager.instance.getPluginMenuEntry(i);
					mit.ToolTip = PluginManager.instance.getPluginLangDesc(i, LM.instance.Language) + " (V" + PluginManager.instance.getPluginVersoin(i).ToString() + ")";
					mit.Click += MenuItem_Click;
					mit.Tag = i;

					mitExport.Items.Add(mit);
					pluginActive++;
				}
			}

			if (pluginActive <= 0)
			{
				mitExport.Visibility = Visibility.Collapsed;
				sepExport.Visibility = Visibility.Collapsed;
			}

		}



		private void setText()
		{
			// MenuItems File
			LM.instance.setElementText(mitFile, LM.MessageType.MENU);
			LM.instance.setElementText(mitNew, LM.MessageType.MENU);
			LM.instance.setElementText(mitOpen, LM.MessageType.MENU);
			LM.instance.setElementText(mitSave, LM.MessageType.MENU);
			LM.instance.setElementText(mitSaveAs, LM.MessageType.MENU);
			LM.instance.setElementText(mitExport, LM.MessageType.MENU);
			LM.instance.setElementText(mitOptions, LM.MessageType.MENU);
			LM.instance.setElementText(mitClose, LM.MessageType.MENU);
			// MenuItems Map
			LM.instance.setElementText(mitMap, LM.MessageType.MENU);
			LM.instance.setElementText(mitMapInfo, LM.MessageType.MENU);
			LM.instance.setElementText(mitMapProperty, LM.MessageType.MENU);
			// MenuItem Info
			LM.instance.setElementText(mitInfo, LM.MessageType.MENU);
			LM.instance.setElementText(mitAbout, LM.MessageType.MENU);
			LM.instance.setElementText(mitHelp, LM.MessageType.MENU);

			// Groupbox UI
			LM.instance.setElementText(grbMap, LM.MessageType.UI);
			LM.instance.setElementText(grbTile, LM.MessageType.UI);
			// TabItems UI
			LM.instance.setElementText(taiTerrain, LM.MessageType.UI);
			LM.instance.setElementText(taiRessource, LM.MessageType.UI);
			LM.instance.setElementText(taiDecal, LM.MessageType.UI);
			LM.instance.setElementText(taiModify, LM.MessageType.UI);
		}



		private void startExport(int plugin)
		{
			Util.showInfo();

			sbiRight.Foreground = Brushes.Red;
			sbiRight.Background = null;
			txbStatusRight.FontWeight = FontWeights.Bold;
			txbStatusRight.Text = LM.instance.getMessage("statRightEB", LM.MessageType.STATUS);

			pgbAdvantage.Visibility = System.Windows.Visibility.Visible;
			pgbAdvantage.Value = 0;
			pgbAdvantage.Minimum = 0;
			pgbAdvantage.Maximum = 100;
			
			m_bDoingExport = true;

			m_Timer.Interval = TimeSpan.FromMilliseconds(300);
			m_Timer.Tag = "EXPORT";
			m_Timer.Start();
			
			PluginContracts.Export.SExportData args = new PluginContracts.Export.SExportData();
			args.witch = plugin;
			
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += new DoWorkEventHandler(doExport);
			worker.ProgressChanged += new ProgressChangedEventHandler(progressExport);
			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(endExport);
			worker.WorkerReportsProgress = true;
			worker.RunWorkerAsync(args);
		}

		private void doExport(object sender, DoWorkEventArgs e)
		{
			Util.showInfo();
			PluginContracts.Export.SExportData args = (PluginContracts.Export.SExportData)e.Argument;
			args.worker = sender as BackgroundWorker;
			args.args = e;
			args.times = 70;
			PluginManager.instance.callExport(args.witch, args);
		}

		private void progressExport(object sender, ProgressChangedEventArgs e)
		{
			Util.showInfo();
			pgbAdvantage.Value = e.ProgressPercentage;
		}

		private void endExport(object sender, RunWorkerCompletedEventArgs e)
		{
			Util.showInfo();

			if ((CKResult)e.Result != CKResult.CK_OK)
			{
				sbiRight.Foreground = Brushes.Black;
				sbiRight.Background = Brushes.Red;
				txbStatusRight.Text = LM.instance.getMessage("statRightEE1", LM.MessageType.STATUS);
			}
			else
			{
				sbiRight.Foreground = Brushes.Black;
				sbiRight.Background = null;
				txbStatusRight.FontWeight = FontWeights.Normal;
				txbStatusRight.Text = LM.instance.getMessage("statRightEE2", LM.MessageType.STATUS);
			}

			m_bDoingExport = false;
			m_Timer.Stop();
			m_Timer.Tag = null;

			sbiProgress.Background = null;
			pgbAdvantage.Visibility = System.Windows.Visibility.Hidden;
			pgbAdvantage.Value = 0;
		}



		private void onMouseDown(object sender, MouseButtonEventArgs e)
		{
			Util.showInfo();

			Point p = e.GetPosition(grdMap);
			m_iPosX = (int)p.X / TILE_SIZE;
			m_iPosY = (int)p.Y / TILE_SIZE;

			txbCursorX.Text = String.Format("X: {0,4:0000}", m_iPosX);
			txbCursorY.Text = String.Format("{0,4:0000} :Y", m_iPosY);
		}


	}
}
