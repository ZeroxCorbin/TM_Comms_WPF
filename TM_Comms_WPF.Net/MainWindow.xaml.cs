using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using ApplicationSettingsNS;

namespace TM_Comms_WPF
{
    public partial class MainWindow : Window
    {

        private ListenNodeWindow ListenNodeWindow { get; set; } = null;
        private Port8080Window Port8080Window { get; set; } = null;
        private EthernetSlaveWindow EthernetSlaveWindow { get; set; } = null;
        private ModbusWindow ModbusWindow { get; set; } = null;

        private bool IsLoading { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
            
            txtRobotIP.Text = App.Settings.RobotIP;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                App.Settings.MainWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();
                App.Settings.ModbusWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();
                App.Settings.ListenNodeWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();
                App.Settings.EthernetSlaveWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();
                App.Settings.Port8080Window = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();
            }

            this.Left = App.Settings.MainWindow.Left;
            this.Top = App.Settings.MainWindow.Top;

            IsLoading = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Port8080Window?.Close();
            EthernetSlaveWindow?.Close();
            ModbusWindow?.Close();
            ListenNodeWindow?.Close();

            ApplicationSettings_Serializer.Save("appsettings.xml", App.Settings);
        }

        private void BtnListenNodeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (ListenNodeWindow == null)
            {
                ListenNodeWindow = new ListenNodeWindow();
                ListenNodeWindow.Closing += ListenNodeWindow_Closing;
                ListenNodeWindow.Owner = this;
                ListenNodeWindow.Show();
            }
        }
        private void ListenNodeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ListenNodeWindow = null;
        }

        private void BtnEthernetSlaveWindow_Click(object sender, RoutedEventArgs e)
        {
            if (EthernetSlaveWindow == null)
            {
                EthernetSlaveWindow = new EthernetSlaveWindow();
                EthernetSlaveWindow.Closing += EthernetSlaveWindow_Closing;
                EthernetSlaveWindow.Owner = this;
                EthernetSlaveWindow.Show();
            }
        }
        private void EthernetSlaveWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EthernetSlaveWindow = null;
        }

        private void BtnPort8080Window_Click(object sender, RoutedEventArgs e)
        {
            if (Port8080Window == null)
            {
                Port8080Window = new Port8080Window();
                Port8080Window.Closing += Port8080Window_Closing;
                Port8080Window.Owner = this;
                Port8080Window.Show();
            }
        }
        private void Port8080Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Port8080Window = null; ;
        }

        private void BtnModbusWindow_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusWindow == null)
            {
                ModbusWindow = new ModbusWindow();
                ModbusWindow.Closing += ModbusWindow_Closing;
                ModbusWindow.Activated += AnyWindow_Activated;
                ModbusWindow.Owner = this;
                ModbusWindow.Show();
            }
        }

        private void AnyWindow_Activated(object sender, EventArgs e)
        {
                MoveToForeground.DoOnProcess("TM_Comms_WPF");
        }

        private void ModbusWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ModbusWindow = null;
        }      
        
        private void TxtRobotIP_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!IPAddress.TryParse(txtRobotIP.Text, out IPAddress ip))
            {
                txtRobotIP.Background = Brushes.LightSalmon;
                return;
            }

            App.Settings.RobotIP = ip.ToString();

            txtRobotIP.Background = Brushes.LightGreen;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoading) return;

            App.Settings.MainWindow.Top = Top;
            App.Settings.MainWindow.Left = Left;
        }
    }
}
