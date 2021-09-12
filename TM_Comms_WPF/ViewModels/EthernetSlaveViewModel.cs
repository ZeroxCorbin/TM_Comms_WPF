using RingBuffer;
using SocketManagerNS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TM_Comms;
using TM_Comms_WPF.Commands;

namespace TM_Comms_WPF.ViewModels
{
    public class EthernetSlaveViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public string Title { get => "Ethernet Slave"; }
        public double Left { get => App.Settings.EthernetSlaveWindow.Left; set { App.Settings.EthernetSlaveWindow.Left = value; OnPropertyChanged(); } }
        public double Top { get => App.Settings.EthernetSlaveWindow.Top; set { App.Settings.EthernetSlaveWindow.Top = value; OnPropertyChanged(); } }
        public double Width { get => App.Settings.EthernetSlaveWindow.Width; set { App.Settings.EthernetSlaveWindow.Width = value; OnPropertyChanged(); } }
        public double Height { get => App.Settings.EthernetSlaveWindow.Height; set { App.Settings.EthernetSlaveWindow.Height = value; OnPropertyChanged(); } }
        public WindowState WindowState { get => App.Settings.EthernetSlaveWindow.WindowState; set { App.Settings.EthernetSlaveWindow.WindowState = value; OnPropertyChanged(); } }

        public PendantControlViewModel Pendant { get; } = new PendantControlViewModel();
        private SocketManager Socket { get; set; } = new SocketManager();

