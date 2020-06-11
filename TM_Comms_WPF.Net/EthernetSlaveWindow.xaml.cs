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
    /// <summary>
    /// Interaction logic for EthernetSlaveWindow.xaml
    /// </summary>
    public partial class EthernetSlaveWindow : Window
    {
        SocketManager ethernetSlaveSoc;
        TM_Comms_EthernetSlave ethernetSlave;

        public EthernetSlaveWindow()
        {
            InitializeComponent();

            txtESConnectionString.Text = $"{App.Settings.RobotIP}:5890";

            ethernetSlave = GetESNode();
        }

        private void btnESConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnESConnect.Tag == null)
            {
                EthernetSlaveSoc_Close();

                ethernetSlaveSoc = new SocketManager($"{App.Settings.RobotIP}:5890");
                if (ethernetSlaveSoc.Connect(true))
                {
                    ethernetSlaveSoc.DataReceived += EthernetSlaveSoc_DataReceived;
                    ethernetSlaveSoc.Disconnected += EthernetSlaveSoc_Disconnected;

                    ethernetSlaveSoc.StartRecieveAsync();

                    btnESConnect.Content = "Stop";
                    btnESConnect.Tag = 1;
                }
            }
            else
            {
                EthernetSlaveSoc_Close();

                btnESConnect.Content = "Start";
                btnESConnect.Tag = null;
            }
        }
        private void EthernetSlaveSoc_Disconnected(object sender, SocketManager.SocketEventArgs data)
        {
            EthernetSlaveSoc_Close();

            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        rectESCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                        txtESDataResponse.Text = "";

                        btnESConnect.Content = "Start";
                        btnESConnect.Tag = null;
                    }));
        }
        private void EthernetSlaveSoc_Close()
        {
            if (ethernetSlaveSoc != null)
            {
                ethernetSlaveSoc.DataReceived -= EthernetSlaveSoc_DataReceived;
                ethernetSlaveSoc.Disconnected -= EthernetSlaveSoc_Disconnected;

                ethernetSlaveSoc.StopRecieveAsync();
                ethernetSlaveSoc.Disconnect();
            }

        }
        private void EthernetSlaveSoc_DataReceived(object sender, SocketManager.SocketEventArgs data)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        rectESCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                        txtESDataResponse.Text = data.Message;
                    }));
        }
        private void btnESSend_Click(object sender, RoutedEventArgs e)
        {
            rectESCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            ethernetSlaveSoc?.Write(ethernetSlave.Message);
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

        private void cmbESDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ethernetSlave = GetESNode();
        }

    }
}
