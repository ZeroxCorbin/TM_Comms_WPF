using ApplicationSettingsNS;
using RingBuffer;
using SocketManagerNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TM_Comms;
using static TM_Comms.MotionScriptBuilder;

namespace TM_Comms_WPF
{
    public partial class ListenNodeWindow : Window
    {

        private ListenNode ListenNode { get; set; }

        private MotionScriptBuilder MotionScriptBuilder { get; set; }
        private MoveStep NewMove { get; set; }
        private string PositionRequest { get; set; } = null;

        //Public
        public ListenNodeWindow(Window owner)
        {
            DataContext = App.Settings;
            Owner = owner;

            InitializeComponent();

            Window_LoadSettings();

            ListenNode = GetLNNode();

            LoadCommandTreeView();
        }
        private void Window_LoadSettings()
        {
            if(double.IsNaN(App.Settings.ListenNodeWindow.Left)
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
            ConnectionInActive();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => CleanSock();


        private void LoadCommandTreeView()
        {
            TreeViewItem tviParent = null;
            foreach (string cmd in ListenNode.Commands[App.Settings.Version])
            {
                if (Regex.IsMatch(cmd, @"^[0-9][.][0-9]"))
                {
                    if (tviParent != null)
                    {
                        LstCommandList.Items.Add(tviParent);
                    }


                    tviParent = new TreeViewItem()
                    {
                        Header = cmd,
                    };
                    tviParent.MouseDoubleClick += TviParent_MouseDoubleClick;
                    continue;
                }
                TreeViewItem tviChild = new TreeViewItem()
                {
                    Header = cmd,
                };
                tviChild.MouseDoubleClick += TviChild_MouseDoubleClick;
                tviParent.Items.Add(tviChild);
            }
        }

        private void TviParent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void TviChild_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;
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


            insert += (string)tvi.Header;
            TxtScript.Text = TxtScript.Text.Insert(start, insert);

            TxtScript.SelectionStart = start + insert.Length;
        }
        private ListenNode GetLNNode()
        {
            ListenNode node;
            if (CmbMessageHeader.SelectedIndex == 0)

                node = new ListenNode(TxtScript.Text, ListenNode.Headers.TMSCT, TxtScriptID.Text);
            else
                node = new ListenNode((string)((ComboBoxItem)CmbMessageSubCommands.SelectedItem).Tag, ListenNode.Headers.TMSTA);

            TxtMessage.Text = node.Message;

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

        //Private
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
            BtnSendMoveMessage.IsEnabled = true;
            btnLNNewReadPosition.IsEnabled = true;

            BtnSendBadChecksum.IsEnabled = true;
            BtnSendBadHeader.IsEnabled = true;
            BtnSendBadPacket.IsEnabled = true;
            BtnSendBadPacketData.IsEnabled = true;

            BtnSendScriptExit.IsEnabled = true;
            BtnSendBadCode.IsEnabled = true;

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
            BtnSendMoveMessage.IsEnabled = false;
            btnLNNewReadPosition.IsEnabled = false;

            BtnSendBadChecksum.IsEnabled = false;
            BtnSendBadHeader.IsEnabled = false;
            BtnSendBadPacket.IsEnabled = false;
            BtnSendBadPacketData.IsEnabled = false;

            BtnSendScriptExit.IsEnabled = false;
            BtnSendBadCode.IsEnabled = false;
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

            ConnectionInActive();
            CleanSock();
        }
        private void ConnectThread(object sender)
        {
            Connect();
        }
        private bool Connect()
        {
            CleanSock();

            Socket = new SocketManager($"{App.Settings.RobotIP}:5890");

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
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        (Action)(() =>
                        {
                            ConnectionInActive();

                            if ((bool)ChkAutoReconnect.IsChecked)
                            {
                                ConnectionWaiting();
                                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnectThread));
                            }
                            else
                            {
                                ConnectionInActive();
                                CleanSock();
                            }

                        }));


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
            ListenNode ln = new ListenNode();

            if (!ln.ParseMessage(message))
                return;

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
                                TxtNewPosition.Text = pos;
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



        private void BtnSend_Click(object sender, RoutedEventArgs e) => Socket?.Write(ListenNode.Message);