        public string ConnectionString { get => App.Settings.RobotIP; set { App.Settings.RobotIP = value; OnPropertyChanged(); } }
        public string ConnectButtonText { get => connectButtonText; set => SetProperty(ref connectButtonText, value); }
        private string connectButtonText = "Connect";
        public bool ConnectionState { get => connectionState; set => SetProperty(ref connectionState, value); }
        private bool connectionState;
        public string ConnectMessage { get => connectMessage; set { SetProperty(ref connectMessage, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMessage")); } }
        private string connectMessage;
        public bool IsConnectMessage { get => !string.IsNullOrEmpty(connectMessage); }
        public bool IsRunning { get => isRunning; private set => SetProperty(ref isRunning, value); }
        private bool isRunning;
        public string CaptureButtonText { get => captureButtonText; set => SetProperty(ref captureButtonText, value); }
        private string captureButtonText = "Start Capture";



        public string ESMessage { get => esMessage; set => SetProperty(ref esMessage, value); }
        private string esMessage;
        public string ESCommandResponse { get => esCommandResponse; set => SetProperty(ref esCommandResponse, value); }
        private string esCommandResponse;
        public string Average { get => average; set => SetProperty(ref average, value); }
        private string average;

        public ICommand ConnectCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand CaptureCommand { get; }
        public ICommand SendBadChecksumCommand { get; }
        public ICommand SendBadHeaderCommand { get; }
        public ICommand SendBadPacketCommand { get; }
        public ICommand SendBadPacketDataCommand { get; }
        public ICommand SendNotSupportedCommand { get; }
        public ICommand SendInvalidDataCommand { get; }
        public ICommand SendNotExistCommand { get; }
        public ICommand SendReadOnlyCommand { get; }
        public ICommand SendValueErrorCommand { get; }



        private double SliderValue { get; set; }

        public EthernetSlaveViewModel()
        {
            ConnectCommand = new RelayCommand(ConnectAction, c => true);
            SendCommand = new RelayCommand(SendCommandAction, c => true);
            CaptureCommand = new RelayCommand(CaptureAction, c => true);
            SendBadChecksumCommand = new RelayCommand(SendBadChecksumAction, c => true);
            SendBadHeaderCommand = new RelayCommand(SendBadHeaderAction, c => true);
            SendBadPacketCommand = new RelayCommand(SendBadPacketAction, c => true);
            SendBadPacketDataCommand = new RelayCommand(SendBadPacketDataAction, c => true);
            SendNotSupportedCommand = new RelayCommand(SendNotSupportedAction, c => true);
            SendInvalidDataCommand = new RelayCommand(SendInvalidDataAction, c => true);
            SendNotExistCommand = new RelayCommand(SendNotExistAction, c => true);
            SendReadOnlyCommand = new RelayCommand(SendReadOnlyAction, c => true);
            SendValueErrorCommand = new RelayCommand(SendValueErrorAction, c => true);

            Pendant.StopEvent += Pendant_StopEvent;
            Pendant.PlayPauseEvent += Pendant_PlayPauseEvent;
            Pendant.PlusEvent += Pendant_PlusEvent;
            Pendant.MinusEvent += Pendant_MinusEvent;

            Socket.ConnectState += Socket_ConnectState;

            EthernetSlaveXMLData.File data = EthernetSlave.GetXMLCommands(App.Settings.Version);
            if (data != null)
            {
                foreach (EthernetSlaveXMLData.FileSetting setting in data.CodeTable)
                {
                    if (setting.Accessibility == "R/W")
                    {
                        ListViewItem lvi = new ListViewItem()
                        {
                            Content = setting.Item
                        };
                        //lvi.MouseDoubleClick += Lvi_MouseDoubleClick;
                        CommandList.Add(lvi);
                    }
                }
            }
        }
        private void ConnectAction(object parameter)
        {
            if (Socket.IsConnected)
            {
                Socket.StopReceiveAsync();
                Socket.Close();
            }
            else
            {
                ConnectMessage = string.Empty;

                Socket.ConnectionString = $"{App.Settings.RobotIP}:5891";

                if (!Socket.Connect())
                    ConnectMessage = Socket.IsException ? Socket.Exception.Message : "Unable to connect!";
            }
        }

        private void SendCommandAction(object parameter) => Socket?.Write(GetESNode().Message);

        private void SendBadChecksumAction(object parameter) => Socket?.Write($"$TMSVR,20,diag,99,Stick_Stop=1,*45\r\n");
        private void SendBadHeaderAction(object parameter) => Socket?.Write($"$TMsvr,20,local,2,Stick_Stop=1,*32\r\n");
        private void SendBadPacketAction(object parameter) => Socket?.Write($"$TMSVR,19,-100,2,Stick_Stop=1,*69\r\n");
        private void SendBadPacketDataAction(object parameter) => Socket?.Write($"$TMSVR,20,local,2,Stick_,*12\r\n");
        private void SendNotSupportedAction(object parameter) => Socket?.Write("$TMSVR,20,diag,99,Stick_Stop=1,*46\r\n");
        private void SendInvalidDataAction(object parameter) => Socket?.Write("$TMSVR,11,diag,1,[{}],*58\r\n");
        private void SendNotExistAction(object parameter) => Socket?.Write("$TMSVR,18,diag,2,Ctrl_DO16=1,*24\r\n");
        private void SendReadOnlyAction(object parameter) => Socket?.Write("$TMSVR,19,diag,2,Robot_Link=1,*64\r\n");
        private void SendValueErrorAction(object parameter) => Socket?.Write("$TMSVR,24,diag,2,Stick_Plus=\"diag\",*48\r\n");


        private void Pendant_StopEvent() => Socket?.Write($"$TMSVR,20,local,2,Stick_Stop=1,*12\r\n");
        private void Pendant_PlayPauseEvent() => Socket?.Write($"$TMSVR,25,local,2,Stick_PlayPause=1,*59\r\n");
        private void Pendant_PlusEvent() => Socket?.Write($"$TMSVR,20,local,2,Stick_Plus=1,*10\r\n");
        private void Pendant_MinusEvent() => Socket?.Write($"$TMSVR,21,local,2,Stick_Minus=1,*67\r\n");

        private bool CaptureData { get; set; }

        private StreamWriter outputFile;
        private void CaptureAction(object parameter)
        {
            if (!CaptureData)
            {
                outputFile = new StreamWriter($"dump_{DateTime.Now.Ticks}.csv");
                CaptureButtonText = "Stop Capture";

                CaptureData = true;

            }
            else
            {
                CaptureData = false;
                Thread.Sleep(10);

                outputFile?.Close();
                CaptureButtonText = "Start Capture";

            }
        }

        private void Socket_ConnectState(object sender, bool state)
        {

            ConnectionState = state;
            if (state)
            {
                ConnectButtonText = "Close";
                Socket.MessageReceived += Socket_MessageReceived;
                Socket.StartReceiveMessages(@"[$]", @"[*][A-Z0-9][A-Z0-9]");

                DataReceiveStopWatch.Restart();
            }
            else
            {
                ConnectButtonText = "Connect";
                Pendant.Reset();
            }
        }

        private Stopwatch DataReceiveStopWatch { get; set; } = new Stopwatch();
        private void Socket_MessageReceived(object sender, string message, string pattern)
        {
            EthernetSlave es = new EthernetSlave();

            if (!es.ParseMessage(message))
            {
                ESCommandResponse += message;
                return;
            }
            if (es.Header == EthernetSlave.Headers.TMSVR && es.TransactionID_Int >= 0 && es.TransactionID_Int <= 9)
            {
                if (CaptureData)
                    outputFile.WriteLine(Regex.Replace(message, @"^[$]TMSVR,\w*,[0-9],[0-2],", "").Replace("\r\n", ","));

                if (DataReceiveStopWatch.ElapsedMilliseconds > 42)
                {
                    DataReceiveStopWatch.Restart();
                    UpdatePendant(es);
                    ESMessage = message;
                }
            }
            else
            {
                if(es.Message.EndsWith("\r\n"))
                    ESCommandResponse += es.Message;
                else
                    ESCommandResponse += es.Message + "\r\n";
            }
        }

        private void UpdatePendant(EthernetSlave es)
        {
            Pendant.Power = Pendant.Good;

            if (es.GetValue("MA_Mode") == "1")
            {
                Pendant.Auto = Pendant.Good;
                Pendant.Manual = Pendant.Disabled;
            }
            else
            {
                Pendant.Auto = Pendant.Disabled;
                Pendant.Manual = Pendant.Good;
            }

            if (es.GetValue("ESTOP") == "true")
                Pendant.Estop = Pendant.Bad;
            else
                Pendant.Estop = Pendant.Disabled;

            if (App.Settings.Version > TMflowVersions.V1_80_xxxx)
            {
                if (es.GetValue("Get_Control") == "true")
                    Pendant.GetControl = Pendant.Good;
                else
                    Pendant.GetControl = Pendant.Bad;

                if (es.GetValue("Auto_Remote_Active") == "true")
                    Pendant.AutoActive = Pendant.Good;
                else
                    Pendant.AutoActive = Pendant.Bad;

                if (es.GetValue("Auto_Remote_Enable") == "true")
                    Pendant.AutoEnable = Pendant.Good;
                else
                    Pendant.AutoEnable = Pendant.Bad;
            }

            if (es.GetValue("Project_Run") == "true")
            {
                Pendant.Play = Pendant.GoodRadial;
                Pendant.Stop = Pendant.Transparent;
            }
            else if (es.GetValue("Project_Pause") == "true")
            {
                Pendant.Play = Pendant.MehRadial;
                Pendant.Stop = Pendant.Transparent;
            }
            else if (es.GetValue("Project_Edit") == "true")
            {
                Pendant.Play = Pendant.Transparent;
                Pendant.Stop = Pendant.MehRadial;
            }
            else
            {
                Pendant.Play = Pendant.Transparent;
                Pendant.Stop = Pendant.BadRadial;
            }

            if (es.GetValue("Robot_Error") == "true")
                Pendant.Error = Pendant.Bad;
            else
                Pendant.Error = Pendant.Disabled;


            uint code = uint.Parse(es.GetValue("Error_Code"));
            if (code != 0)
            {
                string dat = $"{es.GetValue("Error_Time")}";
                if (DateTime.TryParse(dat, out DateTime date))
                    Pendant.ErrorDate = date.ToString();
            }
            else
                Pendant.ErrorDate = "";

            Pendant.ErrorCode = code.ToString("X");

            if (ErrorCodes.Codes.TryGetValue(code, out string val))
                Pendant.ErrorDescription = val;
            else
                Pendant.ErrorDescription = "CAN NOT FIND ERROR IN TABLE.";
        }

        public ListViewItem CommandItem { get => commandItem; set { SetProperty(ref commandItem, value); UpdateScript(); } }
        private ListViewItem commandItem;

        public ComboBoxItem MessageType { get => messageType; set { SetProperty(ref messageType, value); GetESNode(); } }
        private ComboBoxItem messageType;
        public ComboBoxItem MessageFormat { get => messageFormat; set { SetProperty(ref messageFormat, value); GetESNode(); } }
        private ComboBoxItem messageFormat;
        public ObservableCollection<ListViewItem> CommandList { get; } = new ObservableCollection<ListViewItem>();
        public string Script { get => script; set { SetProperty(ref script, value); GetESNode(); } }
        private string script;
        public string TransactionID { get => transactionID; set { SetProperty(ref transactionID, value); GetESNode(); } }
        private string transactionID = "local";
        public string Command { get => command; set => SetProperty(ref command, value); }
        private string command;
        //private void Lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    ListViewItem tvi = (ListViewItem)sender;
        //    string insert = "";

        //    int start = TxtScript.SelectionStart;

        //    if (start == TxtScript.Text.Length)
        //    {
        //        if (start != 0)
        //            if (TxtScript.Text[start - 1] != '\n')
        //                insert += "\r\n";
        //    }
        //    else if (TxtScript.Text[start] == '\r')
        //    {
        //        if (start != 0)
        //            if (TxtScript.Text[start - 1] != '\n')
        //                insert += "\r\n";
        //    }


        //    insert += $"{tvi.Content}=";

        //    TxtScript.Text = TxtScript.Text.Insert(start, insert);

        //    TxtScript.Focus();
        //    TxtScript.SelectionStart = start + insert.Length;
        //}

        //private EthernetSlave GetESNode()
        //{
        //    EthernetSlave node = new EthernetSlave();
        //    if (Enum.TryParse((string)((ComboBoxItem)cmbESDataType.SelectedItem).Tag, out EthernetSlave.Headers header))
        //    {
        //        if (Enum.TryParse((string)((ComboBoxItem)cmbESDataMode.SelectedItem).Tag, out EthernetSlave.Modes mode))
        //        {
        //            node = new EthernetSlave(TxtScript.Text, header, TxtTransactionID.Text, mode);

        //            TxtCommand.Text = node.Message;
        //        }
        //    }

        //    return node;
        //}

        //private void BtnSend_Click(object sender, RoutedEventArgs e)
        //{
        //    EthernetSlave = GetESNode();

        //}
        //private void CmbESDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    EthernetSlave = GetESNode();
        //}
        //private void TxtScript_TextChanged(object sender, TextChangedEventArgs e)
        //{

        //    EthernetSlave = GetESNode();
        //}
        //private void TxtTransactionID_LostFocus(object sender, RoutedEventArgs e)
        //{

        //    if (!Regex.IsMatch(TxtTransactionID.Text, @"^[a-zA-Z0-9_]+$"))
        //        TxtTransactionID.Text = "local";

        //    EthernetSlave = GetESNode();
        //}
        //private void CmbESDataType_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        //{

        //    EthernetSlave = GetESNode();
        //}
        //private void CmbESDataMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{

        //    EthernetSlave = GetESNode();
        //}

        //CPERR Disgnostics






        //Receive Data

        //Receive Rate Control
        //private double SliderValue { get; set; }
        //private void SldUpdateFreq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    SliderValue = SldUpdateFreq.Value;
        //    TxtDisplayRate.Text = (SliderValue / 1000).ToString("0.00 sec");
        //}

        private void UpdateScript()
        {
            if (CommandItem == null) return;

            if (string.IsNullOrEmpty(Script))
                Script = CommandItem.Content.ToString() + "=";
            else
                Script += "\r\n" + CommandItem.Content.ToString() + "=";
        }
        private EthernetSlave GetESNode()
        {

            EthernetSlave es = new EthernetSlave();

            if (MessageType == null) return es;
            if (MessageFormat == null) return es;

            if (Enum.TryParse(MessageType.Content.ToString(), out EthernetSlave.Headers header))
            {
                if (Enum.TryParse(MessageFormat.Content.ToString(), out EthernetSlave.Modes mode))
                {
                    es = new EthernetSlave(Script, header, TransactionID, mode);

                    Command = es.Message;
                }
            }

            return es;
        }


        //private void TxtTransactionID_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (IsLoading) return;
        //    if (!Regex.IsMatch(TxtTransactionID.Text, @"^[a-zA-Z0-9_]+$"))
        //        TxtTransactionID.Text = "local";

        //    EthernetSlave = GetESNode();
        //}



    }
}
