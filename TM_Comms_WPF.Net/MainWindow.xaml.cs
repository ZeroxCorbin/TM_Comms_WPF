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
    /// 
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
            return Convert.ToInt32(value);
        }
    }

    public partial class MainWindow : Window
    {
        clsSocket monitorSoc;
        clsSocket listenNodeSoc;
        TM_Comms_ModbusTCP modbusSoc;
        TM_Comms_ModbusDict modbusDictionary;

        TM_Comms_ListenNode listenNode;

        ApplicationSettings_Serializer.ApplicationSettings appSettings;
        bool isLoading = false;

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

            //Initialize the modbus register combo boxes with a refernce to the dictionary keys.
            //Assign the selection changed event and add an index value to the tag for use in the event.
            //These steps must be done first.
            int i = 0;
            foreach (ComboBox cmb in stackModbusComboBox.Children)
            {
                cmb.ItemsSource = modbusDictionary.MobusData.Keys;
                cmb.SelectionChanged += CmbModbusRegister_SelectionChanged;
                cmb.Tag = i++;
            }

            //Assign the click event and add an index value to the tag for use in the event. 
            i = 0;
            foreach (Button btn in stackModbusReadButton.Children)
            {
                btn.Click += BtnModbusRead_Click;
                btn.Tag = i++;
            }

            //Assign the click event and initially hide all the buttons.
            i = 0;
            foreach (Button btn in stackModbusWriteButton.Children)
            {
                btn.Click += BtnModbusWrite_Click;
                btn.Tag = i++;
                btn.IsEnabled = false;
                btn.Visibility = Visibility.Hidden;
            }

            //Set the modbus register combo boxes to the stored values.
            //This must be done last because all the event handling is setup.
            i = 0;
            foreach (string str in appSettings.ModbusComboBoxIndices)
                ((ComboBox)stackModbusComboBox.Children[i++]).SelectedValue = str;

            //Initialize the modbus user register combo boxes with a refernce to the dictionary keys.
            //Assign the selection changed event and add an index value to the tag for use in the event.
            //These steps must be done first.
            i = 0;
            foreach (ComboBox cmb in stackModbusUserComboBox.Children)
            {
                cmb.Items.Add("Bool");
                cmb.Items.Add("Int16");
                cmb.Items.Add("Int32");
                cmb.Items.Add("Float");
                cmb.Items.Add("String");

                cmb.SelectionChanged += CmbModbusUserRegister_SelectionChanged;
                cmb.Tag = i++;
            }

            //Assign the click event and add an index value to the tag for use in the event. 
            i = 0;
            foreach (Button btn in stackModbusUserReadButton.Children)
            {
                btn.Click += BtnModbusUserRead_Click;
                btn.Tag = i++;
            }

            //Assign the click event and add an index value to the tag for use in the event. 
            i = 0;
            foreach (Button btn in stackModbusUserWriteButton.Children)
            {
                btn.Click += BtnModbusUserWrite_Click;
                btn.Tag = i++;
            }

            isLoading = true;

            //Set the modbus register combo boxes to the stored values.
            //This must be done last because all the event handling is setup.
            i = 0;
            foreach (string str in appSettings.ModbusUserComboBoxIndices)
                ((ComboBox)stackModbusUserComboBox.Children[i++]).SelectedValue = str;

            isLoading = false;

            RecalcUserRegisters();

            listenNode = GetNode();

            txtListenNodeConnectionString.Text = appSettings.ListenNodeConnectionString;
            txtMonitorConnectionString.Text = appSettings.MonitorConnectionString;
            txtModbusIP.Text = appSettings.ModbusIP;
        }

        private void CmbModbusUserRegister_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = ((ComboBox)sender);
            cmb.Tag = Enum.Parse(typeof(TM_Comms_ModbusDict.MobusValue.DataTypes), (string)cmb.SelectedValue);

            if (!isLoading) RecalcUserRegisters();
        }

        private void RecalcUserRegisters()
        {
            int num = 9000;
            int i = 0;
            foreach (Label lbl in stackModbusUserLabels.Children)
            {
                lbl.Content = num.ToString();
                TM_Comms_ModbusDict.MobusValue.DataTypes val = (TM_Comms_ModbusDict.MobusValue.DataTypes)((ComboBox)stackModbusUserComboBox.Children[i++]).Tag;

                switch (val)
                {
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                        num = num + 1;
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:
                        num = num + 2;
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                        num = num + 1;
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:
                        num = num + 2;
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.String:
                        num = num + 32;
                        break;
                }
            }
        }

        private void CmbModbusRegister_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = ((ComboBox)sender);
            Button rbtn = (Button)stackModbusReadButton.Children[(int)cmb.Tag];
            Button wbtn = (Button)stackModbusWriteButton.Children[(int)cmb.Tag];

            rbtn.Content = modbusDictionary.MobusData[(string)cmb.SelectedValue].Addr.ToString();

            if (modbusDictionary.MobusData[(string)cmb.SelectedValue].Access == TM_Comms_ModbusDict.MobusValue.AccessTypes.R)
            {
                wbtn.IsEnabled = false;
                wbtn.Visibility = Visibility.Hidden;
            }
            else
            {
                wbtn.IsEnabled = true;
                wbtn.Visibility = Visibility.Visible;
            }
        }

        private void BtnModbusWrite_Click(object sender, RoutedEventArgs e)
        {
            if (modbusSoc == null) return;

            int i = (int)((Button)sender).Tag;
            string val = (String)((ComboBox)stackModbusComboBox.Children[i]).SelectedValue;
            int addr = modbusDictionary.MobusData[val].Addr;

            if (val == null) return;

            if (modbusSoc != null)
            {
                switch (modbusDictionary.MobusData[val].Type)
                {
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                        bool send = ((TextBox)stackModbusText.Children[i]).Text.ToBoolean();
                        modbusSoc.SetBool(addr, send);
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:

                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                        int data = ((TextBox)stackModbusText.Children[i]).Text.ToInt();
                        modbusSoc.SetInt16(addr, data);
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:

                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.String:

                        break;
                }
            }
        }

        private void BtnModbusRead_Click(object sender, RoutedEventArgs e)
        {
            if (modbusSoc == null) return;

            int i = (int)((Button)sender).Tag;
            string val = (String)((ComboBox)stackModbusComboBox.Children[i]).SelectedValue;
            int addr = modbusDictionary.MobusData[val].Addr;

            if (val == null) return;

            switch (modbusDictionary.MobusData[val].Type)
            {
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                    ((TextBox)stackModbusText.Children[i]).Text = modbusSoc.GetBool(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:
                    ((TextBox)stackModbusText.Children[i]).Text = modbusSoc.GetFloat(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                    ((TextBox)stackModbusText.Children[i]).Text = modbusSoc.GetInt16(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:
                    ((TextBox)stackModbusText.Children[i]).Text = modbusSoc.GetInt32(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.String:
                    ((TextBox)stackModbusText.Children[i]).Text = modbusSoc.GetString(addr);
                    break;
            }

        }

        private void BtnModbusUserWrite_Click(object sender, RoutedEventArgs e)
        {
            if (modbusSoc == null) return;

            int i = (int)((Button)sender).Tag;

            TM_Comms_ModbusDict.MobusValue.DataTypes val = (TM_Comms_ModbusDict.MobusValue.DataTypes)((ComboBox)stackModbusUserComboBox.Children[i]).Tag;
            int addr = Convert.ToInt32((string)((Label)stackModbusUserLabels.Children[i]).Content);

            switch (val)
            {
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                    bool send = ((TextBox)stackModbusUserText.Children[i]).Text.ToBoolean();
                    modbusSoc.SetBool(addr, send);
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:

                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                    int data = ((TextBox)stackModbusUserText.Children[i]).Text.ToInt();
                    modbusSoc.SetInt16(addr, data);
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:

                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.String:

                    break;
            }
        }

        private void BtnModbusUserRead_Click(object sender, RoutedEventArgs e)
        {
            if (modbusSoc == null) return;

            int i = (int)((Button)sender).Tag;

            TM_Comms_ModbusDict.MobusValue.DataTypes val = (TM_Comms_ModbusDict.MobusValue.DataTypes)((ComboBox)stackModbusUserComboBox.Children[i]).Tag;
            int addr = Convert.ToInt32((string)((Label)stackModbusUserLabels.Children[i]).Content);

            switch (val)
            {
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                    ((TextBox)stackModbusUserText.Children[i]).Text = modbusSoc.GetBool(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:
                    ((TextBox)stackModbusUserText.Children[i]).Text = modbusSoc.GetFloat(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                    ((TextBox)stackModbusUserText.Children[i]).Text = modbusSoc.GetInt16(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:
                    ((TextBox)stackModbusUserText.Children[i]).Text = modbusSoc.GetInt32(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.String:
                    ((TextBox)stackModbusUserText.Children[i]).Text = modbusSoc.GetString(addr).ToString();
                    break;
            }
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
            int i = 0;
            foreach (ComboBox cmb in stackModbusComboBox.Children)
                appSettings.ModbusComboBoxIndices[i++] = (string)cmb.SelectedValue;

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

        private void btnModbusReadAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button btn in stackModbusReadButton.Children)
                BtnModbusRead_Click(btn, new RoutedEventArgs());
        }
    }
}
