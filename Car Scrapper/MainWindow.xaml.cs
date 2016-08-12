using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Car_Scrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow UI;
        //public static ManualResetEvent mre = new ManualResetEvent(true);
        Thread botThread;
        Thread botThread2;
        Thread botThread3;

        public MainWindow()
        {
            InitializeComponent();
            UI = this;
        }

        private void StatButton_Click(object sender, RoutedEventArgs e)
        {
            botThread = new Thread(Scrapper.botStuff);
            botThread.Start();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }
        public object GetPropertyValue(object kek, string propertyName)
        {
            return kek.GetType().GetProperties()
               .Single(pi => pi.Name == propertyName)
               .GetValue(kek, null);
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            //string ClipBoardText = Clipboard.GetText();
            SearchLink.Text = Clipboard.GetText();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchLink.Text = string.Empty;
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            string Path = AppDomain.CurrentDomain.BaseDirectory;
            Process.Start(Path);

        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Scrapper.mre.Reset();
            LogBox.AppendText("Pausing...\n");
            LogBox.Focus();
            LogBox.CaretIndex = MainWindow.UI.LogBox.Text.Length;
            LogBox.ScrollToEnd();
            LogBox2.AppendText("Pausing...\n");
            LogBox2.Focus();
            LogBox2.CaretIndex = MainWindow.UI.LogBox.Text.Length;
            LogBox2.ScrollToEnd();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            Scrapper.mre.Set();
            LogBox.AppendText("Resuming...\n");
            LogBox.Focus();
            LogBox.CaretIndex = MainWindow.UI.LogBox.Text.Length;
            LogBox.ScrollToEnd();
            LogBox2.AppendText("Resuming...\n");
            LogBox2.Focus();
            LogBox2.CaretIndex = MainWindow.UI.LogBox.Text.Length;
            LogBox2.ScrollToEnd();
        }
    }
}
