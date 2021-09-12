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

namespace TM_Comms_WPF.ViewModels
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
            //if (PositionRequest != null)
            //{
            //    if (Regex.IsMatch(message, @"^[$]TMSTA,\w*,90,"))
            //    {
            //        PositionRequest = null;

            //        string[] spl = message.Split('{');
            //        string pos = spl[1].Substring(0, spl[1].IndexOf('}'));

            //        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //                (Action)(() =>
            //                {
            //                    TxtNewPosition.Text = pos;
            //                }));

            //    }
            //}

            //Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //        (Action)(() =>
            //        {
            //            //RectCommandHasResponse.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            //            txtLNDataResponse.Text += message + "\r\n";
            //            txtLNDataResponse.ScrollToEnd();
            //        }));

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

                if((string)value.Tag == "0")
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
    }
}