        private void BtnLNNewReadPosition_Click(object sender, RoutedEventArgs e)
        {
            if (((string)((ComboBoxItem)CmbPositionType.SelectedItem).Tag) == "0")
            {
                ListenNode ln = new ListenNode("ListenSend(90, GetString(Robot[1].CoordRobot, 10, 3))");
                PositionRequest = ln.ScriptID.ToString();
                Socket?.Write(ln.Message);
            }
            else
            {
                ListenNode ln = new ListenNode("ListenSend(90, GetString(Robot[1].Joint, 10, 3))");
                PositionRequest = ln.ScriptID.ToString();
                Socket?.Write(ln.Message);
            }

        }
        private void BtnInsertMove_Click(object sender, RoutedEventArgs e)
        {
            string delim = ",";

            StringBuilder sb = new StringBuilder();

            if (((string)((ComboBoxItem)CmdMoveType.SelectedItem).Tag) == "0")
                sb.Append("Linear");
            else
                sb.Append("Joint");
            sb.Append(delim);

            if (((string)((ComboBoxItem)CmbPositionType.SelectedItem).Tag) == "0")
                sb.Append("Pose");
            else
                sb.Append("Joint");
            sb.Append(delim);

            sb.Append(TxtNewPosition.Text);
            sb.Append(delim);

            sb.Append((string)((ComboBoxItem)CmbLNMoveVelocity.SelectedItem).Tag);
            sb.Append(delim);

            sb.Append((string)((ComboBoxItem)CmbLNMoveAccel.SelectedItem).Tag);
            sb.Append(delim);

            sb.Append((string)((ComboBoxItem)CmbLNMoveBlend.SelectedItem).Tag);

            string insert = "";
            int start = TxtMoveList.SelectionStart;

            if (start == TxtMoveList.Text.Length)
            {
                if (start != 0)
                    if (TxtMoveList.Text[start - 1] != '\n')
                        insert += "\r\n";
            }
            else if (TxtMoveList.Text[start] == '\r')
            {
                if (start != 0)
                    if (TxtMoveList.Text[start - 1] != '\n')
                        insert += "\r\n";
            }


            insert += sb.ToString();
            TxtMoveList.Text = TxtMoveList.Text.Insert(start, insert);

            TxtMoveList.SelectionStart = start + insert.Length;
        }

        private void BtnGenerateMessage_Click(object sender, RoutedEventArgs e)
        {
            string[] spl = TxtMoveList.Text.Split('\n');

            List<MoveStep> moves = new List<MoveStep>();

            foreach (string str in spl)
            {
                string s = str.Trim('\r').Trim();

                if (string.IsNullOrEmpty(s)) continue;

                if (GetMoveStep(s))
                    moves.Add(NewMove);
            }

            MotionScriptBuilder = new MotionScriptBuilder(moves);
            ListenNode ln = MotionScriptBuilder.BuildScriptData((bool)ChkAddScriptExit.IsChecked, (bool)ChkInitializeVariables.IsChecked);
            TxtMoveMessage.Text = ln.Message;
        }
        private void BtnSendMoveMessage_Click(object sender, RoutedEventArgs e) => Socket?.Write(TxtMoveMessage.Text);

