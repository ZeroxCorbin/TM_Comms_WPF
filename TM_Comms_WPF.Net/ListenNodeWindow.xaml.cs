using ApplicationSettingsNS;
using RingBuffer;
using SocketManagerNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using static TM_Comms_WPF.MotionScriptBuilder;

namespace TM_Comms_WPF
{
    public partial class ListenNodeWindow : Window
    {
        //Private
        private bool IsLoading { get; set; } = true;
        private ListenNode ListenNode { get; set; }

        private MotionScriptBuilder MotionScriptBuilder { get; set; }
        private MoveStep NewMove { get; set; }
        private string PositionRequest { get; set; } = null;

        //Public
        public ListenNodeWindow()
        {
            InitializeComponent();

            ListenNode = GetLNNode();

            if (Keyboard.IsKeyDown(Key.LeftShift))
                App.Settings.ListenNodeWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();

            this.Left = App.Settings.ListenNodeWindow.Left;
            this.Top = App.Settings.ListenNodeWindow.Top;

            this.WindowState = App.Settings.ListenNodeWindow.WindowState;

            if (!CheckOnScreen.IsOnScreen(this))
            {
                App.Settings.ListenNodeWindow = new ApplicationSettings_Serializer.ApplicationSettings.WindowSettings();

                this.Left = App.Settings.ListenNodeWindow.Left;
                this.Top = App.Settings.ListenNodeWindow.Top;

                this.WindowState = App.Settings.ListenNodeWindow.WindowState;
            }

            LstCommandList.ItemsSource = ListenNode.Commands[App.Settings.Version];

            IsLoading = false;

            ConnectionInActive();
        }

        //Private
        private void ConnectionActive()
        {
            btnLNSend.IsEnabled = true;
            btnLNSendMoves.IsEnabled = true;
            btnLNNewReadPosition.IsEnabled = true;
        }
        private void ConnectionInActive()
        {
            //RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));

            BtnConnect.Content = "Connect";
            BtnConnect.Tag = null;

            btnLNSend.IsEnabled = false;
            btnLNSendMoves.IsEnabled = false;
            btnLNNewReadPosition.IsEnabled = false;
        }
        private ListenNode GetLNNode()
        {
            ListenNode node = new ListenNode();
            if (cmbLNDataType.SelectedIndex == 0)
            {
                node.Header = ListenNode.HEADERS.TMSCT;

                txtLNScriptID.Text = node.ScriptID.ToString();
            }
            else
            {
                node.Header = ListenNode.HEADERS.TMSTA;
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

            Socket = new SocketManager($"{App.Settings.RobotIP}:5890");

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
            {
                CleanSock();
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        (Action)(() =>
                        {
                            ConnectionActive();
                        }));

                Socket.MessageReceived += Socket_MessageReceived;

                Socket.StartReceiveMessages(@"[$]", @"[*][A-Z0-9][A-Z0-9]");
            }
        }
        //Receive Data
        private void Socket_MessageReceived(object sender, string message, string pattern)
        {
            if (PositionRequest != null)
            {
                if (Regex.IsMatch(message, @"^[$]TMSTA,\w*,90,"))
                {
                        PositionRequest = null;

                        string[] spl = message.Split('{');
                        string pos = spl[1].Substring(0, spl[1].IndexOf('}'));

                        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (Action)(() =>
                                {
                                    txtLNNewPosition.Text = pos;
                                }));
                  
                }
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        //RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                        txtLNDataResponse.Text += message + "\r\n";
                        txtLNDataResponse.ScrollToEnd();
                    }));

        }
        //Window Changes
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoading) return;

            App.Settings.ListenNodeWindow.Top = Top;
            App.Settings.ListenNodeWindow.Left = Left;
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (IsLoading) return;
            if (this.WindowState == WindowState.Minimized) return;

            App.Settings.ListenNodeWindow.WindowState = this.WindowState;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => CleanSock();


        private void BtnLNSend_Click(object sender, RoutedEventArgs e)
        {
            //RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            Socket?.Write(ListenNode.Message);
        }

        private void BtnLNNewReadPosition_Click(object sender, RoutedEventArgs e)
        {
            if (((string)((ComboBoxItem)CmdPositionType.SelectedItem).Tag) == "0")
            {
                ListenNode ln = new ListenNode(ListenNode.HEADERS.TMSCT, "ListenSend(90, GetString(Robot[1].CoordRobot, 10, 3))");
                PositionRequest = ln.ScriptID.ToString();
                Socket?.Write(ln.Message);
            }
            else
            {
                ListenNode ln = new ListenNode(ListenNode.HEADERS.TMSCT, "ListenSend(90, GetString(Robot[1].Joint, 10, 3))");
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

            sb.Append((string)((ComboBoxItem)CmbLNMoveAccel.SelectedItem).Tag);
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

            MotionScriptBuilder = new MotionScriptBuilder(moves);
            ListenNode ln = MotionScriptBuilder.BuildScriptData();
            txtLNMovesCode.Text = ln.Message;
        }
        private void BtnLNSendMoves_Click(object sender, RoutedEventArgs e)
        {
            //RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
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




        private void TxtLNMoves_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (txtLNMoves.IsSelectionActive)
                btnLNInsertMove.IsEnabled = true;
            else
                btnLNInsertMove.IsEnabled = false;
        }

    }
}
