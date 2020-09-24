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
        private bool IsLoading { get; set; } = true;
        public MainWindow()
        {
            InitializeComponent();

            txtRobotIP.Text = App.Settings.RobotIP;

            CmbSystemVersions.ItemsSource = Enum.GetValues(typeof(TMflowVersions));
            CmbSystemVersions.SelectedItem = App.Settings.Version;

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

            if (!CheckOnScreen.IsOnScreen(this))
            {
                App.Settings.MainWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();

                this.Left = App.Settings.MainWindow.Left;
                this.Top = App.Settings.MainWindow.Top;
            }

            IsLoading = false;
        }

        private void TxtRobotIP_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!IPAddress.TryParse(txtRobotIP.Text, out IPAddress ip))
            {
                txtRobotIP.Background = Brushes.LightSalmon;
                return;
            }

            App.Settings.RobotIP = ip.ToString();

            txtRobotIP.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
        }
        private void CmbSystemVersions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch ((TMflowVersions)CmbSystemVersions.SelectedItem)
            {
                case TMflowVersions.V1_68_xxxx:
                    btnPort8080Window.IsEnabled = true;
                    btnEthernetSlaveWindow.IsEnabled = false;
                    break;
                case TMflowVersions.V1_76_xxxx:
                    btnPort8080Window.IsEnabled = true;
                    btnEthernetSlaveWindow.IsEnabled = true;
                    break;
                case TMflowVersions.V1_80_xxxx:
                    btnPort8080Window.IsEnabled = false;
                    btnEthernetSlaveWindow.IsEnabled = true;
                    break;
            }

            if (IsLoading) return;

            App.Settings.Version = (TMflowVersions)CmbSystemVersions.SelectedItem;
        }

        private void WindowShow()
        {
            CmbSystemVersions.IsEnabled = false;
            txtRobotIP.IsEnabled = false;
        }
        private void WindowClose()
        {
            if (ListenNodeWindow == null &&
                Port8080Window == null &&
                EthernetSlaveWindow == null &&
                ModbusWindow == null)
            {
                CmbSystemVersions.IsEnabled = true;
                txtRobotIP.IsEnabled = true;
            }
        }

        private ListenNodeWindow ListenNodeWindow { get; set; } = null;
        private void BtnListenNodeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (ListenNodeWindow == null)
            {
                ListenNodeWindow = new ListenNodeWindow();
                ListenNodeWindow.Closed += ListenNodeWindow_Closed;
                ListenNodeWindow.Activated += AnyWindow_Activated;
                ListenNodeWindow.Owner = this;
                ListenNodeWindow.Show();

                WindowShow();
            }
        }
        private void ListenNodeWindow_Closed(object sender, EventArgs e)
        {
            ListenNodeWindow = null;
            WindowClose();
        }
        private EthernetSlaveWindow EthernetSlaveWindow { get; set; } = null;
        private void BtnEthernetSlaveWindow_Click(object sender, RoutedEventArgs e)
        {
            if (EthernetSlaveWindow == null)
            {
                EthernetSlaveWindow = new EthernetSlaveWindow();
                EthernetSlaveWindow.Closed += EthernetSlaveWindow_Closed;
                EthernetSlaveWindow.Activated += AnyWindow_Activated;
                EthernetSlaveWindow.Owner = this;
                EthernetSlaveWindow.Show();

                WindowShow();
            }
        }
        private void EthernetSlaveWindow_Closed(object sender, EventArgs e)
        {
            EthernetSlaveWindow = null;
            WindowClose();
        }
        private Port8080Window Port8080Window { get; set; } = null;
        private void BtnPort8080Window_Click(object sender, RoutedEventArgs e)
        {
            if (Port8080Window == null)
            {
                Port8080Window = new Port8080Window();
                Port8080Window.Closed += Port8080Window_Closed;
                Port8080Window.Activated += AnyWindow_Activated;
                Port8080Window.Owner = this;
                Port8080Window.Show();

                WindowShow();
            }
        }
        private void Port8080Window_Closed(object sender, EventArgs e)
        {
            Port8080Window = null;
            WindowClose();
        }
        private ModbusWindow ModbusWindow { get; set; } = null;
        private void BtnModbusWindow_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusWindow == null)
            {
                ModbusWindow = new ModbusWindow();
                ModbusWindow.Closed += ModbusWindow_Closed;
                ModbusWindow.Activated += AnyWindow_Activated;
                ModbusWindow.Owner = this;
                ModbusWindow.Show();

                WindowShow();
            }
        }
        private void ModbusWindow_Closed(object sender, EventArgs e)
        {
            ModbusWindow = null;
            WindowClose();
        }

        private void AnyWindow_Activated(object sender, EventArgs e) => MoveToForeground.DoOnProcess("TM_Comms_WPF");

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoading) return;

            App.Settings.MainWindow.Top = Top;
            App.Settings.MainWindow.Left = Left;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Port8080Window?.Close();
            EthernetSlaveWindow?.Close();
            ModbusWindow?.Close();
            ListenNodeWindow?.Close();

            ApplicationSettings_Serializer.Save("appsettings.xml", App.Settings);
        }
    }
}
