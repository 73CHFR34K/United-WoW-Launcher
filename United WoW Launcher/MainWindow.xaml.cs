using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace United_WoW_Launcher
{
    public partial class MainWindow : Window, IComponentConnector
    {
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);

            InitializeComponent();
        }
        private static void DoWork()
        {
            new Program().Update();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Thread(new ThreadStart(MainWindow.DoWork)).Start();
            timer.Start();
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            Program.text = (sender as TextBlock);
        }

        private void PBar_Loaded(object sender, RoutedEventArgs e)
        {
            Program.pbar = (sender as ProgressBar);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Program.pbar.IsIndeterminate = false;
        }

    }
}
