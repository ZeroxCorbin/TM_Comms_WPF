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
using TM_Comms_WPF.Core;
using TM_Comms_WPF.ControlViewModels;

namespace TM_Comms_WPF.WindowViewModels
{
    public class EthernetSlaveViewModel : Core.BaseViewModel
    {
        private AsyncSocket.ASocketManager Socket { get; }
        public string ConnectButtonText { get => connectButtonText; set => SetProperty(ref connectButtonText, value); }
        private string connectButtonText = "Connect";
        public bool ConnectionState { get => connectionState; set => SetProperty(ref connectionState, value); }
        private bool connectionState;
        public string ConnectMessage { get => connectMessage; set { _ = SetProperty(ref connectMessage, value); OnPropertyChanged("IsMessage"); } }
        private string connectMessage;

        public string CaptureButtonText { get => captureButtonText; set => SetProperty(ref captureButtonText, value); }
        private string captureButtonText = "Start Capture";

        public PendantControlViewModel Pendant { get; } = new PendantControlViewModel();

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

            Socket = new AsyncSocket.ASocketManager();
            Socket.CloseEvent += Socket_CloseEvent;
            Socket.ConnectEvent += Socket_ConnectEvent;
            Socket.ExceptionEvent += Socket_ExceptionEvent;
            Socket.MessageEvent += Socket_MessageEvent;

            Reload();
        }
        public void Reload()
        {
            if (Socket.IsConnected)
                ConnectAction(new object());

            CommandList.Clear();

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

                        CommandList.Add(lvi);
                    }
                }
            }
        }



        private void Socket_MessageEvent(object sender, EventArgs e)
        {
            string message = (string)sender;

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
                    Pendant.UpdatePendant(es);
                    ESMessage = message;
                }


            }
            else
            {
                if (es.Message.EndsWith("\r\n"))
                    ESCommandResponse += es.Message;
                else
                    ESCommandResponse += es.Message + "\r\n";
            }
            es = null;
        }

        private void Socket_ExceptionEvent(object sender, EventArgs e)
        {
            ConnectMessage = ((Exception)sender).Message;
        }

        private void Socket_ConnectEvent(object sender, EventArgs e)
        {
            ConnectionState = true;
            ConnectButtonText = "Close";

            Socket.StartReceiveMessages("\r\n");

            DataReceiveStopWatch.Restart();
        }

        private void Socket_CloseEvent(object sender, EventArgs e)
        {
            ConnectionState = false;
            ConnectButtonText = "Connect";

            Pendant.Reset();
        }

        public void ViewClosing()
        {
            //Socket.StopReceiveAsync();
            Socket.Close();
        }

        private void ConnectAction(object parameter)
        {
            if (Socket.IsConnected)
            {
                Socket.Close();
            }
            else
            {
                ConnectMessage = string.Empty;

                Task.Run(() =>
                {
                    ConnectButtonText = "Trying";

                    if (!Socket.Connect(App.Settings.RobotIP, 5891))
                        ConnectMessage = "Unable to connect!";
                });
            }
        }

        //private void Socket_MessageReceived(object sender, string message, string pattern)
        //{
        //    EthernetSlave es = new EthernetSlave();

        //    if (!es.ParseMessage(message))
        //    {
        //        ESCommandResponse += message;
        //        return;
        //    }
        //    if (es.Header == EthernetSlave.Headers.TMSVR && es.TransactionID_Int >= 0 && es.TransactionID_Int <= 9)
        //    {
        //        if (CaptureData)
        //            outputFile.WriteLine(Regex.Replace(message, @"^[$]TMSVR,\w*,[0-9],[0-2],", "").Replace("\r\n", ","));

        //        if (DataReceiveStopWatch.ElapsedMilliseconds > 42)
        //        {
        //            DataReceiveStopWatch.Restart();
        //            UpdatePendant(es);
        //            ESMessage = message;
        //        }
        //    }
        //    else
        //    {
        //        if(es.Message.EndsWith("\r\n"))
        //            ESCommandResponse += es.Message;
        //        else
        //            ESCommandResponse += es.Message + "\r\n";
        //    }
        //}

        private void SendCommandAction(object parameter) => Socket?.Send(GetESNode().Message);

        private void SendBadChecksumAction(object parameter) => Socket?.Send($"$TMSVR,19,diag,2,Stick_Stop=1,*7C\r\n");
        private void SendBadHeaderAction(object parameter) => Socket?.Send($"$TMSVr,20,local,2,Stick_Stop=1,*32\r\n");
        private void SendBadPacketAction(object parameter) => Socket?.Send($"$TMSVR,19,-100,2,Stick_Stop=1,*69\r\n");
        private void SendBadPacketDataAction(object parameter) => Socket?.Send($"$TMSVR,20,local,2,Stick_,*12\r\n");

        private void SendNotSupportedAction(object parameter) => Socket?.Send("$TMSVR,20,diag,99,Stick_Stop=1,*46\r\n");
        private void SendInvalidDataAction(object parameter) => Socket?.Send("$TMSVR,11,diag,1,[{}],*58\r\n");
        private void SendNotExistAction(object parameter) => Socket?.Send("$TMSVR,18,diag,2,Ctrl_DO16=1,*24\r\n");
        private void SendReadOnlyAction(object parameter) => Socket?.Send("$TMSVR,19,diag,2,Robot_Link=1,*64\r\n");
        private void SendValueErrorAction(object parameter) => Socket?.Send("$TMSVR,24,diag,2,Stick_Plus=\"diag\",*48\r\n");


        private void Pendant_StopEvent() => Socket?.Send($"$TMSVR,20,local,2,Stick_Stop=1,*12\r\n");
        private void Pendant_PlayPauseEvent() => Socket?.Send($"$TMSVR,25,local,2,Stick_PlayPause=1,*59\r\n");
        private void Pendant_PlusEvent() => Socket?.Send($"$TMSVR,20,local,2,Stick_Plus=1,*10\r\n");
        private void Pendant_MinusEvent() => Socket?.Send($"$TMSVR,21,local,2,Stick_Minus=1,*67\r\n");

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

        private Stopwatch DataReceiveStopWatch { get; set; } = new Stopwatch();

        public ObservableCollection<ListViewItem> CommandList { get; } = new ObservableCollection<ListViewItem>();
        public ListViewItem CommandItem { get => commandItem; set { _ = SetProperty(ref commandItem, value); UpdateScript(); } }
        private ListViewItem commandItem;
        public ComboBoxItem MessageHeader { get => messageHeader; set { _ = SetProperty(ref messageHeader, value); GetESNode(); } }
        private ComboBoxItem messageHeader;
        public ComboBoxItem MessageFormat { get => messageFormat; set { _ = SetProperty(ref messageFormat, value); GetESNode(); } }
        private ComboBoxItem messageFormat;

        public string Script { get => script; set { _ = SetProperty(ref script, value); GetESNode(); } }
        private string script;
        public string TransactionID { get => transactionID; set { _ = SetProperty(ref transactionID, value); GetESNode(); } }
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

            if (MessageHeader == null) return es;
            if (MessageFormat == null) return es;

            if (Enum.TryParse(MessageHeader.Content.ToString(), out EthernetSlave.Headers header))
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
