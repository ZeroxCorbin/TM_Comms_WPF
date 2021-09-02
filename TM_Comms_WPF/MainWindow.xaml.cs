using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using ApplicationSettingsNS;
using TM_Comms;

namespace TM_Comms_WPF
{
    public partial class MainWindow : Window
    {
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
                App.Settings.ExternalVisionWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();
                App.Settings.Port8080Window = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();
            }

            if (double.IsNaN(App.Settings.MainWindow.Left))
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            else
            {
                this.Left = App.Settings.MainWindow.Left;
                this.Top = App.Settings.MainWindow.Top;

                if (!CheckOnScreen.IsOnScreen(this))
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

           
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (WindowStartupLocation == WindowStartupLocation.CenterScreen)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                this.Top /= 2;
            }
         }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if(!IsLoaded) return;

            App.Settings.MainWindow.Top = Top;
            App.Settings.MainWindow.Left = Left;
        }

        private bool IPValid { get; set; } = false;
        private void TxtRobotIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(txtRobotIP.Text, @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"))
            {
                IPValid = false;
                txtRobotIP.Background = Brushes.LightSalmon;
                return;
            }
            if (!IPAddress.TryParse(txtRobotIP.Text, out IPAddress ip))
            {
                IPValid = false;
                txtRobotIP.Background = Brushes.LightSalmon;
                return;
            }

            IPValid = true;
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
                    btnExternalVisionWindow.IsEnabled = false;
                    break;
                case TMflowVersions.V1_72_xxxx:
                    btnPort8080Window.IsEnabled = true;
                    btnEthernetSlaveWindow.IsEnabled = false;
                    btnExternalVisionWindow.IsEnabled = false;
                    break;
                case TMflowVersions.V1_76_xxxx:
                    btnPort8080Window.IsEnabled = true;
                    btnEthernetSlaveWindow.IsEnabled = true;
                    btnExternalVisionWindow.IsEnabled = false;
                    break;
                case TMflowVersions.V1_80_xxxx:
                    btnPort8080Window.IsEnabled = false;
                    btnEthernetSlaveWindow.IsEnabled = true;
                    btnExternalVisionWindow.IsEnabled = true;
                    break;
                case TMflowVersions.V1_82_xxxx:
                    btnPort8080Window.IsEnabled = false;
                    btnEthernetSlaveWindow.IsEnabled = true;
                    btnExternalVisionWindow.IsEnabled = true;
                    break;
            }

            if (!IsLoaded) return;

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
                ExternalVisionWindow == null &&
                ModbusWindow == null)
            {
                CmbSystemVersions.IsEnabled = true;
                txtRobotIP.IsEnabled = true;
            }
        }

        private ModbusWindow ModbusWindow { get; set; } = null;
        private void BtnModbusWindow_Click(object sender, RoutedEventArgs e)
        {
            if(!IPValid)
            {
                return;
            }
            if(ModbusWindow == null)
            {
                ModbusWindow = new ModbusWindow(this);
                ModbusWindow.Closed += ModbusWindow_Closed;
                ModbusWindow.Activated += AnyWindow_Activated;
                ModbusWindow.Show();

                ModbusWindow.Owner = null;

                WindowShow();
            }
            else if(ModbusWindow.WindowState == WindowState.Minimized)
            {
                ModbusWindow.WindowState = App.Settings.ModbusWindow.WindowState;
            }
            else
            {
                ModbusWindow.Focus();
            }
        }
        private void ModbusWindow_Closed(object sender, EventArgs e)
        {
            ModbusWindow = null;
            WindowClose();
        }

        private ListenNodeWindow ListenNodeWindow { get; set; } = null;
        private void BtnListenNodeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (!IPValid)
            {
                return;
            }
            if (ListenNodeWindow == null)
            {
                ListenNodeWindow = new ListenNodeWindow(this);
                ListenNodeWindow.Closed += ListenNodeWindow_Closed;
                ListenNodeWindow.Activated += AnyWindow_Activated;
                ListenNodeWindow.Show();

                ListenNodeWindow.Owner = null;

                WindowShow();
            }
            else if(ListenNodeWindow.WindowState == WindowState.Minimized)
            {
                ListenNodeWindow.WindowState = App.Settings.ListenNodeWindow.WindowState;
            }
            else
            {
                ListenNodeWindow.Focus();
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
            if (!IPValid)
            {
                return;
            }
            if (EthernetSlaveWindow == null)
            {
                EthernetSlaveWindow = new EthernetSlaveWindow(this);
                EthernetSlaveWindow.Closed += EthernetSlaveWindow_Closed;
                EthernetSlaveWindow.Activated += AnyWindow_Activated;
                EthernetSlaveWindow.Show();

                EthernetSlaveWindow.Owner = null;

                WindowShow();
            }
            else if(EthernetSlaveWindow.WindowState == WindowState.Minimized)
            {
                EthernetSlaveWindow.WindowState = App.Settings.EthernetSlaveWindow.WindowState;
            }
            else
            {
                EthernetSlaveWindow.Focus();
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
            if (!IPValid)
            {
                return;
            }
            if (Port8080Window == null)
            {
                Port8080Window = new Port8080Window();
                Port8080Window.Closed += Port8080Window_Closed;
                Port8080Window.Activated += AnyWindow_Activated;
                Port8080Window.Owner = this;
                Port8080Window.Show();

                Port8080Window.Owner = null;

                WindowShow();
            }
            else if(Port8080Window.WindowState == WindowState.Minimized)
            {
                Port8080Window.WindowState = App.Settings.Port8080Window.WindowState;
            }
            else
            {
                Port8080Window.Focus();
            }
        }
        private void Port8080Window_Closed(object sender, EventArgs e)
        {
            Port8080Window = null;
            WindowClose();
        }

        private ExternalVisionWindow ExternalVisionWindow { get; set; } = null;
        private void BtnExternalVisionWindow_Click(object sender, RoutedEventArgs e)
        {
            if(!IPValid)
            {
                return;
            }
            if(ExternalVisionWindow == null)
            {
                ExternalVisionWindow = new ExternalVisionWindow(this);
                ExternalVisionWindow.Closed += ExternalVisionWindow_Closed;
                ExternalVisionWindow.Activated += AnyWindow_Activated;
                ExternalVisionWindow.Show();

                ExternalVisionWindow.Owner = null;

                WindowShow();
            }
            else if(ExternalVisionWindow.WindowState == WindowState.Minimized)
            {
                ExternalVisionWindow.WindowState = App.Settings.ExternalVisionWindow.WindowState;
            }
            else
            {
                ExternalVisionWindow.Focus();
            }

        }
        private void ExternalVisionWindow_Closed(object sender, EventArgs e)
        {
            ExternalVisionWindow = null;
            WindowClose();
        }

        private void AnyWindow_Activated(object sender, EventArgs e) => MoveToForeground.DoOnProcess("TM_Comms_WPF");


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Port8080Window?.Close();
            EthernetSlaveWindow?.Close();
            ModbusWindow?.Close();
            ListenNodeWindow?.Close();
            ExternalVisionWindow?.Close();
        }

    }
}
