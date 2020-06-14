using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TM_Comms_WPF.Net
{
    /// <summary>
    /// Interaction logic for ModbusWindow.xaml
    /// </summary>
    public partial class ModbusWindow : Window
    {
        TM_Comms_ModbusTCP ModbusSoc { get; set; }
        private TM_Comms_ModbusDict ModbusRegisters { get; } = new TM_Comms_ModbusDict();

        bool isLoading = true;
        bool isRunning = false;
        private int NumModbusRegisters { get; set; } = 15;
        private int NumModbusUserRegisters { get; set; } = 15;

        public ModbusWindow()
        {
            InitializeComponent();

            ResetModbusRegisterList();
            ResetModbusUserRegisterList();

            isLoading = false;

            RecalcUserRegisters();

            MobusConnect();
        }

        private void MobusConnect()
        {
            ModbusSoc?.Disconnect();
            ModbusSoc = new TM_Comms_ModbusTCP();

            if (ModbusSoc.Connect(App.Settings.RobotIP))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncRecieveThread_DoWork));

            }
            else
            {
                ModbusSoc?.Disconnect();
                ModbusSoc = null;
            }

        }

        //Control Pad
        private void AsyncRecieveThread_DoWork(object sender)
        {
            isRunning = true;
            while (isRunning)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {
                            elpPowerLight.Fill = Brushes.Red;

                            if (ModbusSoc.GetInt16(ModbusRegisters.MobusData["M/A Mode"].Addr) == 1)
                            {
                                elpAutoLight.Fill = Brushes.SkyBlue;
                                elpManualLight.Fill = Brushes.Transparent;
                            }
                            else
                            {
                                elpAutoLight.Fill = Brushes.Transparent;
                                elpManualLight.Fill = Brushes.Green;
                            }
                            if (ModbusSoc.GetBool(ModbusRegisters.MobusData["EStop"].Addr))
                                elpEstopButton.Fill = Brushes.Red;
                            else
                                elpEstopButton.Fill = Brushes.Transparent;

                            btnStop.Background = Brushes.Transparent;
                            btnPlayPause.Background = Brushes.Transparent;

                            if (ModbusSoc.GetBool(ModbusRegisters.MobusData["Project Running"].Addr))
                                btnPlayPause.Background = new RadialGradientBrush(Colors.LightGreen, Colors.Transparent);
                            else if (ModbusSoc.GetBool(ModbusRegisters.MobusData["Project Paused"].Addr))
                                btnPlayPause.Background = new RadialGradientBrush(Colors.LightYellow, Colors.Transparent);
                            else if (ModbusSoc.GetBool(ModbusRegisters.MobusData["Project Editing"].Addr))
                                btnStop.Background = new RadialGradientBrush(Colors.LightYellow, Colors.Transparent);
                            else
                                btnStop.Background = new RadialGradientBrush(Colors.LightSalmon, Colors.Transparent);

                            if (ModbusSoc.GetBool(ModbusRegisters.MobusData["Error"].Addr))
                                elpIsError.Fill = Brushes.Red;
                            else
                                elpIsError.Fill = Brushes.Green;


                            uint code = (uint)ModbusSoc.GetInt32(ModbusRegisters.MobusData["Last Error Code"].Addr);
                            if (code != 0)
                            {
                                string dat = $"{ModbusSoc.GetInt16(ModbusRegisters.MobusData["Last Error Time Month"].Addr)}/" +
                                                $"{ModbusSoc.GetInt16(ModbusRegisters.MobusData["Last Error Time Date"].Addr)}/" +
                                                $"{ModbusSoc.GetInt16(ModbusRegisters.MobusData["Last Error Time Year"].Addr)} " +
                                                $"{ModbusSoc.GetInt16(ModbusRegisters.MobusData["Last Error Time Hour"].Addr)}:" +
                                                $"{ModbusSoc.GetInt16(ModbusRegisters.MobusData["Last Error Time Minute"].Addr)}:" +
                                                $"{ModbusSoc.GetInt16(ModbusRegisters.MobusData["Last Error Time Second"].Addr)} ";
                                if( DateTime.TryParse(dat, out DateTime date))
                                    txtxErrorDate.Text = date.ToString();
                            }
                            else
                                txtxErrorDate.Text = "";

                            txtErrorCode.Text = code.ToString("X");

                            if (TM_Comms_ErrorCodes.Codes.TryGetValue(code, out string val))
                                txtErrorDescription.Text = val;
                            else
                                txtErrorDescription.Text = "CAN NOT FIND ERROR IN TABLE.";

                        }));

                Thread.Sleep(500);
            }

            isRunning = true;

            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        elpPowerLight.Fill = Brushes.Yellow;
                        elpManualLight.Fill = Brushes.Transparent;
                        elpAutoLight.Fill = Brushes.Transparent;
                        elpEstopButton.Fill = Brushes.Transparent;


                        btnStop.Background = Brushes.Transparent;
                        btnPlayPause.Background = Brushes.Transparent;

                        elpIsError.Fill = Brushes.Transparent;
                        txtErrorCode.Text = "";
                        txtErrorDescription.Text = "";
                    }));
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ModbusSoc.SetBool(ModbusRegisters.MobusData["Stop"].Addr, true);
        }
        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            ModbusSoc.SetBool(ModbusRegisters.MobusData["Play/Pause"].Addr, true);
        }
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            ModbusSoc.SetBool(ModbusRegisters.MobusData["Stick-"].Addr, true);
        }
        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            ModbusSoc.SetBool(ModbusRegisters.MobusData["Stick+"].Addr, true);
        }

        //Modbus Registers
        private void ResetModbusRegisterList()
        {
            Button btn;

            stackModbusComboBox.Children.Clear();
            stackModbusReadButton.Children.Clear();
            stackModbusText.Children.Clear();
            stackModbusWriteButton.Children.Clear();

            stackModbusComboBox.Children.Add(new Label() { Content = "Modbus Register Name", Height = 26, Width = 240 });

            btn = new Button() { Content = "Read All", Height = 26, Width = 90 };
            btn.Click += BtnModbusReadAll_Click;
            stackModbusReadButton.Children.Add(btn);

            stackModbusText.Children.Add(new Label() { Content = "Value", Height = 26, Width = 90 });
            stackModbusWriteButton.Children.Add(new Label() { Content = "", Height = 26, Width = 60 });

            for (int i = 1; i <= NumModbusRegisters; i++)
            {
                ComboBox cmb = new ComboBox() { Height = 26, Width = 240 };
                cmb.Tag = i;
                cmb.ItemsSource = ModbusRegisters.MobusData.Keys;
                cmb.SelectionChanged += CmbModbusRegister_SelectionChanged;
                stackModbusComboBox.Children.Add(cmb);

                btn = new Button() { Height = 26, Width = 90 };
                btn.Tag = i;
                btn.Click += BtnModbusRead_Click;
                stackModbusReadButton.Children.Add(btn);

                stackModbusText.Children.Add(new TextBox() { Height = 26, Width = 90 });

                btn = new Button() { Height = 26, Width = 60 };
                btn.Tag = i;
                btn.Click += BtnModbusWrite_Click;
                btn.IsEnabled = false;
                btn.Visibility = Visibility.Hidden;
                btn.Content = "Write";
                stackModbusWriteButton.Children.Add(btn);
            }

            isLoading = false;

            int ii = 1;
            foreach (string str in App.Settings.ModbusComboBoxIndices)
                if (ii <= NumModbusRegisters)
                    ((ComboBox)stackModbusComboBox.Children[ii++]).SelectedValue = str;
                else
                    break;

            isLoading = true;

        }
        private void CmbModbusRegister_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = ((ComboBox)sender);
            Button rbtn = (Button)stackModbusReadButton.Children[(int)cmb.Tag];
            Button wbtn = (Button)stackModbusWriteButton.Children[(int)cmb.Tag];

            rbtn.Content = $"{ModbusRegisters.MobusData[(string)cmb.SelectedValue].Addr} / {ModbusRegisters.MobusData[(string)cmb.SelectedValue].Addr:X2}";

            if (ModbusRegisters.MobusData[(string)cmb.SelectedValue].Access == TM_Comms_ModbusDict.MobusValue.AccessTypes.R)
            {
                wbtn.IsEnabled = false;
                wbtn.Visibility = Visibility.Hidden;
            }
            else
            {
                wbtn.IsEnabled = true;
                wbtn.Visibility = Visibility.Visible;
            }

            ((TextBox)stackModbusText.Children[(int)cmb.Tag]).Text = string.Empty;
        }
        private void BtnModbusWrite_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusSoc == null) return;

            int i = (int)((Button)sender).Tag;
            string val = (String)((ComboBox)stackModbusComboBox.Children[i]).SelectedValue;
            int addr = ModbusRegisters.MobusData[val].Addr;

            if (val == null) return;

            if (ModbusSoc != null)
            {
                switch (ModbusRegisters.MobusData[val].Type)
                {
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                        bool send = ((TextBox)stackModbusText.Children[i]).Text.ToBoolean();
                        ModbusSoc.SetBool(addr, send);
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:

                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                        short data = (short)((TextBox)stackModbusText.Children[i]).Text.ToInt();
                        ModbusSoc.SetInt16(addr, data);
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
            if (ModbusSoc == null) return;

            int i = (int)((Button)sender).Tag;
            string val = (String)((ComboBox)stackModbusComboBox.Children[i]).SelectedValue;
            if (val == null) return;
            int addr = ModbusRegisters.MobusData[val].Addr;

            if (val == null) return;

            switch (ModbusRegisters.MobusData[val].Type)
            {
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                    ((TextBox)stackModbusText.Children[i]).Text = ModbusSoc.GetBool(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:
                    ((TextBox)stackModbusText.Children[i]).Text = ModbusSoc.GetFloat(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                    ((TextBox)stackModbusText.Children[i]).Text = ModbusSoc.GetInt16(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:
                    ((TextBox)stackModbusText.Children[i]).Text = ModbusSoc.GetInt32(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.String:
                    ((TextBox)stackModbusText.Children[i]).Text = ModbusSoc.GetString(addr);
                    break;
            }

        }
        private void BtnModbusReadAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i <= NumModbusRegisters; i++)
                BtnModbusRead_Click(stackModbusReadButton.Children[i], new RoutedEventArgs());
        }

        //Mobus User Registers
        private void ResetModbusUserRegisterList()
        {
            Button btn;

            stackModbusUserComboBox.Children.Clear();
            stackModbusUserReadButton.Children.Clear();
            stackModbusUserText.Children.Clear();
            stackModbusUserWriteButton.Children.Clear();

            stackModbusUserComboBox.Children.Add(new Label() { Content = "Data Type", Height = 26, Width = 80 });

            btn = new Button() { Content = "Read All", Height = 26, Width = 70 };
            btn.Click += BtnModbusUserReadAll_Click;
            stackModbusUserReadButton.Children.Add(btn);

            stackModbusUserText.Children.Add(new Label() { Content = "Value", Height = 26, Width = 90 });

            btn = new Button() { Content = "Write All", Height = 26, Width = 70 };
            btn.Click += BtnModbusUserWriteAll_Click;
            stackModbusUserWriteButton.Children.Add(btn);

            for (int i = 1; i <= NumModbusUserRegisters; i++)
            {
                ComboBox cmb = new ComboBox() { Height = 26, Width = 80 };
                cmb.Items.Add("Bool");
                cmb.Items.Add("Int16");
                cmb.Items.Add("Int32");
                cmb.Items.Add("Float");
                cmb.Items.Add("String");
                cmb.SelectionChanged += CmbModbusUserRegister_SelectionChanged;
                stackModbusUserComboBox.Children.Add(cmb);

                btn = new Button() { Height = 26, Width = 70 };
                btn.Tag = i;
                btn.Click += BtnModbusUserRead_Click;
                stackModbusUserReadButton.Children.Add(btn);

                stackModbusUserText.Children.Add(new TextBox() { Height = 26, Width = 90 });

                btn = new Button() { Height = 26, Width = 70 };
                btn.Tag = i;
                btn.Content = "Write";
                btn.Click += BtnModbusUserWrite_Click;
                stackModbusUserWriteButton.Children.Add(btn);
            }

            isLoading = false;

            int ii = 1;
            foreach (string str in App.Settings.ModbusUserComboBoxIndices)
                if (ii <= NumModbusUserRegisters)
                    ((ComboBox)stackModbusUserComboBox.Children[ii++]).SelectedValue = str;
                else
                    break;

            isLoading = true;
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

            for (int i = 1; i <= NumModbusUserRegisters; i++)
            {
                object tag = ((ComboBox)stackModbusUserComboBox.Children[i]).Tag;
                if (tag == null)
                {
                    ((Button)stackModbusUserReadButton.Children[i]).Content = string.Empty;
                    ((TextBox)stackModbusUserText.Children[i]).Text = string.Empty;
                    continue;
                }
                TM_Comms_ModbusDict.MobusValue.DataTypes val = (TM_Comms_ModbusDict.MobusValue.DataTypes)tag;

                ((Button)stackModbusUserReadButton.Children[i]).Content = num.ToString();

                switch (val)
                {
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                        num += 1;
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:
                        num += 2;
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                        num += 1;
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:
                        num += 2;
                        break;
                    case TM_Comms_ModbusDict.MobusValue.DataTypes.String:
                        num += 32;
                        break;
                }
            }
        }
        private void BtnModbusUserWrite_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusSoc == null) return;

            int i = (int)((Button)sender).Tag;

            object tag = ((ComboBox)stackModbusUserComboBox.Children[i]).Tag;
            if (tag == null) return;
            TM_Comms_ModbusDict.MobusValue.DataTypes type = (TM_Comms_ModbusDict.MobusValue.DataTypes)tag;

            int addr = Convert.ToInt32((string)((Button)stackModbusUserReadButton.Children[i]).Content);

            switch (type)
            {
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                    bool send = ((TextBox)stackModbusUserText.Children[i]).Text.ToBoolean();
                    ModbusSoc.SetBool(addr, send);
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:

                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                    short data = (short)((TextBox)stackModbusUserText.Children[i]).Text.ToInt();
                    ModbusSoc.SetInt16(addr, data);
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:

                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.String:

                    break;
            }
        }
        private void BtnModbusUserRead_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusSoc == null) return;

            int i = (int)((Button)sender).Tag;

            object tag = ((ComboBox)stackModbusUserComboBox.Children[i]).Tag;
            if (tag == null) return;
            TM_Comms_ModbusDict.MobusValue.DataTypes type = (TM_Comms_ModbusDict.MobusValue.DataTypes)tag;

            int addr = Convert.ToInt32((string)((Button)stackModbusUserReadButton.Children[i]).Content);

            switch (type)
            {
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Bool:
                    ((TextBox)stackModbusUserText.Children[i]).Text = ModbusSoc.GetBool(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Float:
                    ((TextBox)stackModbusUserText.Children[i]).Text = ModbusSoc.GetFloat(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int16:
                    ((TextBox)stackModbusUserText.Children[i]).Text = ModbusSoc.GetInt16(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.Int32:
                    ((TextBox)stackModbusUserText.Children[i]).Text = ModbusSoc.GetInt32(addr).ToString();
                    break;
                case TM_Comms_ModbusDict.MobusValue.DataTypes.String:
                    ((TextBox)stackModbusUserText.Children[i]).Text = ModbusSoc.GetString(addr).ToString();
                    break;
            }
        }
        private void BtnModbusUserReadAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i <= NumModbusUserRegisters; i++)
                BtnModbusUserRead_Click(stackModbusUserReadButton.Children[i], new RoutedEventArgs());
        }
        private void BtnModbusUserWriteAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i <= NumModbusUserRegisters; i++)
                BtnModbusUserWrite_Click(stackModbusUserWriteButton.Children[i], new RoutedEventArgs());
        }

        //Window
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //int i = 0;
            //foreach (ComboBox cmb in stackModbusComboBox.Children)
            //    App.Settings.ModbusComboBoxIndices[i++] = (string)cmb.SelectedValue;

            isRunning = false;
            while (isRunning == false) { }

            ModbusSoc?.Disconnect();
        }

   }
}