        private void TxtScript_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ListenNode != null)
            {
                ListenNode.Script = TxtScript.Text;
                TxtMessage.Text = ListenNode.Message;
            }
        }

        private void CmbMessageHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMessageHeader.SelectedIndex == 0)
            {
                LblMessageType.Content = "Scritp ID";
                CmbMessageSubCommands.Visibility = Visibility.Collapsed;
                TxtScriptID.Visibility = Visibility.Visible;

                LstCommandList.IsEnabled = true;
                TxtScript.IsEnabled = true;
            }
            else
            {
                LblMessageType.Content = "Sub Command";
                CmbMessageSubCommands.Visibility = Visibility.Visible;
                TxtScriptID.Visibility = Visibility.Collapsed;

                LstCommandList.IsEnabled = false;
                TxtScript.IsEnabled = false;
            }
            if (!IsLoaded) return;

            ListenNode = GetLNNode();
        }
        private void CmbMessageSubCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!IsLoaded) return;

            ListenNode = GetLNNode();
        }

        private void BtnSendScriptExit_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMSCT,17,diag,ScriptExit(),*5E\r\n");
        private void BtnSendBadCode_Click(object sender, RoutedEventArgs e) => Socket?.Write("$TMSCT,21,diag,int i=0\r\nint i=0,*52\r\n");

        private void BtnSendBadChecksum_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMSCT,25,1,ChangeBase(\"RobotBase\"),*09\r\n");
        private void BtnSendBadHeader_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMsct,25,1,ChangeBase(\"RobotBase\"),*28\r\n");
        private void BtnSendBadPacket_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMSCT,-100,1,ChangeBase(\"RobotBase\"),*13\r\n");
        private void BtnSendBadPacketData_Click(object sender, RoutedEventArgs e) => Socket?.Write($"$TMSTA,4,XXXX,*47\r\n");

        private void TxtScriptID_LostFocus(object sender, RoutedEventArgs e)
        {
            if(!IsLoaded) return;
            if (!Regex.IsMatch(TxtScriptID.Text, @"^[a-zA-Z0-9_]+$"))
                TxtScriptID.Text = "local";

            ListenNode = GetLNNode();
        }

        private List<System.Windows.Controls.TextBox> PositionBoxes { get; set; } = new List<System.Windows.Controls.TextBox>();

        private bool UpdatingPositionString = false;
        private void UpdatePositionString()
        {
            UpdatingPositionString = true;

            StringBuilder sb = new StringBuilder();

            int i = 0;
            foreach (TextBox txt in PositionBoxes)
                if (Regex.IsMatch(txt.Text, @"^[-+]?[0-9].*[.]?[0-9]*$"))
                {
                    if (i++ < PositionBoxes.Count - 1)
                        sb.Append($"{txt.Text},");
                    else
                        sb.Append($"{txt.Text}");
                }
                else
                {
                    if (i++ < PositionBoxes.Count - 1)
                        sb.Append($",");
                }

            TxtNewPosition.Text = sb.ToString();

            UpdatingPositionString = false;
        }

        private bool UpdatingPositionBoxes = false;
        private void UpdatePositionBoxes()
        {
            if (PositionBoxes.Count == 0)
            {
                PositionBoxes.Add(TxtPositionX);
                PositionBoxes.Add(TxtPositionY);
                PositionBoxes.Add(TxtPositionZ);
                PositionBoxes.Add(TxtPositionRX);
                PositionBoxes.Add(TxtPositionRY);
                PositionBoxes.Add(TxtPositionRZ);
            }

            UpdatingPositionBoxes = true;

            string[] spl = Regex.Split(TxtNewPosition.Text, @",");

            int i = 0;
            foreach (string val in spl)
                if (Regex.IsMatch(val, @"^[-+]?[0-9].*[.]?[0-9]*$"))
                    PositionBoxes[i++].Text = val;
                else
                    PositionBoxes[i++].Text = string.Empty;

            for (; i < PositionBoxes.Count; i++)
                PositionBoxes[i++].Text = string.Empty;

            UpdatingPositionBoxes = false;
        }

        private void TxtNewPosition_TextChanged(object sender, TextChangedEventArgs e)
        {


            if (!Regex.IsMatch(TxtNewPosition.Text, @"^[-+]?[0-9].*[.]?[0-9]*,[-+]?[0-9].*[.]?[0-9]*,[-+]?[0-9].*[.]?[0-9]*,[-+]?[0-9].*[.]?[0-9]*,[-+]?[0-9].*[.]?[0-9]*,[-+]?[0-9].*[.]?[0-9]*$"))
                TxtNewPosition.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19FF9900"));
            else
                TxtNewPosition.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19FFFF00"));

            if (!UpdatingPositionString)
                UpdatePositionBoxes();
        }

        private void TxtPosition_TextChanged(object sender, TextChangedEventArgs e)
        {


            TextBox tb = (TextBox)sender;

            if (!Regex.IsMatch(tb.Text, @"^[-+]?[0-9].*[.]?[0-9]*$"))
                tb.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19FF9900"));
            else
                tb.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19FFFF00"));

            if (!UpdatingPositionBoxes)
                UpdatePositionString();
        }

        private void CmbPositionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!IsLoaded) return;

            if (CmbPositionType.SelectedIndex == 0)
            {
                LblMB1.Content = "X";
                LblMB2.Content = "Y";
                LblMB3.Content = "Z";
                LblMB4.Content = "Rx";
                LblMB5.Content = "Ry";
                LblMB6.Content = "Rz";
            }
            else
            {
                LblMB1.Content = "J1";
                LblMB2.Content = "J2";
                LblMB3.Content = "J3";
                LblMB4.Content = "J4";
                LblMB5.Content = "J5";
                LblMB6.Content = "J6";
            }
        }
    }
}
