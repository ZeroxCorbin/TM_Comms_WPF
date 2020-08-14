using SocketManagerNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class EthernetSlaveWindow : Window
    {
        private bool IsLoading { get; set; } 
        private SocketManager Socket { get; set; }
        private TM_Comms_EthernetSlave EthernetSlave { get; set; }

        public EthernetSlaveWindow()
        {
            InitializeComponent();

            txtESConnectionString.Text = $"{App.Settings.RobotIP}:5890";

            EthernetSlave = GetESNode();
        }

        private TM_Comms_EthernetSlave GetESNode()
        {
            TM_Comms_EthernetSlave node = new TM_Comms_EthernetSlave();
            if (cmbESDataType.SelectedIndex == 0)
            {
                node.Header = TM_Comms_EthernetSlave.HEADERS.TMSVR;

                txtESScriptID.Text = node.ScriptID.ToString();
            }
            else
            {
                node.Header = TM_Comms_EthernetSlave.HEADERS.TMSVR;
                txtESScriptID.Text = "";
            }


            node.Data = txtESScriptData.Text;

            txtESDataString.Text = node.Message;

            return node;
        }
        private void EthernetSlaveSoc_ConnectState(object sender, bool data)
        {
            if (!data)
            {
                CleanSock();

                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {
                            rectESCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                            txtESDataResponse.Text = "";

                            btnESConnect.Content = "Start";
                            btnESConnect.Tag = null;
                        }));
            }
        }
        private void EthernetSlaveSoc_DataReceived(object sender, string data)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        rectESCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                        txtESDataResponse.Text = data;
                    }));
        } 
        private void CleanSock()
        {
            if (Socket != null)
            {
                Socket.DataReceived -= EthernetSlaveSoc_DataReceived;
                Socket.ConnectState += EthernetSlaveSoc_ConnectState;

                Socket.StopReceiveAsync();
                Socket.Close();

                Socket = null;
            }
        }

        private void BtnESConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnESConnect.Tag == null)
            {
                CleanSock();

                Socket = new SocketManager($"{App.Settings.RobotIP}:5890");

                Socket.DataReceived += EthernetSlaveSoc_DataReceived;
                Socket.ConnectState += EthernetSlaveSoc_ConnectState;

                if (Socket.Connect())
                {
                    Socket.ReceiveAsync();

                    btnESConnect.Content = "Stop";
                    btnESConnect.Tag = 1;
                }
                else
                {
                    CleanSock();
                }
            }
            else
            {
                CleanSock();

                btnESConnect.Content = "Start";
                btnESConnect.Tag = null;
            }
        }
        private void BtnESSend_Click(object sender, RoutedEventArgs e)
        {
            rectESCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            Socket?.Write(EthernetSlave.Message);
        }

        private void CmbESDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EthernetSlave = GetESNode();
        }

    }
}
