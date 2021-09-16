using SocketManagerNS;
using TM_Comms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TM_Comms_WPF.Commands;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using TM_Comms_WPF.ControlViewModels;
using static TM_Comms.MotionScriptBuilder;
using TM_Comms_WPF.ControlViews;

namespace TM_Comms_WPF.WindowViewModels
{
    public class ListenNodeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public string Title => "Listen Node";
        public double Left { get => App.Settings.ListenNodeWindow.Left; set { App.Settings.ListenNodeWindow.Left = value; OnPropertyChanged(); } }
        public double Top { get => App.Settings.ListenNodeWindow.Top; set { App.Settings.ListenNodeWindow.Top = value; OnPropertyChanged(); } }
        public double Width { get => App.Settings.ListenNodeWindow.Width; set { App.Settings.ListenNodeWindow.Width = value; OnPropertyChanged(); } }
        public double Height { get => App.Settings.ListenNodeWindow.Height; set { App.Settings.ListenNodeWindow.Height = value; OnPropertyChanged(); } }
        public WindowState WindowState { get => App.Settings.ListenNodeWindow.WindowState; set { App.Settings.ListenNodeWindow.WindowState = value; OnPropertyChanged(); } }

        public string ConnectionString { get => $"{App.Settings.RobotIP}:5890"; }
        public string ConnectButtonText { get => connectButtonText; set => SetProperty(ref connectButtonText, value); }
        private string connectButtonText = "Connect";
        public bool ConnectionState { get => connectionState; set => SetProperty(ref connectionState, value); }
        private bool connectionState;
        public string ConnectMessage { get => connectMessage; set { _ = SetProperty(ref connectMessage, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMessage")); } }
        private string connectMessage;

        public bool AutoReconnect { get => autoReconnect; set => SetProperty(ref autoReconnect, value); }
        private bool autoReconnect;
        public bool IsConnectMessage { get => !string.IsNullOrEmpty(connectMessage); }


        public string LNMessage { get => lnMessage; set => SetProperty(ref lnMessage, value); }
        private string lnMessage;
        public string LNCommandResponse { get => lnCommandResponse; set => SetProperty(ref lnCommandResponse, value); }
        private string lnCommandResponse;

        public ICommand ConnectCommand { get; }
        public ICommand SendCommand { get; }

        public ICommand SendBadChecksumCommand { get; }
        public ICommand SendBadHeaderCommand { get; }
        public ICommand SendBadPacketCommand { get; }
        public ICommand SendBadPacketDataCommand { get; }

        public ICommand SendScriptExitCommand { get; }
        public ICommand SendBadCodeCommand { get; }

        public ICommand ReadPosition { get; }
        public ICommand InsertMoveStep { get; }
        public ICommand GenerateMotionScript { get; }
        public ICommand SendMotionScript { get; }
        public PositionControlViewModel PositionControl { get; } = new PositionControlViewModel();

        public void ViewClosing()
        {
            Socket.StopReceiveAsync();
            Socket.Close();

            AutoReconnect = false;
        }
        public ListenNodeViewModel()
        {
            ConnectCommand = new RelayCommand(ConnectAction, c => true);
            SendCommand = new RelayCommand(SendCommandAction, c => true);

            SendBadChecksumCommand = new RelayCommand(SendBadChecksumAction, c => true);
            SendBadHeaderCommand = new RelayCommand(SendBadHeaderAction, c => true);
            SendBadPacketCommand = new RelayCommand(SendBadPacketAction, c => true);
            SendBadPacketDataCommand = new RelayCommand(SendBadPacketDataAction, c => true);

            SendScriptExitCommand = new RelayCommand(SendScriptExit, c => true);
            SendBadCodeCommand = new RelayCommand(SendBadCode, c => true);

            ReadPosition = new RelayCommand(ReadPositionAction, c => true);
            InsertMoveStep = new RelayCommand(InsertMoveStepAction, c => true);
            GenerateMotionScript = new RelayCommand(GenerateMotionScriptAction, c => true);
            SendMotionScript = new RelayCommand(SendMotionScriptAction, c => true);

            Socket = new SocketManager(ConnectionString);
            Socket.ConnectState += Socket_ConnectState;
            Socket.MessageReceived += Socket_MessageReceived;

            LoadCommandTreeView();
        }


        private SocketManager Socket { get; set; }
        private void ConnectAction(object parameter)
        {
            if (Socket.IsConnected)
            {
                Socket.StopReceiveAsync();
                Socket.Close();

                AutoReconnect = false;
            }
            else
            {
                ConnectMessage = string.Empty;

                Task.Run(() =>
                  {
                      ConnectButtonText = "Trying";

                      if (!Socket.Connect())
                          ConnectMessage = Socket.IsException ? Socket.Exception.Message : "Unable to connect!";
                      else
                          ConnectMessage = "Connected";
                  });
            }
        }
        private void Socket_ConnectState(object sender, bool state)
        {
            ConnectionState = state;

            if (state)
            {
                ConnectButtonText = "Close";

                Socket.StartReceiveMessages(@"[$]", @"[*][A-Z0-9][A-Z0-9]");
            }
            else
            {
                ConnectButtonText = "Connect";

                if (AutoReconnect)
                    Task.Run(() =>
                    {
                        ConnectButtonText = "Trying";

                        if (!Socket.Connect())
                            ConnectMessage = Socket.IsException ? Socket.Exception.Message : "Unable to connect!";
                        else
                            ConnectMessage = "Connected";
                    });
            }
        }
        private void Socket_MessageReceived(object sender, string message, string pattern)
        {
            ListenNode ln = new ListenNode();

            if (!ln.ParseMessage(message))
                return;

            LNCommandResponse += message + "\r\n";

            if (PositionRequest != null)
            {
                if (Regex.IsMatch(message, @"^[$]TMSTA,\w*,90,"))
                {
                    PositionRequest = null;

                    string[] spl = message.Split('{');
                    string pos = spl[1].Substring(0, spl[1].IndexOf('}'));

                    PositionControl.Position = pos;
                }
            }
        }


        private void SendBadChecksumAction(object parameter) => Socket?.Write($"$TMSCT,25,1,ChangeBase(\"RobotBase\"),*09\r\n");
        private void SendBadHeaderAction(object parameter) => Socket?.Write($"$TMsct,25,1,ChangeBase(\"RobotBase\"),*28\r\n");
        private void SendBadPacketAction(object parameter) => Socket?.Write($"$TMSCT,-100,1,ChangeBase(\"RobotBase\"),*13\r\n");
        private void SendBadPacketDataAction(object parameter) => Socket?.Write($"$TMSTA,4,XXXX,*47\r\n");

        private void SendScriptExit(object parameter) => Socket?.Write($"$TMSCT,17,diag,ScriptExit(),*5E\r\n");
        private void SendBadCode(object parameter) => Socket?.Write("$TMSCT,21,diag,int i=0\r\nint i=0,*52\r\n");


        //public ListViewItem CommandItem { get => commandItem; set { _ = SetProperty(ref commandItem, value); UpdateScript(); } }
        //private ListViewItem commandItem;

        public ComboBoxItem MessageHeader
        {
            get => messageHeader;
            set
            {
                _ = SetProperty(ref messageHeader, value);

                if ((string)value.Tag == "0")
                {
                    SubCommandVisible = Visibility.Collapsed;
                    ScriptIDVisible = Visibility.Visible;
                }
                else
                {
                    SubCommandVisible = Visibility.Visible;
                    ScriptIDVisible = Visibility.Collapsed;
                }

                GetLNNode();
            }
        }
        private ComboBoxItem messageHeader;

        public Visibility ScriptIDVisible { get => scriptIDVisible; set { _ = SetProperty(ref scriptIDVisible, value); } }
        private Visibility scriptIDVisible = Visibility.Visible;
        public string ScriptID { get => scritpID; set { _ = SetProperty(ref scritpID, value); GetLNNode(); } }
        private string scritpID = "local";

        public Visibility SubCommandVisible { get => subCommandVisible; set { _ = SetProperty(ref subCommandVisible, value); } }
        private Visibility subCommandVisible = Visibility.Collapsed;
        public ComboBoxItem SubCommand { get => subCommand; set { _ = SetProperty(ref subCommand, value); GetLNNode(); } }
        private ComboBoxItem subCommand;

        public ObservableCollection<TreeViewItem> CommandList { get; } = new ObservableCollection<TreeViewItem>();
        public TreeViewItem CommandItem { get => commandItem; set { _ = SetProperty(ref commandItem, value); UpdateScript(); } }
        private TreeViewItem commandItem;

        public string Script { get => script; set { _ = SetProperty(ref script, value); GetLNNode(); } }
        private string script;

        public string Command { get => command; set => SetProperty(ref command, value); }
        private string command;
        private void LoadCommandTreeView()
        {
            TreeViewItem tviParent = null;
            foreach (string cmd in ListenNode.Commands[App.Settings.Version])
            {
                if (Regex.IsMatch(cmd, @"^[0-9][.][0-9]"))
                {
                    tviParent = new TreeViewItem()
                    {
                        Header = cmd,
                    };

                    CommandList.Add(tviParent);

                    continue;
                }
                TreeViewItem tviChild = new TreeViewItem()
                {
                    Header = cmd,
                };
                tviParent.Items.Add(tviChild);
            }
        }
        private void UpdateScript()
        {
            if (CommandItem == null) return;
            if (CommandItem.HasItems) return;

            if (string.IsNullOrEmpty(Script))
                Script = CommandItem.Header.ToString();
            else
                Script += "\r\n" + CommandItem.Header.ToString();
        }
        private ListenNode GetLNNode()
        {
            ListenNode node;
            if ((string)MessageHeader.Tag == "0")
                node = new ListenNode(Script, ListenNode.Headers.TMSCT, scritpID);
            else
                node = new ListenNode((string)subCommand.Tag, ListenNode.Headers.TMSTA);

            Command = node.Message;

            return node;
        }
        private void SendCommandAction(object parameter) => Socket?.Write(GetLNNode().Message);

        public ObservableCollection<ListBoxItem> PositionControlList { get; } = new ObservableCollection<ListBoxItem>();
        public bool AddScriptExit { get => addScriptExit; set => SetProperty(ref addScriptExit, value); }
        private bool addScriptExit = true;
        public bool InitializeVariables { get => initializeVariables; set => SetProperty(ref initializeVariables, value); }
        private bool initializeVariables;

        public string MotionScript { get => motionScript; set => SetProperty(ref motionScript, value); }
        private string motionScript = "";

        private string PositionRequest = null;
        public void ReadPositionAction(object parameter)
        {
            char[] type = PositionControl.DataFormat.ToCharArray();
            if (type[0] == 'C')
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

        public void InsertMoveStepAction(object parameter)
        {
            PositionControlViewModel pcvm = new PositionControlViewModel();
            pcvm.MoveStep = PositionControl.MoveStep; 
            pcvm.DragDropEvent += PositionControl_DragDropEvent;
            pcvm.Labels = Visibility.Collapsed;
            pcvm.DragDropTarget = Visibility.Visible;

            ListBoxItem lbi = new ListBoxItem() { Content = new ControlViews.PositionControl() { DataContext = pcvm } };


            //lbi.PreviewMouseMove += Lbi_PreviewMouseMove;
            //lbi.Drop += Lbi_Drop;
            //lbi.PreviewMouseLeftButtonDown += Lbi_PreviewMouseLeftButtonDown;
            lbi.AllowDrop = true;
            PositionControlList.Add(lbi);
        }

        private ListBoxItem draggedItem;
        private void PositionControl_DragDropEvent(object parent, bool drop)
        {

            if (drop)
            {
                //ListBoxItem droppedData = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
                ListBoxItem target = (ListBoxItem)parent;

                int removedIdx = PositionControlList.IndexOf(draggedItem);
                int targetIdx = PositionControlList.IndexOf(target);

                if (removedIdx < targetIdx)
                {
                    PositionControlList.Insert(targetIdx + 1, draggedItem);
                    PositionControlList.RemoveAt(removedIdx);
                }
                else
                {
                    int remIdx = removedIdx + 1;
                    if (PositionControlList.Count + 1 > remIdx)
                    {
                        PositionControlList.Insert(targetIdx, draggedItem);
                        PositionControlList.RemoveAt(remIdx);
                    }
                }
            }
            else
            {
                if (parent is ListBoxItem)
                {
                    draggedItem = parent as ListBoxItem;

                    draggedItem.Dispatcher.BeginInvoke(new Action(() => { DragDrop.DoDragDrop(draggedItem, draggedItem, DragDropEffects.Move); }));

                    draggedItem.IsSelected = true;
                }
            }

        }
        //private void Lbi_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (sender is ListBoxItem)
        //    {
        //        ListBoxItem draggedItem = sender as ListBoxItem;

        //        draggedItem.Dispatcher.BeginInvoke(new Action(() => { DragDrop.DoDragDrop(draggedItem, draggedItem, DragDropEffects.Move); }));

        //        draggedItem.IsSelected = true;
        //    }
        // }

        private void Lbi_Drop(object sender, DragEventArgs e)
        {
            //ListBoxItem droppedData = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
            //ListBoxItem target = (ListBoxItem)sender;

            //int removedIdx = PositionControlList.IndexOf(droppedData);
            //int targetIdx = PositionControlList.IndexOf(target);

            //if (removedIdx < targetIdx)
            //{
            //    PositionControlList.Insert(targetIdx + 1, droppedData);
            //    PositionControlList.RemoveAt(removedIdx);
            //}
            //else
            //{
            //    int remIdx = removedIdx + 1;
            //    if (PositionControlList.Count + 1 > remIdx)
            //    {
            //        PositionControlList.Insert(targetIdx, droppedData);
            //        PositionControlList.RemoveAt(remIdx);
            //    }
            //}
        }

        private void Lbi_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed)
            //    if (sender is ListBoxItem)
            //    {
            //        ListBoxItem draggedItem = sender as ListBoxItem;

            //        draggedItem.Dispatcher.BeginInvoke(new Action(() => { DragDrop.DoDragDrop(draggedItem, draggedItem, DragDropEffects.Move); }));

            //        draggedItem.IsSelected = true;
            //    }
        }
        public void DeleteMoveStep(object e)
        {
            ListBoxItem droppedData = e as ListBoxItem;
            int removedIdx = PositionControlList.IndexOf(droppedData);

            PositionControlList.RemoveAt(removedIdx);
        }

        private void GenerateMotionScriptAction(object p)
        {
            MotionScriptBuilder msb = new MotionScriptBuilder();

            foreach (ListBoxItem lbi in PositionControlList)
            {
                msb.Moves.Add(((PositionControlViewModel)((PositionControl)lbi.Content).DataContext).MoveStep);
            }
            ListenNode ln = msb.BuildScriptData(AddScriptExit, InitializeVariables);

            MotionScript = ln.Message;
        }
        private void SendMotionScriptAction(object p) => Socket.Write(MotionScript);
    }
}
