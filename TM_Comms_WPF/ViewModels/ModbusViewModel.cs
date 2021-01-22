﻿using SimpleModbus;
using SocketManagerNS;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TM_Comms;
using TM_Comms_WPF.Commands;

namespace TM_Comms_WPF.ViewModels
{
    public class ModbusViewModel : INotifyPropertyChanged
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

        public string Title { get => "Modbus TCP"; }
        public double Left { get => App.Settings.ModbusWindow.Left; set { App.Settings.ModbusWindow.Left = value; OnPropertyChanged(); } }
        public double Top { get => App.Settings.ModbusWindow.Top; set { App.Settings.ModbusWindow.Top = value; OnPropertyChanged(); } }
        public double Width { get => App.Settings.ModbusWindow.Width; set { App.Settings.ModbusWindow.Width = value; OnPropertyChanged(); } }
        public double Height { get => App.Settings.ModbusWindow.Height; set { App.Settings.ModbusWindow.Height = value; OnPropertyChanged(); } }
        public WindowState WindowState { get => App.Settings.ModbusWindow.WindowState; set { App.Settings.ModbusWindow.WindowState = value; OnPropertyChanged(); } }

        private SocketManager Socket { get; set; }
        SimpleModbusTCP ModbusTCP { get; set; }

        private string connectButtonText = "Connect";
        private bool connectionState;
        private string message;

        private bool isRunning;
        private bool heartbeat;


