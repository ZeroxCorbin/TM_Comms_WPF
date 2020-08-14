using System;
using System.Windows;
using System.Windows.Threading;
using SocketManagerNS;

namespace TM_Comms_WPF.Net
{
    /// <summary>
    /// Interaction logic for Port8080Window.xaml
    /// </summary>
    public partial class Port8080Window : Window
    {
        SocketManager monitorSoc;

        private TM_Monitor.Rootobject _data;
        public TM_Monitor.Rootobject data
        {
            get { return _data; }
            set { _data = value; }
        }
        
        public Port8080Window()
        {
            InitializeComponent();

            txtMonitorConnectionString.Text = $"{App.Settings.RobotIP}:8080"; 
        }

        private void btnConnectMonitor_Click(object sender, RoutedEventArgs e)
        {
            MonitorSoc_Close();

            if (btnConnectMonitor.Tag == null)
            {
                monitorSoc = new SocketManager($"{App.Settings.RobotIP}:8080");

                if (monitorSoc.Connect())
                {
                    monitorSoc.DataReceived += MonitorSoc_DataReceived;
                    monitorSoc.ConnectState += MonitorSoc_ConnectState;
                     
                    monitorSoc.ReceiveAsync();

                    btnConnectMonitor.Content = "Stop";
                    btnConnectMonitor.Tag = 1;
                }
            }
            else
            {
                btnConnectMonitor.Content = "Start";
                btnConnectMonitor.Tag = null;
            }

        }
        private void MonitorSoc_ConnectState(object sender, bool data)
        {
            if (!data)
            {
            MonitorSoc_Close();

            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        btnConnectMonitor.Content = "Start";
                        btnConnectMonitor.Tag = null;
                    }));
            }

        }
        private void MonitorSoc_Close()
        {
            if (monitorSoc != null)
            {
                monitorSoc.DataReceived -= MonitorSoc_DataReceived;
                monitorSoc.ConnectState -= MonitorSoc_ConnectState;

                monitorSoc.StopReceiveAsync();
                monitorSoc.Close();
            }

        }
        private void MonitorSoc_DataReceived(object sender, string data)
        {
            string msg = CleanMessage(data);
            if (msg != "")
                this.data = TM_Monitor.Parse(msg);
            if (this.data != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<TM_Monitor.Rootobject, string>(MonitorViewUpdate), this.data, data);
            }
        }
        private string CleanMessage(string msg)
        {
            string[] str = msg.Split('?');

            foreach (string s in str)
            {
                if (s.StartsWith("{\"DataType\""))
                    if (s.EndsWith("}"))
                        return s;
            }
            return "";

        }

        private void MonitorViewUpdate(TM_Monitor.Rootobject data, string msg)
        {
            txtMonitorDataType.Text = data.DataType.ToString();
            txtMonitorResults.Text = msg;

            if (data.DataType == 1300)
            {
                txtMonitorRobot_Base.Text = data._Data.CurrentBaseName;
                txtMonitorRobot_X.Text = (string)data._Data.RobotPoint1[0].ToString();
                txtMonitorRobot_Y.Text = (string)data._Data.RobotPoint1[1].ToString();
                txtMonitorRobot_Z.Text = (string)data._Data.RobotPoint1[2].ToString();

                txtMonitorRobot_RX.Text = (string)data._Data.RobotPoint1[3].ToString();
                txtMonitorRobot_RY.Text = (string)data._Data.RobotPoint1[4].ToString();
                txtMonitorRobot_RZ.Text = (string)data._Data.RobotPoint1[5].ToString();

                txtMonitorRobot_Xa.Text = (string)data._Data.RobotPoint1[6].ToString();
                txtMonitorRobot_Ya.Text = (string)data._Data.RobotPoint1[7].ToString();
                txtMonitorRobot_Za.Text = (string)data._Data.RobotPoint1[8].ToString();

                txtMonitorRobot_RXa.Text = (string)data._Data.RobotPoint1[9].ToString();
                txtMonitorRobot_RYa.Text = (string)data._Data.RobotPoint1[10].ToString();
                txtMonitorRobot_RZa.Text = (string)data._Data.RobotPoint1[11].ToString();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            monitorSoc?.StopReceiveAsync();
            monitorSoc?.Close();
        }
    }
}
