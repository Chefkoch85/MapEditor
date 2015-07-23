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

using System.ComponentModel;

namespace MapEditor_Tilemap
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            txbCursorX.Text = "";
            txbCursorY.Text = "";
            txbStatusCenter.Text = "";
            txbStatusRight.Text = "";
        }

        private bool m_bDirty = false;
        private static readonly int TILE_SIZE = 64;

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() != typeof(MenuItem))
                return;

            MenuItem item = sender as MenuItem;

            if (item.Tag.ToString() == "tagExport")
            {
                startExport();
            }
            else if (item.Tag.ToString() == "tagNew")
            {
                int rows = 4, cols = 4;

                grdMap.ShowGridLines = true;
                grdMap.Width = cols * TILE_SIZE;
                grdMap.Height = rows * TILE_SIZE;
                grdMap.Background = Brushes.Transparent;
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


            }

        }

        int m_iPosX = 0;
        int m_iPosY = 0;
        void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("onMouseDown() : " + sender.GetType());

            Point p = e.GetPosition(grdMap);
            m_iPosX = (int)p.X / TILE_SIZE;
            m_iPosY = (int)p.Y / TILE_SIZE;

            txbCursorX.Text = String.Format("X: {0,4:0000}", m_iPosX);
            txbCursorY.Text = String.Format("{0,4:0000} :Y", m_iPosY);
        }

        bool m_bDoingExport = false;
        void startExport()
        {
            txbStatusRight.Foreground = Brushes.Red;
            txbStatusRight.Text = "doing Export, don't close app!";

            pgbAdvantage.Visibility = System.Windows.Visibility.Visible;
            pgbAdvantage.Value = 0;
            pgbAdvantage.Minimum = 0;
            pgbAdvantage.Maximum = 100;

            DateTime start = DateTime.Now;
            m_bDoingExport = true;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(doExport);
            worker.ProgressChanged += new ProgressChangedEventHandler(changedExport);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(endExport);
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync(start);
        }

        void doExport(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("doExport() : " + sender.GetType());
            BackgroundWorker w = sender as BackgroundWorker;

            int old = 0;
            const int MAX_CYCLE = 20000000;
            for (int i = 0; i < MAX_CYCLE; i++)
            {
                int percent = 100 * i / MAX_CYCLE + 1;
                if (percent > old)
                    w.ReportProgress(percent);
                old = percent;

                for (int j = 0; j < 50; j++)
                {
                    double sqr = Math.Sqrt(i);
                }
            }

            e.Result = e.Argument;
        }
        void changedExport(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("changedExport() : " + sender.GetType() + " : " + e.ProgressPercentage);
            pgbAdvantage.Value = e.ProgressPercentage;
        }
        void endExport(object sender, RunWorkerCompletedEventArgs e)
        {
            DateTime start = DateTime.Now;
            if(e.Result is DateTime)
                start = (DateTime)e.Result;

            TimeSpan ts = DateTime.Now - start;
            string time = "";
            if (ts.TotalSeconds <= 60)
                time = String.Format("{0,4:00.00}sec", ts.TotalSeconds);
            else
                time = String.Format("{0,2:00}m:{1,2:00}s:{2,3:000}ms", ts.Minutes, ts.Seconds, ts.Milliseconds);

            Debug.WriteLine("endExport() : " + sender.GetType());
            txbStatusRight.Foreground = Brushes.Black;
            txbStatusRight.Text = "Export done! Time used: " + time;

            m_bDoingExport = false;
            pgbAdvantage.Visibility = System.Windows.Visibility.Hidden;
            pgbAdvantage.Value = 0;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = AppClose();
        }

        private bool AppClose()
        {
            MessageBoxResult hr = MessageBoxResult.None;
            if (m_bDirty)
            {
                hr = MessageBox.Show("Unsaved data, want to save it now?", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if(hr == MessageBoxResult.Yes)

            }
            else if (m_bDoingExport)
            {
                hr = MessageBox.Show("Map data is exporting, sure you want to close?\nExporting will fail!", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }



            return false;
        }



    }
}
