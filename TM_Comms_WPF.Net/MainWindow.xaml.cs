using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using ApplicationSettingsNS;

namespace TM_Comms_WPF.Net
{
    public static class StringExtensions
    {
        public static bool ToBoolean(this string value)
        {
            switch (value.ToLower())
            {
                case "true":
                    return true;
                case "t":
                    return true;
                case "1":
                    return true;
                case "0":
                    return false;
                case "false":
                    return false;
                case "f":
                    return false;
                default:
                    throw new InvalidCastException("You can't cast that value to a bool!");
            }
        }

        public static int ToInt(this string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            return Convert.ToInt32(value);
        }
    }

    public partial class MainWindow : Window
    {

        private ListenNodeWindow listenNodeWindow = null;
        private Port8080Window port8080Window = null;
        private EthernetSlaveWindow ethernetSlaveWindow = null;
        private ModbusWindow modbusWindow = null;

        public MainWindow()
        {

            InitializeComponent();
            
            txtRobotIP.Text = App.Settings.RobotIP;
        }

  

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            port8080Window?.Close();
            ethernetSlaveWindow?.Close();
            modbusWindow?.Close();
            listenNodeWindow?.Close();

            ApplicationSettings_Serializer.Save("appsettings.xml", App.Settings);
        }

        private void btnListenNodeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (listenNodeWindow == null)
            {
                listenNodeWindow = new ListenNodeWindow();
                listenNodeWindow.Closing += ModbusWindow_Closing;
                listenNodeWindow.Owner = this;
                listenNodeWindow.Show();
            }
            listenNodeWindow.BringIntoView();
        }
        private void ListenNodeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listenNodeWindow = null;
        }

        private void btnEthernetSlaveWindow_Click(object sender, RoutedEventArgs e)
        {
            if (ethernetSlaveWindow == null)
            {
                ethernetSlaveWindow = new EthernetSlaveWindow();
                ethernetSlaveWindow.Closing += ModbusWindow_Closing;
                ethernetSlaveWindow.Owner = this;
                ethernetSlaveWindow.Show();
            }
            ethernetSlaveWindow.BringIntoView();
        }
        private void EthernetSlaveWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ethernetSlaveWindow = null;
        }

        private void btnPort8080Window_Click(object sender, RoutedEventArgs e)
        {
            if (port8080Window == null)
            {
                port8080Window = new Port8080Window();
                port8080Window.Closing += ModbusWindow_Closing;
                port8080Window.Owner = this;
                port8080Window.Show();
            }
            port8080Window.BringIntoView();
        }
        private void Port8080Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            port8080Window = null; ;
        }

        private void btnModbusWindow_Click(object sender, RoutedEventArgs e)
        {
            if (modbusWindow == null)
            {
                modbusWindow = new ModbusWindow();
                modbusWindow.Closing += ModbusWindow_Closing;
                modbusWindow.Owner = this;
                modbusWindow.Show();
            }
            modbusWindow.BringIntoView();
            
        }
        private void ModbusWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            modbusWindow = null;
        }      
        
        private void txtRobotIP_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!IPAddress.TryParse(txtRobotIP.Text, out IPAddress ip))
            {
                txtRobotIP.Background = Brushes.LightSalmon;
                return;
            }

            App.Settings.RobotIP = ip.ToString();

            txtRobotIP.Background = Brushes.LightGreen;
        }


    }
}
