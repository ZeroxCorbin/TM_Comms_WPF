using ApplicationSettingsNS;
using RingBuffer;
using SocketManagerNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace TM_Comms_WPF
{
    public partial class EthernetSlaveWindow : Window
    {

        private EthernetSlave EthernetSlave { get; set; }

        private bool IsLoading { get; set; } = true;
        public EthernetSlaveWindow()
        {
            InitializeComponent();

            if (Keyboard.IsKeyDown(Key.LeftShift))
                App.Settings.EthernetSlaveWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();

            this.Left = App.Settings.EthernetSlaveWindow.Left;
            this.Top = App.Settings.EthernetSlaveWindow.Top;

            if (!CheckOnScreen.IsOnScreen(this))
            {
                App.Settings.EthernetSlaveWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();

                this.Left = App.Settings.EthernetSlaveWindow.Left;
                this.Top = App.Settings.EthernetSlaveWindow.Top;
            }

            EthernetSlave = GetESNode();

            IsLoading = false;
        }

        private EthernetSlave GetESNode()
        {
            EthernetSlave.HEADERS h;

            if ((string)((ComboBoxItem)cmbESDataType.SelectedItem).Tag == "0")
                h = EthernetSlave.HEADERS.TMSVR;
            else
                h = EthernetSlave.HEADERS.CPERR;

            EthernetSlave.MODES m;
            if ((string)((ComboBoxItem)cmbESDataMode.SelectedItem).Tag == "0")
                m = EthernetSlave.MODES.STRING;
            else if ((string)((ComboBoxItem)cmbESDataMode.SelectedItem).Tag == "1")
                m = EthernetSlave.MODES.JSON;
            else if ((string)((ComboBoxItem)cmbESDataMode.SelectedItem).Tag == "2")
                m = EthernetSlave.MODES.STRING_RESPONSE;
            else
                m = EthernetSlave.MODES.STRING;

            EthernetSlave node = new EthernetSlave(TxtCommandData.Text, h, txtESTransactionID.Text, m);

            TxtCommand.Text = node.Message;

            return node;
        }
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));

            EthernetSlave = GetESNode();
            //Socket.Flush();
            Socket?.Write(EthernetSlave.Message);
        }
        private void CmbESDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EthernetSlave = GetESNode();
        }
        private void TxtESScriptData_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoading) return;
            EthernetSlave = GetESNode();
        }
        private void TxtESTransactionID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;
            if (!Regex.IsMatch(txtESTransactionID.Text, @"^[a-zA-Z0-9_]+$"))
                txtESTransactionID.Text = "local";

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

        //Connect Socket
        private SocketManager Socket { get; set; }
        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (BtnConnect.Tag == null)
            {
                if (Connect())
                {
                    BtnConnect.Content = "Close";
                    BtnConnect.Tag = 1;
                    return;
                }
            }
            CleanSock();
        }
        private bool Connect()
        {
            CleanSock();

            Socket = new SocketManager($"{App.Settings.RobotIP}:5891");

            Socket.ConnectState += Socket_ConnectState;

            if (Socket.Connect())
                return true;
            else
            {
                CleanSock();
                return false;
            }
        }
        private void CleanSock()
        {
            if (Socket != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {
                            RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));

                            BtnConnect.Content = "Connect";
                            BtnConnect.Tag = null;
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
            {
                CleanSock();

                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {
                            RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));

                            BtnConnect.Content = "Connect";
                            BtnConnect.Tag = null;
                        }));
            }
            else
            {
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
                updateRate = (int)(SliderValue / PackRate.Average);

            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        TxtAverageMessage.Text = PackRate.Average.ToString("# ms");
                    }));

            if (PackRate.Count < updateRate && Regex.IsMatch(message, @"^[$]\w*,\w*,[0-9]"))
                return;

            PackRate.Count = 0;

            Dispatcher.BeginInvoke(DispatcherPriority.Render,
                    (Action)(() =>
                    {
                        if (!Regex.IsMatch(message, @"^[$]\w*,\w*,[0-9],"))
                        {
                            RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                            TxtCommandResponse.Text += message + "\r\n";
                            TxtCommandResponse.ScrollToEnd();
                        }
                        else
                            TxtSocketResponse.Text = message;
                    }));


        }
        //Receive Rate Control
        private double SliderValue { get; set; }
        private void SldUpdateFreq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderValue = SldUpdateFreq.Value;
            TxtDisplayRate.Text = (SliderValue / 1000).ToString("0.00 sec");
        }
        //Window Changes
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoading) return;

            App.Settings.EthernetSlaveWindow.Top = Top;
            App.Settings.EthernetSlaveWindow.Left = Left;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => CleanSock();
    }
}
