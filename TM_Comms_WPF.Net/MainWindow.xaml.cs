using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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
using System.Windows.Threading;
using clsSocketNS;
using ApplicationSettingsNS;

namespace TM_Comms_WPF.Net
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        clsSocket monitorSoc;
        clsSocket listenNodeSoc;
        TM_Comms_ModbusTCP modbusSoc;
        TM_Comms_ModbusDict modbusDictionary;

        TM_Comms_ListenNode listenNode;

        ApplicationSettings_Serializer.ApplicationSettings appSettings;

        private TM_Monitor.Rootobject _data;
        public TM_Monitor.Rootobject data
        {
            get { return _data; }
            set { _data = value; }
        }

        public MainWindow()
        {
            appSettings = ApplicationSettings_Serializer.Load("appsettings.xml");

            InitializeComponent();

            modbusDictionary = new TM_Comms_ModbusDict();
            cmbModbusRegisters.ItemsSource = modbusDictionary.MobusData.Keys;

            listenNode = GetNode();

            txtListenNodeConnectionString.Text = appSettings.ListenNodeConnectionString;
            txtMonitorConnectionString.Text = appSettings.MonitorConnectionString;
            txtModbusIP.Text = appSettings.ModbusIP;
        }

        private TM_Comms_ListenNode GetNode()
        {
            TM_Comms_ListenNode node = new TM_Comms_ListenNode();
            if (cmbDataType.SelectedIndex == 0)
            {
                node.Header = TM_Comms_ListenNode.HEADERS.TMSCT;

                txtScriptID.Text = node.ScriptID.ToString();
            }
            else
            {
                node.Header = TM_Comms_ListenNode.HEADERS.TMSTA;
                txtScriptID.Text = "";
            }


            node.Data = txtScriptData.Text;

            txtDataString.Text = node.Message;

            return node;
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

        private void btnConnectListenNode_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnectListenNode.Tag == null)
            {
                ListenNodeSoc_Close();

                listenNodeSoc = new clsSocket(txtListenNodeConnectionString.Text);
                if (listenNodeSoc.Connect(true))
                {
                    listenNodeSoc.DataReceived += ListenNodeSoc_DataReceived;
                    listenNodeSoc.Closed += ListenNodeSoc_Closed;

                    listenNodeSoc.StartRecieveAsync();

                    btnConnectListenNode.Content = "Stop";
                    btnConnectListenNode.Tag = 1;

                    appSettings.ListenNodeConnectionString = txtListenNodeConnectionString.Text;
                }
            }
            else
            {
                ListenNodeSoc_Close();

                btnConnectListenNode.Content = "Start";
                btnConnectListenNode.Tag = null;
            }
        }
        private void ListenNodeSoc_Closed(object sender, clsSocket.clsSocketEventArgs data)
        {
            ListenNodeSoc_Close();

            var disp = this.Dispatcher;/* Get the UI dispatcher, each WPF object has a dispatcher which you can query*/
            disp.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                        txtDataResponse.Text = "";

                        btnConnectListenNode.Content = "Start";
                        btnConnectListenNode.Tag = null;
                    }));
        }
        private void ListenNodeSoc_Close()
        {
            if (listenNodeSoc != null)
            {
                listenNodeSoc.DataReceived -= ListenNodeSoc_DataReceived;
                listenNodeSoc.Closed -= ListenNodeSoc_Closed;

                listenNodeSoc.StopRecieveAsync();
                listenNodeSoc.Disconnect();
            }

        }
        private void ListenNodeSoc_DataReceived(object sender, clsSocket.clsSocketEventArgs data)
        {

            var disp = this.Dispatcher;/* Get the UI dispatcher, each WPF object has a dispatcher which you can query*/
            disp.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                        txtDataResponse.Text = data.Message;
                    }));
        }
        private void btnSendListenNode_Click(object sender, RoutedEventArgs e)
        {
            rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            listenNodeSoc?.Write(listenNode.Message);
        }

        private void btnConnectMonitor_Click(object sender, RoutedEventArgs e)
        {
            MonitorSoc_Close();

            if (btnConnectMonitor.Tag == null)
            {
                monitorSoc = new clsSocket(txtMonitorConnectionString.Text);

                if (monitorSoc.Connect(true))
                {
                    monitorSoc.DataReceived += MonitorSoc_DataReceived;
                    monitorSoc.Closed += MonitorSoc_Closed;

                    monitorSoc.StartRecieveAsync();

                    btnConnectMonitor.Content = "Stop";
                    btnConnectMonitor.Tag = 1;

                    appSettings.ListenNodeConnectionString = txtMonitorConnectionString.Text;
                }
            }
            else
            {
                btnConnectMonitor.Content = "Start";
                btnConnectMonitor.Tag = null;
            }

        }
        private void MonitorSoc_Closed(object sender, clsSocket.clsSocketEventArgs data)
        {
            MonitorSoc_Close();

            var disp = this.Dispatcher;/* Get the UI dispatcher, each WPF object has a dispatcher which you can query*/
            disp.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        btnConnectMonitor.Content = "Start";
                        btnConnectMonitor.Tag = null;
                    }));
        }
        private void MonitorSoc_Close()
        {
            if (monitorSoc != null)
            {
                monitorSoc.DataReceived -= MonitorSoc_DataReceived;
                monitorSoc.Closed -= MonitorSoc_Closed;

                monitorSoc.StopRecieveAsync();
                monitorSoc.Disconnect();
            }

        }
        private void MonitorSoc_DataReceived(object sender, clsSocket.clsSocketEventArgs data)
        {
            string msg = CleanMessage(data.Message);
            if (msg != "")
                this.data = TM_Monitor.Parse(msg);
            if (this.data != null)
            {
                var disp = this.Dispatcher;/* Get the UI dispatcher, each WPF object has a dispatcher which you can query*/
                disp.BeginInvoke(DispatcherPriority.Render, new Action<TM_Monitor.Rootobject, string>(MonitorViewUpdate), this.data, data.Message);
            }
        }
        private void MonitorViewUpdate(TM_Monitor.Rootobject data, string msg)
        {
            txtMonitorDataType.Text = data.DataType.ToString();
            txtMonitorPoolID.Text = msg.Length.ToString();
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

        private void txtScriptData_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (listenNode != null)
            {
                listenNode.Data = txtScriptData.Text;
                txtDataString.Text = listenNode.Message;
            }
        }

        private void cmbDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listenNode = GetNode();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ApplicationSettings_Serializer.Save("appsettings.xml", appSettings);

            monitorSoc?.StopRecieveAsync();
            monitorSoc?.Disconnect();

            listenNodeSoc?.StopRecieveAsync();
            listenNodeSoc?.Disconnect();

            modbusSoc?.Disconnect();
        }


        //float test = modbusSoc.GetFloat(0x1B71);
        //bool test1 = modbusSoc.GetBool(0x1C22);
        //bool test2 = modbusSoc.GetBool(0x1C28);
        //int speed = modbusSoc.GetInt16(0x1C85);
        //int error = modbusSoc.GetInt32(0x1C98);
        private void btnConnectModbus_Click(object sender, RoutedEventArgs e)
        {
            ModbusClose();

            if (btnConnectModbus.Tag == null)
            {
                modbusSoc = new TM_Comms_ModbusTCP();

                if (modbusSoc.Connect(txtModbusIP.Text))
                {

                    btnConnectModbus.Content = "Stop";
                    btnConnectModbus.Tag = 1;

                    appSettings.ModbusIP = txtModbusIP.Text;
                }
            }
            else
            {
                btnConnectModbus.Content = "Start";
                btnConnectModbus.Tag = null;
            }

        }

        private void ModbusClose()
        {
            if (modbusSoc != null)
            {
                modbusSoc.Disconnect();
            }
        }

        private void btnModbusRead_Click(object sender, RoutedEventArgs e)
        {
            if (modbusSoc != null)
            {
                switch (modbusDictionary.MobusData[(string)cmbModbusRegisters.SelectedItem].Type)
                {
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                        txtModbusReadResult.Text = modbusSoc.GetBool(modbusDictionary.MobusData[(string)cmbModbusRegisters.SelectedItem].Addr).ToString();

                    break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:
                        txtModbusReadResult.Text = modbusSoc.GetFloat(modbusDictionary.MobusData[(string)cmbModbusRegisters.SelectedItem].Addr).ToString();
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                        txtModbusReadResult.Text = modbusSoc.GetInt16(modbusDictionary.MobusData[(string)cmbModbusRegisters.SelectedItem].Addr).ToString();
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:
                        txtModbusReadResult.Text = modbusSoc.GetInt32(modbusDictionary.MobusData[(string)cmbModbusRegisters.SelectedItem].Addr).ToString();
                        break;
                    //case TM_Comms_ModbusDict.MobusValue.DataTypes.String:
                    //    txtModbusReadResult.Text = modbusSoc.ge(modbusDictionary.MobusData[(string)cmbModbusRegisters.SelectedItem].Addr).ToString();
                    //    break;
                }


            }
        }
    }
}
