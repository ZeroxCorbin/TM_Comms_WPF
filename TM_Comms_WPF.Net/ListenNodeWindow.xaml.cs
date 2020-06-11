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
    /// Interaction logic for ListenNodeWindow.xaml
    /// </summary>
    public partial class ListenNodeWindow : Window
    {
        SocketManager listenNodeSoc;
        TM_Comms_ListenNode listenNode;


        public ListenNodeWindow()
        {
            InitializeComponent();

            listenNode = GetLNNode();

            txtLNConnectionString.Text = $"{App.Settings.RobotIP}:5890";

            ConnectionInActive();
        }

        private void ConnectionActive()
        {
            btnLNConnect.Content = "Stop";
            btnLNConnect.Tag = 1;

            btnLNSend.IsEnabled = true;
            btnLNSendMoves.IsEnabled = true;
        }

        private void ConnectionInActive()
        {
            btnLNConnect.Content = "Start";
            btnLNConnect.Tag = null;

            btnLNSend.IsEnabled = false;
            btnLNSendMoves.IsEnabled = false;
        }

        private void btnLNConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnLNConnect.Tag == null)
            {
                ListenNodeSoc_Close();

                listenNodeSoc = new SocketManager($"{App.Settings.RobotIP}:5890");
                if (listenNodeSoc.Connect(true))
                {
                    listenNodeSoc.DataReceived += ListenNodeSoc_DataReceived;
                    listenNodeSoc.Disconnected += ListenNodeSoc_Disconnected;

                    listenNodeSoc.StartRecieveAsync();

                    ConnectionActive();
                }
            }
            else
            {
                ListenNodeSoc_Close();

                ConnectionInActive();
            }
        }
        private void ListenNodeSoc_Disconnected(object sender, SocketManager.SocketEventArgs data)
        {
            ListenNodeSoc_Close();

            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                        txtLNDataResponse.Text = "";

                        ConnectionInActive();
                    }));
        }
        private void ListenNodeSoc_Close()
        {
            if (listenNodeSoc != null)
            {
                listenNodeSoc.DataReceived -= ListenNodeSoc_DataReceived;
                listenNodeSoc.Disconnected -= ListenNodeSoc_Disconnected;

                listenNodeSoc.StopRecieveAsync();
                listenNodeSoc.Disconnect();
            }

        }
        private string positionRequest = null;
        private void ListenNodeSoc_DataReceived(object sender, SocketManager.SocketEventArgs data)
        {
            if(positionRequest != null)
            {
                string[] spl = data.Message.Trim('\0').Split(',');

                if (spl[0].Equals("$TMSTA"))
                {
                    if(spl[2].Equals("90"))
                    {
                        positionRequest = null;

                        spl = data.Message.Trim('\0').Split('{');
                        string pos = spl[1].Substring(0, spl[1].IndexOf('}'));

                        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (Action)(() =>
                                {
                                    txtLNNewPosition.Text = pos;
                                }));
                    }
                }
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                        txtLNDataResponse.Text += data.Message.Trim('\0');
                        txtLNDataResponse.ScrollToEnd();
                    }));
        }
        private void btnLNSend_Click(object sender, RoutedEventArgs e)
        {
            rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            listenNodeSoc?.Write(listenNode.Message);
        }

        private TM_Comms_ListenNode GetLNNode()
        {
            TM_Comms_ListenNode node = new TM_Comms_ListenNode();
            if (cmbLNDataType.SelectedIndex == 0)
            {
                node.Header = TM_Comms_ListenNode.HEADERS.TMSCT;

                txtLNScriptID.Text = node.ScriptID.ToString();
            }
            else
            {
                node.Header = TM_Comms_ListenNode.HEADERS.TMSTA;
                txtLNScriptID.Text = "";
            }


            node.Data = txtLNScriptData.Text;

            txtLNDataString.Text = node.Message;

            return node;
        }

        private void txtLNScriptData_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (listenNode != null)
            {
                listenNode.Data = txtLNScriptData.Text;
                txtLNDataString.Text = listenNode.Message;
            }
        }

        private void cmbLNDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listenNode = GetLNNode();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listenNodeSoc?.StopRecieveAsync();
            listenNodeSoc?.Disconnect();
        }

        private void btnLNSendMoves_Click(object sender, RoutedEventArgs e)
        {
            rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            listenNodeSoc?.Write(txtLNMovesCode.Text);
        }

        TM_Comms_MotionScriptBuilder msb;
        private void btnLNInsertMove_Click(object sender, RoutedEventArgs e)
        {
            TM_Comms_ListenNode ln = new TM_Comms_ListenNode(TM_Comms_ListenNode.HEADERS.TMSCT, "ListenSend(90, GetString(Robot[1].CoordRobot, 10, 3))");
            positionRequest = ln.ScriptID.ToString();
            listenNodeSoc?.Write(ln.Message);

        }

        MoveStep newMove;
        private bool GetMoveStep(string str)
        {
            List<string> spl = str.Split(',').ToList<string>();

            if (spl.Count != 11) return false;

            spl[0] = spl[0].Trim().ToLower();
            if (!spl[0].Equals("linear") & !spl[0].Equals("joint")) return false;
            string moveType = spl[0];
            spl.RemoveAt(0);

            spl[0] = spl[0].Trim().ToLower();
            if (!spl[0].Equals("pose") & !spl[0].Equals("joint")) return false;
            string posType = spl[0];
            spl.RemoveAt(0);

            List<double> pos = new List<double>();
            foreach(string s in spl)
            {
                if (!double.TryParse(s, out double res)) return false;
                pos.Add(res);
            }

            switch (moveType)
            {
                case "linear":
                    if (posType.Equals("pose"))
                        newMove = new MoveStep(new CPose(pos[0], pos[1], pos[2], pos[3], pos[4], pos[5], 0), MotionType.LINEAR, (int)pos[6], (int)pos[7], (int)pos[8]);
                    else if (posType.Equals("joint"))
                        newMove = new MoveStep(new Joint(pos[0], pos[1], pos[2], pos[3], pos[4], pos[5], 0), MotionType.LINEAR, (int)pos[6], (int)pos[7], (int)pos[8]);
                    else
                        return false;
                    break;
                case "joint":
                    if (posType.Equals("pose"))
                        newMove = new MoveStep(new CPose(pos[0], pos[1], pos[2], pos[3], pos[4], pos[5], 0), MotionType.JOINT, (int)pos[6], (int)pos[7], (int)pos[8]);
                    else if (posType.Equals("joint"))
                        newMove = new MoveStep(new Joint(pos[0], pos[1], pos[2], pos[3], pos[4], pos[5], 0), MotionType.JOINT, (int)pos[6], (int)pos[7], (int)pos[8]);
                    else
                        return false;
                    break;
                default:
                    return false;
            }

            return true;

        }

        private void btnLNValidateMoves_Click(object sender, RoutedEventArgs e)
        {
            string[] spl = txtLNMoves.Text.Split('\n');

            List<MoveStep> moves = new List<MoveStep>();

            foreach (string str in spl)
            {
                string s = str.Trim('\r').Trim();

                if (string.IsNullOrEmpty(s)) continue;

                if (GetMoveStep(s))
                    moves.Add(newMove);
            }

            msb = new TM_Comms_MotionScriptBuilder(moves);
            TM_Comms_ListenNode ln = msb.BuildScriptData();
            txtLNMovesCode.Text = ln.Message;
        }

    }
}
