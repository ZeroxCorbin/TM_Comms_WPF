using ApplicationSettingsNS;
using RingBuffer;
using SocketManagerNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml;
using System.Xml.Serialization;

namespace TM_Comms_WPF
{
    public partial class EthernetSlaveWindow : Window
    {
        private EthernetSlave EthernetSlave { get; set; }

        private bool IsLoading { get; set; } = true;
        public EthernetSlaveWindow(Window owner)
        {
            DataContext = App.Settings;

            Owner = owner;

            InitializeComponent();

            Window_LoadSettings();
        }
        private void Window_LoadSettings()
        {
            if(double.IsNaN(App.Settings.EthernetSlaveWindow.Left)
                || !CheckOnScreen.IsOnScreen(this)
                || Keyboard.IsKeyDown(Key.LeftShift))
            {
                Left = Owner.Left;
                Top = Owner.Top + Owner.Height;
                Height = 768;
                Width = 1024;
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (App.Settings.Version == TMflowVersions.V1_76_xxxx || App.Settings.Version == TMflowVersions.V1_80_xxxx)
            {
                EthernetSlaveXMLData.File data = null;
                XmlSerializer serializer = new XmlSerializer(typeof(EthernetSlaveXMLData.File));
                using (TextReader sr = new StringReader(EthernetSlave.Commands[App.Settings.Version]))
                {
                    data = (EthernetSlaveXMLData.File)serializer.Deserialize(sr);
                }
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
                            lvi.MouseDoubleClick += Lvi_MouseDoubleClick;
                            LsvWritableValues.Items.Add(lvi);
                        }

                    }

                }
            }

            EthernetSlave = GetESNode();

            IsLoading = false;