        public string ConnectionString { get => App.Settings.RobotIP; set { App.Settings.RobotIP = value; OnPropertyChanged(); } }
        public string ConnectButtonText { get => connectButtonText; set => SetProperty(ref connectButtonText, value); }
        public bool ConnectionState { get => connectionState; set => SetProperty(ref connectionState, value); }
        public string Message { get => message; set { SetProperty(ref message, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMessage")); } }
        public bool IsMessage { get => !string.IsNullOrEmpty(message); }
        public bool IsRunning { get => isRunning; private set => SetProperty(ref isRunning, value); }
        public bool Heartbeat { get => heartbeat; set { SetProperty(ref heartbeat, value); } }

        public System.Windows.Visibility Border18Visible { get => App.Settings.Version >= TMflowVersions.V1_80_xxxx ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }


        private string errorDescription;
        private string errorCode;
        private string errorDate;
        public string ErrorDescription { get => errorDescription; set => SetProperty(ref errorDescription, value); }
        public string ErrorCode { get => errorCode; set => SetProperty(ref errorCode, value); }
        public string ErrorDate { get => errorDate; set => SetProperty(ref errorDate, value); }


        private Brush Good { get; } = new SolidColorBrush(Colors.Cyan);
        private Brush Bad { get; } = new SolidColorBrush(Colors.Red);
        private Brush Meh { get; } = new SolidColorBrush(Colors.Yellow);
        private Brush Disabled { get; } = new SolidColorBrush(Colors.White);
        private Brush GoodRadial { get; } = new RadialGradientBrush(Colors.Cyan, Colors.Transparent);
        private Brush BadRadial { get; } = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        private Brush MehRadial { get; } = new RadialGradientBrush(Colors.Yellow, Colors.Transparent);
        private Brush Transparent { get; } = new SolidColorBrush(Colors.Transparent);

        private Brush power = new SolidColorBrush(Colors.Yellow);
        private Brush manual = new SolidColorBrush(Colors.White);
        private Brush auto = new SolidColorBrush(Colors.White);
        private Brush error = new SolidColorBrush(Colors.White);
        private Brush estop = new SolidColorBrush(Colors.White);
        private Brush getControl = new SolidColorBrush(Colors.White);
        private Brush autoActive = new SolidColorBrush(Colors.White);
        private Brush autoEnable = new SolidColorBrush(Colors.White);

        private Brush stop = new SolidColorBrush(Colors.Transparent);
        private Brush play = new SolidColorBrush(Colors.Transparent);

        public Brush Power { get => power; set => SetProperty(ref power, value); }
        public Brush Manual { get => manual; set => SetProperty(ref manual, value); }
        public Brush Auto { get => auto; set => SetProperty(ref auto, value); }
        public Brush Error { get => error; set => SetProperty(ref error, value); }
        public Brush Estop { get => estop; set => SetProperty(ref estop, value); }
        public Brush GetControl { get => getControl; set => SetProperty(ref getControl, value); }
        public Brush AutoActive { get => autoActive; set => SetProperty(ref autoActive, value); }
        public Brush AutoEnable { get => autoEnable; set => SetProperty(ref autoEnable, value); }

        public Brush Stop { get => stop; set => SetProperty(ref stop, value); }
        public Brush Play { get => play; set => SetProperty(ref play, value); }

        public ObservableCollection<ModbusItemViewModel> Items { get; } = new ObservableCollection<ModbusItemViewModel>();
        public ObservableCollection<ModbusItemViewModel> UserItems { get; } = new ObservableCollection<ModbusItemViewModel>();

        public ICommand ConnectCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand PlusCommand { get; }
        public ICommand MinusCommand { get; }

        public ModbusViewModel()
        {
            PlayPauseCommand = new RelayCommand(PlayPauseAction, c => true);
            PlusCommand = new RelayCommand(PlusAction, c => true);
            MinusCommand = new RelayCommand(MinusAction, c => true);
            StopCommand = new RelayCommand(StopAction, c => true);
            ConnectCommand = new RelayCommand(ConnectAction, c => true);

            Socket = new SocketManager($"{App.Settings.RobotIP}:502");
            ModbusTCP = new SimpleModbusTCP(Socket);

            if (Socket.IsConnected)
                ConnectButtonText = "Close";

            Socket.ConnectState += Socket_ConnectState;

            GetItems();

            if (Socket.IsConnected && !IsRunning)
                ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncRecieveThread_DoWork));
        }

        private void GetItems()
        {
            foreach (var kv in ModbusDictionary.ModbusData[App.Settings.Version])
                Items.Add(new ModbusItemViewModel(kv.Key, kv.Value, ModbusTCP));

            for (int i = 9000; i < 9041; i += 4)
            {
                UserItems.Add(new ModbusItemViewModel(i.ToString(), new ModbusDictionary.MobusValue() { Access = ModbusDictionary.MobusValue.AccessTypes.RW, Addr = i, Type = ModbusDictionary.MobusValue.DataTypes.Float }, ModbusTCP));
            }
        }

        private void ConnectAction(object parameter)
        {
            if (Socket.IsConnected)
            {

                Cancel = true;
                while (isRunning) Thread.Sleep(1);

                Socket.StopReceiveAsync();
                Socket.Close();
            }
            else
            {
                Message = string.Empty;

                Socket.ConnectionString = $"{App.Settings.RobotIP}:502";
                if (Socket.Connect())
                {
                    if (Socket.IsConnected && !IsRunning)
                        ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncRecieveThread_DoWork));
                }
                else
                    Message = Socket.IsException ? Socket.Exception.Message : "Unable to connect!";
            }
        }

        private void StopAction(object parameter)
        {
            ModbusTCP.WriteSingleCoil(ModbusDictionary.ModbusData[App.Settings.Version]["Stop"].Addr, true);
        }
        private void PlayPauseAction(object parameter)
        {
            ModbusTCP.WriteSingleCoil(ModbusDictionary.ModbusData[App.Settings.Version]["Play/Pause"].Addr, true);
        }
        private void PlusAction(object parameter)
        {
            ModbusTCP.WriteSingleCoil(ModbusDictionary.ModbusData[App.Settings.Version]["Stick+"].Addr, true);
        }
        private void MinusAction(object parameter)
        {
            ModbusTCP.WriteSingleCoil(ModbusDictionary.ModbusData[App.Settings.Version]["Stick-"].Addr, true);
        }
        private void Socket_ConnectState(object sender, bool state)
        {
            ConnectionState = state;
            if (state)
                ConnectButtonText = "Close";
            else
            {
                ConnectButtonText = "Connect";
                Heartbeat = false;
            }

        }

        private bool Cancel { get; set; } = false;
        private void AsyncRecieveThread_DoWork(object sender)
        {
            IsRunning = true;

            while (!Cancel)
            {
                if (Socket.IsConnected)
                {
                    try
                    {
                        Heartbeat = !Heartbeat;

                        ReadModbus();

                        if (Cancel) break;

                        foreach (ModbusItemViewModel mvi in Items)
                            mvi.Read();

                        foreach (ModbusItemViewModel mvi in UserItems)
                            mvi.Read();
                    }
                    catch
                    {
                        break;
                    }
                }
                else
                    Reset();
            }

            IsRunning = false;
            Cancel = false;

            Reset();
        }
        private void Reset()
        {
            Power = Meh;
            Manual = Disabled;
            Auto = Disabled;
            Estop = Disabled;
            GetControl = Disabled;
            AutoActive = Disabled;
            AutoEnable = Disabled;

            Stop = Transparent;
            Play = Transparent;

            Error = Disabled;
            ErrorCode = "";
            ErrorDate = "";
            ErrorDescription = "";
        }
        private void ReadModbus()
        {
            Power = Good;

            if (ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["M/A Mode"].Addr) == 1)
            {
                Auto = Good;
                Manual = Disabled;
            }
            else
            {
                Auto = Disabled;
                Manual = Good;
            }

            if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["EStop"].Addr))
                Estop = Bad;
            else
                Estop = Disabled;

            if (App.Settings.Version == TMflowVersions.V1_80_xxxx)
            {
                if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Get Control"].Addr))
                    GetControl = Good;
                else
                    GetControl = Bad;

                if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Active"].Addr))
                    AutoActive = Good;
                else
                    AutoActive = Bad;

                if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Enabled"].Addr))
                    AutoEnable = Good;
                else
                    AutoEnable = Bad;
            }

            if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Project Running"].Addr))
            {
                Play = GoodRadial;
                Stop = Transparent;
            }
            else if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Project Paused"].Addr))
            {
                Play = MehRadial;
                Stop = Transparent;
            }
            else if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Project Editing"].Addr))
            {
                Play = Transparent;
                Stop = MehRadial;
            }
            else
            {
                Play = Transparent;
                Stop = BadRadial;
            }

            if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Error"].Addr))
                Error = Bad;
            else
                Error = Disabled;


            uint code = (uint)ModbusTCP.GetInt32(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Code"].Addr);
            if (code != 0)
            {
                string dat = $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Month"].Addr)}/" +
                                $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Date"].Addr)}/" +
                                $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Year"].Addr)} " +
                                $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Hour"].Addr)}:" +
                                $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Minute"].Addr)}:" +
                                $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Second"].Addr)} ";
                if (DateTime.TryParse(dat, out DateTime date))
                    ErrorDate = date.ToString();
            }
            else
                ErrorDate = "";

            ErrorCode = code.ToString("X");

            if (ErrorCodes.Codes.TryGetValue(code, out string val))
                ErrorDescription = val;
            else
                ErrorDescription = "CAN NOT FIND ERROR IN TABLE.";
        }
    }
}
