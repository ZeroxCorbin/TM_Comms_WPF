using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ApplicationSettingsNS;
using RingBuffer;
using SocketManagerNS;

namespace TM_Comms_WPF
{
    /// <summary>
    /// Interaction logic for Port8080Window.xaml
    /// </summary>
    public partial class Port8080Window : Window
    {
        private Port8080.Rootobject _data;
        public Port8080.Rootobject data
        {
            get { return _data; }
            set { _data = value; }
        }

        private bool IsLoading = true;
        public Port8080Window()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                App.Settings.Port8080Window = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();

            if (double.IsNaN(App.Settings.Port8080Window.Left))
            {
                App.Settings.Port8080Window.Left = Owner.Left;
                App.Settings.Port8080Window.Top = Owner.Top + Owner.Height;
            }

            this.Left = App.Settings.Port8080Window.Left;
            this.Top = App.Settings.Port8080Window.Top;

            if (!CheckOnScreen.IsOnScreen(this))
            {
                App.Settings.Port8080Window.Left = Owner.Left;
                App.Settings.Port8080Window.Top = Owner.Top + Owner.Height;

                this.Left = App.Settings.Port8080Window.Left;
                this.Top = App.Settings.Port8080Window.Top;
            }

            IsLoading = false;
        }
        private string CleanMessage(string msg)
        {
            msg = msg.Replace("�~�", "");
            return msg;
        }
        private void MonitorViewUpdate(Port8080.Rootobject data, string msg)
        {
            txtMonitorDataType.Text = data.DataType.ToString();
            txtMonitorResults.Text = msg.TrimEnd('\n').TrimEnd('\r');

            if (data.DataType == 1300)
            {
                txtMonitorRobot_Base.Text = data._Data.CurrentBaseName;
                txtMonitorRobot_X.Text = data._Data.RobotPoint1[0].ToString();
                txtMonitorRobot_Y.Text = data._Data.RobotPoint1[1].ToString();
                txtMonitorRobot_Z.Text = data._Data.RobotPoint1[2].ToString();

                txtMonitorRobot_RX.Text = data._Data.RobotPoint1[3].ToString();
                txtMonitorRobot_RY.Text = data._Data.RobotPoint1[4].ToString();
                txtMonitorRobot_RZ.Text = data._Data.RobotPoint1[5].ToString();

                txtMonitorRobot_Xa.Text = data._Data.RobotPoint1[6].ToString();
                txtMonitorRobot_Ya.Text = data._Data.RobotPoint1[7].ToString();
                txtMonitorRobot_Za.Text = data._Data.RobotPoint1[8].ToString();

                txtMonitorRobot_RXa.Text = data._Data.RobotPoint1[9].ToString();
                txtMonitorRobot_RYa.Text = data._Data.RobotPoint1[10].ToString();
                txtMonitorRobot_RZa.Text = data._Data.RobotPoint1[11].ToString();
            }
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

            BtnConnect.Content = "Connect";
            BtnConnect.Tag = null;
        }
        private bool Connect()
        {
            CleanSock();

            Socket = new SocketManager($"{App.Settings.RobotIP}:8080");

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
                            BtnConnect.Content = "Start";
                            BtnConnect.Tag = null;
                        }));
            }
            else
            {
                Socket.MessageReceived += Socket_MessageReceived;

                Socket.StartReceiveMessages(@"[�~\u0005�]", "==\"}");

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

            if (PackRate.Count < updateRate)
                return;

            PackRate.Count = 0;

                string msg = CleanMessage(message);
                if (msg != "")
                    this.data = Port8080.Parse(msg);

                if (this.data != null)
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<Port8080.Rootobject, string>(MonitorViewUpdate), this.data, message);
            
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