            ConnectionInActive();
        }

        private void Lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem tvi = (ListViewItem)sender;
            string insert = "";

            int start = TxtScript.SelectionStart;

            if (start == TxtScript.Text.Length)
            {
                if (start != 0)
                    if (TxtScript.Text[start - 1] != '\n')
                        insert += "\r\n";
            }
            else if (TxtScript.Text[start] == '\r')
            {
                if (start != 0)
                    if (TxtScript.Text[start - 1] != '\n')
                        insert += "\r\n";
            }


            insert += $"{tvi.Content}=";

            TxtScript.Text = TxtScript.Text.Insert(start, insert);

            TxtScript.Focus();
            TxtScript.SelectionStart = start + insert.Length;
        }

        private EthernetSlave GetESNode()
        {
            EthernetSlave node = new EthernetSlave();
            if(Enum.TryParse((string)((ComboBoxItem)cmbESDataType.SelectedItem).Tag, out EthernetSlave.Headers header))
            {
                if (Enum.TryParse((string)((ComboBoxItem)cmbESDataMode.SelectedItem).Tag, out EthernetSlave.Modes mode))
                {
                    node = new EthernetSlave(TxtScript.Text, header, TxtTransactionID.Text, mode);

                    TxtCommand.Text = node.Message;
                }
            }

            return node;
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            EthernetSlave = GetESNode();
            //Socket.Flush();
            Socket?.Write(EthernetSlave.Message);
        }
        private void CmbESDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EthernetSlave = GetESNode();
        }
        private void TxtScript_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoading) return;
            EthernetSlave = GetESNode();
        }
        private void TxtTransactionID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;
            if (!Regex.IsMatch(TxtTransactionID.Text, @"^[a-zA-Z0-9_]+$"))
                TxtTransactionID.Text = "local";

            EthernetSlave = GetESNode();
        }
        private void CmbESDataType_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading) return;
            EthernetSlave = GetESNode();
        }
        private void CmbESDataMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading) return;
            EthernetSlave = GetESNode();
        }

        //CPERR Disgnostics
        private void BtnSendBadChecksum_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMSVR,20,diag,99,Stick_Stop=1,*45\r\n");
        private void BtnSendBadHeader_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMsvr,20,local,2,Stick_Stop=1,*32\r\n");
        private void BtnSendBadPacket_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMSVR,19,-100,2,Stick_Stop=1,*69\r\n");

        private void BtnSendBadPacketData_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMSTA,4,XXXX,*47\r\n");

        private void BtnSendNotSupported_Click(object sender, RoutedEventArgs e) => Socket?.Write("$TMSVR,20,diag,99,Stick_Stop=1,*46\r\n");
        private void BtnSendInvalidData_Click(object sender, RoutedEventArgs e)=> Socket?.Write("$TMSVR,11,diag,1,[{}],*58\r\n");
        private void BtnSendNotExist_Click(object sender, RoutedEventArgs e)=> Socket?.Write("$TMSVR,18,diag,2,Ctrl_DO16=1,*24\r\n");
        private void BtnSendReadOnly_Click(object sender, RoutedEventArgs e)=> Socket?.Write("$TMSVR,19,diag,2,Robot_Link=1,*64\r\n");
        private void BtnSendValueError_Click(object sender, RoutedEventArgs e)=> Socket?.Write("$TMSVR,24,diag,2,Stick_Plus=\"diag\",*48\r\n");

        private void ConnectionActive()
        {
            BtnConnect.Content = "Close";
            BtnConnect.Tag = 2;

            List<GradientStop> gsc = new List<GradientStop>
            {
                new GradientStop((Color)ColorConverter.ConvertFromString("#FFDDDDDD"), 1),
                new GradientStop((Color)ColorConverter.ConvertFromString("#AA4c88d6"), 1)
            };

            BtnConnect.Background = new RadialGradientBrush(new GradientStopCollection(gsc));

            BtnSend.IsEnabled = true;
            BtnSendBadChecksum.IsEnabled = true;
            BtnSendBadHeader.IsEnabled = true;
            BtnSendBadPacket.IsEnabled = true;
            BtnSendBadPacketData.IsEnabled = true;
            BtnSendInvalidData.IsEnabled = true;
            BtnSendNotExist.IsEnabled = true;
            BtnSendNotSupported.IsEnabled = true;
            BtnSendReadOnly.IsEnabled = true;
            BtnSendValueError.IsEnabled = true;
        }
        private void ConnectionInActive()
        {
            BtnConnect.Content = "Connect";
            BtnConnect.Tag = 0;

            List<GradientStop> gsc = new List<GradientStop>
            {
                new GradientStop((Color)ColorConverter.ConvertFromString("#FFDDDDDD"), 1),
                new GradientStop((Color)ColorConverter.ConvertFromString("#AA880000"), 1)
            };

            BtnConnect.Background = new RadialGradientBrush(new GradientStopCollection(gsc));

            BtnSend.IsEnabled = false;
            BtnSendBadChecksum.IsEnabled = false;
            BtnSendBadHeader.IsEnabled = false;
            BtnSendBadPacket.IsEnabled = false;
            BtnSendBadPacketData.IsEnabled = false;
            BtnSendInvalidData.IsEnabled = false;
            BtnSendNotExist.IsEnabled = false;
            BtnSendNotSupported.IsEnabled = false;
            BtnSendReadOnly.IsEnabled = false;
            BtnSendValueError.IsEnabled = false;
        }
        private void ConnectionWaiting()
        {
            BtnConnect.Content = "Trying";
            BtnConnect.Tag = 1;

            List<GradientStop> gsc = new List<GradientStop>
            {
                new GradientStop((Color)ColorConverter.ConvertFromString("#FFDDDDDD"), 1),
                new GradientStop((Color)ColorConverter.ConvertFromString("#AA888800"), 1)
            };

            BtnConnect.Background = new RadialGradientBrush(new GradientStopCollection(gsc));

        }



        private SocketManager Socket { get; set; }
        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if ((int)BtnConnect.Tag == 0)
            {
                ConnectionWaiting();
                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnectThread));
                return;
            }
            else if ((int)BtnConnect.Tag == 1)
                return;

            CleanSock();
        }
        private void ConnectThread(object sender)
        {
            Connect();
        }
        private bool Connect()
        {
            CleanSock();

            Socket = new SocketManager($"{App.Settings.RobotIP}:5891");

            Socket.ConnectState += Socket_ConnectState;

            if (Socket.Connect())
                return true;
            else
                return false;
        }
        private void CleanSock()
        {
            if (Socket != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        (Action)(() =>
                        {
                            ConnectionInActive();
                        }));

                Socket.MessageReceived -= Socket_MessageReceived;
                Socket.ConnectState -= Socket_ConnectState;

                Socket.StopReceiveAsync();
                Socket.Close();

                Socket = null;
            }
        }
        private void Socket_ConnectState(object sender, bool data)
        {
            if (!data)
                CleanSock();
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        (Action)(() =>
                        {
                            ConnectionActive();
                        }));

                Socket.MessageReceived += Socket_MessageReceived;
                Socket.StartReceiveMessages(@"[$]", @"[*][A-Z0-9][A-Z0-9]");

                DataReceiveStopWatch.Restart();
            }
        }
        //Receive Data
        private class PacketRate : RingBuffer<long>
        {
            public PacketRate(int length) : base(length) { }

            public int Count { get; set; } = int.MaxValue - 1;
            public new long Add(long o)
            {
                Count++;
                base.Add(o);
                return 0;
            }

            public long Average
            {
                get
                {
                    long[] lst = base.Raw;
                    long total = 0;
                    foreach (long l in lst)
                        total += l;
                    return total /= base.Length;
                }
            }
        }
        private PacketRate PackRate { get; set; } = new PacketRate(100);
        private Stopwatch DataReceiveStopWatch { get; set; } = new Stopwatch();
        private void Socket_MessageReceived(object sender, string message, string pattern)
        {
            long time = DataReceiveStopWatch.ElapsedMilliseconds;
            PackRate.Add(time);
            DataReceiveStopWatch.Restart();

            int updateRate;
            if (!PackRate.IsFull)
                updateRate = (int)(SliderValue / time);
            else
            {
                updateRate = (int)(SliderValue / PackRate.Average);

                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    TxtAverageMessage.Text = (1.0 / (PackRate.Average / 1000.0)).ToString("# Hz");
                }));
            }

            if (PackRate.Count < updateRate && Regex.IsMatch(message, @"^[$]TMSVR,\w*,[0-9]"))
                return;

            PackRate.Count = 0;

            EthernetSlave es = new EthernetSlave();

            if (!es.ParseMessage(message))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        (Action)(() =>
                        {
                            TxtSocketResponse.Text = message;
                        }));
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        (Action)(() =>
                        {
                            if (es.Header == EthernetSlave.Headers.TMSVR && es.TransactionID_Int >= 0 && es.TransactionID_Int <= 9)
                                TxtSocketResponse.Text = message;
                            else
                            {
                                TxtCommandResponse.Text += es.Message;
                                TxtCommandResponse.ScrollToEnd();
                            }
                        }));
            }
        }
        //Receive Rate Control
        private double SliderValue { get; set; }
        private void SldUpdateFreq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderValue = SldUpdateFreq.Value;
            TxtDisplayRate.Text = (SliderValue / 1000).ToString("0.00 sec");
        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => CleanSock();




    }
}
