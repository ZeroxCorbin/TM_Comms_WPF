using SocketManagerNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace TM_Comms_WPF.Net
{
    public partial class ListenNodeWindow : Window
    {
        //Private
        private bool IsLoading { get; set; } = true;
        private SocketManager Socket { get; set; }
        private TM_Comms_ListenNode ListenNode { get; set; }

        private TM_Comms_MotionScriptBuilder MotionScriptBuilder { get; set; }
        private MoveStep NewMove { get; set; }
        private string PositionRequest { get; set; } = null;

        //Public
        public ListenNodeWindow()
        {
            InitializeComponent();

            ListenNode = GetLNNode();

            this.Left = App.Settings.ListenNodeWindow.Left;
            this.Top = App.Settings.ListenNodeWindow.Top;

            if (App.Settings.ListenNodeWindow.WindowState == WindowState.Maximized)
                RemoveMaxWidth();
            if (App.Settings.ListenNodeWindow.WindowState == WindowState.Normal)
                SetMaxWidth();

            this.WindowState = App.Settings.ListenNodeWindow.WindowState;

            IsLoading = false;

            ConnectionInActive();
        }

        //Private
        private void ConnectionActive()
        {
            btnLNConnect.Content = "Stop";
            btnLNConnect.Tag = 1;

            btnLNSend.IsEnabled = true;
            btnLNSendMoves.IsEnabled = true;
            btnLNNewReadPosition.IsEnabled = true;
        }
        private void ConnectionInActive()
        {
            btnLNConnect.Content = "Start";
            btnLNConnect.Tag = null;

            btnLNSend.IsEnabled = false;
            btnLNSendMoves.IsEnabled = false;
            btnLNNewReadPosition.IsEnabled = false;
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
            foreach (string s in spl)
            {
                if (!double.TryParse(s, out double res)) return false;
                pos.Add(res);
            }

            switch (moveType)
            {
                case "linear":
                    if (posType.Equals("pose"))
                        NewMove = new MoveStep(new CPose(pos[0], pos[1], pos[2], pos[3], pos[4], pos[5], 0), MotionType.LINEAR, (int)pos[6], (int)pos[7], (int)pos[8]);
                    else if (posType.Equals("joint"))
                        NewMove = new MoveStep(new Joint(pos[0], pos[1], pos[2], pos[3], pos[4], pos[5], 0), MotionType.LINEAR, (int)pos[6], (int)pos[7], (int)pos[8]);
                    else
                        return false;
                    break;
                case "joint":
                    if (posType.Equals("pose"))
                        NewMove = new MoveStep(new CPose(pos[0], pos[1], pos[2], pos[3], pos[4], pos[5], 0), MotionType.JOINT, (int)pos[6], (int)pos[7], (int)pos[8]);
                    else if (posType.Equals("joint"))
                        NewMove = new MoveStep(new Joint(pos[0], pos[1], pos[2], pos[3], pos[4], pos[5], 0), MotionType.JOINT, (int)pos[6], (int)pos[7], (int)pos[8]);
                    else
                        return false;
                    break;
                default:
                    return false;
            }

            return true;

        }

        private void CleanSock()
        {
            Socket?.StopRecieveAsync();
            Socket?.Disconnect();

            Socket?.Dispose();
            Socket = null;
        }
        private void Socket_ConnectState(object sender, SocketManager.SocketStateEventArgs data)
        {
            if (!data.State)
            {
                CleanSock();

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                        ConnectionInActive();
                    }));
            }
            else
            {
                Socket.StartRecieveAsync();

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        ConnectionActive();
                    }));

            }
        }
        private void Socket_DataReceived(object sender, SocketManager.SocketMessageEventArgs data)
        {
            if (PositionRequest != null)
            {
                string[] spl = data.Message.Trim('\0').Split(',');

                if (spl[0].Equals("$TMSTA"))
                {
                    if (spl[2].Equals("90"))
                    {
                        PositionRequest = null;

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

        private void BtnLNConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnLNConnect.Tag == null)
            {
                CleanSock();

                Socket = new SocketManager($"{App.Settings.RobotIP}:5890", null, Socket_ConnectState, Socket_DataReceived);
                if (Socket.Connect(true))
                { 
                    Socket.StartRecieveAsync();
                    btnLNConnect.Tag = "";
                }
                else
                    CleanSock();
            }
            else
            {
                CleanSock();
                ConnectionInActive();

                btnLNConnect.Tag = null;
            }
        }
        private void BtnLNSend_Click(object sender, RoutedEventArgs e)
        {
            rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            Socket?.Write(ListenNode.Message);
        }

        private void BtnLNNewReadPosition_Click(object sender, RoutedEventArgs e)
        {
            if(((string)((ComboBoxItem)CmdPositionType.SelectedItem).Tag) == "0")
            {
                TM_Comms_ListenNode ln = new TM_Comms_ListenNode(TM_Comms_ListenNode.HEADERS.TMSCT, "ListenSend(90, GetString(Robot[1].CoordRobot, 10, 3))");
                PositionRequest = ln.ScriptID.ToString();
                Socket?.Write(ln.Message);
            }
            else
            {
                TM_Comms_ListenNode ln = new TM_Comms_ListenNode(TM_Comms_ListenNode.HEADERS.TMSCT, "ListenSend(90, GetString(Robot[1].Joint, 10, 3))");
                PositionRequest = ln.ScriptID.ToString();
                Socket?.Write(ln.Message);
            }

        }
        private void BtnLNInsertMove_Click(object sender, RoutedEventArgs e)
        {
            string delim = ",";

            StringBuilder sb = new StringBuilder();

            if (((string)((ComboBoxItem)CmdMoveType.SelectedItem).Tag) == "0")
                sb.Append("Linear");
            else
                sb.Append("Joint");
            sb.Append(delim);

            if (((string)((ComboBoxItem)CmdPositionType.SelectedItem).Tag) == "0")
                sb.Append("Pose");
            else
                sb.Append("Joint");
            sb.Append(delim);

            sb.Append(txtLNNewPosition.Text);
            sb.Append(delim);

            sb.Append((string)((ComboBoxItem)CmbLNMoveVelocity.SelectedItem).Tag);
            sb.Append(delim);

            sb.Append("0");
            sb.Append(delim);

            sb.Append((string)((ComboBoxItem)CmbLNMoveBlend.SelectedItem).Tag);

            int start = txtLNMoves.SelectionStart;

            txtLNMoves.Text = txtLNMoves.Text.Insert(txtLNMoves.SelectionStart, sb.ToString());
            txtLNMoves.Focus();
            txtLNMoves.SelectionStart = start + sb.ToString().Length;

        }

        private void BtnLNValidateMoves_Click(object sender, RoutedEventArgs e)
        {
            string[] spl = txtLNMoves.Text.Split('\n');

            List<MoveStep> moves = new List<MoveStep>();

            foreach (string str in spl)
            {
                string s = str.Trim('\r').Trim();

                if (string.IsNullOrEmpty(s)) continue;

                if (GetMoveStep(s))
                    moves.Add(NewMove);
            }

            MotionScriptBuilder = new TM_Comms_MotionScriptBuilder(moves);
            TM_Comms_ListenNode ln = MotionScriptBuilder.BuildScriptData();
            txtLNMovesCode.Text = ln.Message;
        }
        private void BtnLNSendMoves_Click(object sender, RoutedEventArgs e)
        {
            rectCommandResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            Socket?.Write(txtLNMovesCode.Text);
        }

        private void TxtLNScriptData_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ListenNode != null)
            {
                ListenNode.Data = txtLNScriptData.Text;
                txtLNDataString.Text = ListenNode.Message;
            }
        }
        private void CmbLNDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading) return;

            ListenNode = GetLNNode();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Socket?.StopRecieveAsync();
            Socket?.Disconnect();
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoading) return;

            App.Settings.ListenNodeWindow.Top = Top;
            App.Settings.ListenNodeWindow.Left = Left;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                SetMaxWidth();
            if (this.WindowState == WindowState.Maximized)
                RemoveMaxWidth();

            if (IsLoading) return;
            if (this.WindowState == WindowState.Minimized) return;

            App.Settings.ListenNodeWindow.WindowState = this.WindowState;
        }

        private void SetMaxWidth()
        {
            this.SizeToContent = SizeToContent.WidthAndHeight;

            txtLNScriptData.MaxWidth = 600;
            txtLNDataString.MaxWidth = 600;
            txtLNMoves.MaxWidth = 600;
            txtLNMovesCode.MaxWidth = 600;
            txtLNDataResponse.MaxWidth = 600;
        }
        private void RemoveMaxWidth()
        {
            this.SizeToContent = SizeToContent.Manual;

            txtLNScriptData.MaxWidth = double.PositiveInfinity;
            txtLNDataString.MaxWidth = double.PositiveInfinity;
            txtLNMoves.MaxWidth = double.PositiveInfinity;
            txtLNMovesCode.MaxWidth = double.PositiveInfinity;
            txtLNDataResponse.MaxWidth = double.PositiveInfinity;
        }

        private void TxtLNMoves_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (txtLNMoves.IsSelectionActive)
                btnLNInsertMove.IsEnabled = true;
            else
                btnLNInsertMove.IsEnabled = false;
        }

    }
}
