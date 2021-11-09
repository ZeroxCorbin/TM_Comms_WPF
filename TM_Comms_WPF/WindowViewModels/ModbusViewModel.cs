﻿using SimpleModbus;
using SocketManagerNS;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TM_Comms;
using TM_Comms_WPF.Commands;
using TM_Comms_WPF.ControlViewModels;

namespace TM_Comms_WPF.WindowViewModels
{
    public class ModbusViewModel : BaseViewModel
    {

        public string Title => "Modbus TCP";
        public double Left { get => App.Settings.ModbusWindow.Left; set { App.Settings.ModbusWindow.Left = value; OnPropertyChanged(); } }
        public double Top { get => App.Settings.ModbusWindow.Top; set { App.Settings.ModbusWindow.Top = value; OnPropertyChanged(); } }
        public double Width { get => App.Settings.ModbusWindow.Width; set { App.Settings.ModbusWindow.Width = value; OnPropertyChanged(); } }
        public double Height { get => App.Settings.ModbusWindow.Height; set { App.Settings.ModbusWindow.Height = value; OnPropertyChanged(); } }
        public WindowState WindowState { get => App.Settings.ModbusWindow.WindowState; set { App.Settings.ModbusWindow.WindowState = value; OnPropertyChanged(); } }

        public PendantControlViewModel Pendant { get; } = new PendantControlViewModel();

        private SocketManager Socket { get; set; }
        private SimpleModbusTCP ModbusTCP { get; set; }

        private bool heartbeat;

        public string ConnectionString { get => App.Settings.RobotIP; set { App.Settings.RobotIP = value; OnPropertyChanged(); } }
        public string ConnectButtonText { get => connectButtonText; set => SetProperty(ref connectButtonText, value); }
        private string connectButtonText = "Connect";
        public bool ConnectionState { get => connectionState; set => SetProperty(ref connectionState, value); } 
        private bool connectionState;
        public string ConnectMessage { get => connectMessage; set { _ = SetProperty(ref connectMessage, value); OnPropertyChanged("IsMessage"); } }
        private string connectMessage;

        public string Message { get => message; set { _ = SetProperty(ref message, value); OnPropertyChanged("IsMessage"); } } 
        private string message;
        public bool IsMessage { get => !string.IsNullOrEmpty(message); }
        public bool IsRunning { get => isRunning; private set => SetProperty(ref isRunning, value); }
        private bool isRunning;
        public bool Heartbeat { get => heartbeat; set { _ = SetProperty(ref heartbeat, value); } }

        public System.Windows.Visibility Border18Visible { get => App.Settings.Version >= TMflowVersions.V1_80_xxxx ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }

        public ICommand ConnectCommand { get; }
        //public ICommand StopCommand { get; }
        //public ICommand PlayPauseCommand { get; }
        //public ICommand PlusCommand { get; }
        //public ICommand MinusCommand { get; }
        public ObservableCollection<ModbusItemViewModel> Items { get; } = new ObservableCollection<ModbusItemViewModel>();
        public ObservableCollection<ModbusItemViewModel> UserItems { get; } = new ObservableCollection<ModbusItemViewModel>();


        public ModbusViewModel()
        {
            ConnectCommand = new RelayCommand(ConnectAction, c => true);

            Pendant.StopEvent += Pendant_StopEvent;
            Pendant.PlayPauseEvent += Pendant_PlayPauseEvent;
            Pendant.PlusEvent += Pendant_PlusEvent;
            Pendant.MinusEvent += Pendant_MinusEvent;

            Socket = new SocketManager($"{App.Settings.RobotIP}:502");
            ModbusTCP = new SimpleModbusTCP(Socket);

            if (Socket.IsConnected)
                ConnectButtonText = "Close";

            Socket.ConnectState += Socket_ConnectState;

            GetItems();

            if (Socket.IsConnected && !IsRunning)
                ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncRecieveThread_DoWork));
        }

        public void ViewClosing()
        {
            Socket.StopReceiveAsync();
            Socket.Close();
        }

        private void Pendant_StopEvent() => ModbusTCP.WriteSingleCoil(ModbusDictionary.ModbusData[App.Settings.Version]["Stop"].Addr, true);
        private void Pendant_PlayPauseEvent() => ModbusTCP.WriteSingleCoil(ModbusDictionary.ModbusData[App.Settings.Version]["Play/Pause"].Addr, true);
        private void Pendant_PlusEvent() => ModbusTCP.WriteSingleCoil(ModbusDictionary.ModbusData[App.Settings.Version]["Stick+"].Addr, true);
        private void Pendant_MinusEvent() => ModbusTCP.WriteSingleCoil(ModbusDictionary.ModbusData[App.Settings.Version]["Stick-"].Addr, true);

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
                Socket.StopReceiveAsync();
                Socket.Close();
            }
            else
            {
                ConnectMessage = string.Empty;

                Task.Run(() =>
                {
                    ConnectButtonText = "Trying";

                    if (!Socket.Connect())
                        ConnectMessage = Socket.IsException ? Socket.Exception.Message : "Unable to connect!";
                });
            }
        }

        private void Socket_ConnectState(object sender, bool state)
        {
            ConnectionState = state;

            if (state)
            {
                ConnectButtonText = "Close";

                if (!IsRunning)
                     ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncRecieveThread_DoWork));
            }

            else
            {
                Cancel = true;
                while (isRunning) Thread.Sleep(1);

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

                        UpdatePendant();

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
                    Pendant.Reset();
            }

            IsRunning = false;
            Cancel = false;

            Pendant.Reset();
        }
        private void UpdatePendant()
        {
            Pendant.Power = Pendant.Good;

            if (ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["M/A Mode"].Addr) == 1)
            {
                Pendant.Auto = Pendant.Good;
                Pendant.Manual = Pendant.Disabled;
            }
            else
            {
                Pendant.Auto = Pendant.Disabled;
                Pendant.Manual = Pendant.Good;
            }

            if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["EStop"].Addr))
                Pendant.Estop = Pendant.Bad;
            else
                Pendant.Estop = Pendant.Disabled;

            if (App.Settings.Version > TMflowVersions.V1_80_xxxx)
            {
                if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Get Control"].Addr))
                    Pendant.GetControl = Pendant.Good;
                else
                    Pendant.GetControl = Pendant.Bad;

                if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Active"].Addr))
                    Pendant.AutoActive = Pendant.Good;
                else
                    Pendant.AutoActive = Pendant.Bad;

                if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Enabled"].Addr))
                    Pendant.AutoEnable = Pendant.Good;
                else
                    Pendant.AutoEnable = Pendant.Bad;
            }

            if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Project Running"].Addr))
            {
                Pendant.Play = Pendant.GoodRadial;
                Pendant.Stop = Pendant.Transparent;
            }
            else if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Project Paused"].Addr))
            {
                Pendant.Play = Pendant.MehRadial;
                Pendant.Stop = Pendant.Transparent;
            }
            else if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Project Editing"].Addr))
            {
                Pendant.Play = Pendant.Transparent;
                Pendant.Stop = Pendant.MehRadial;
            }
            else
            {
                Pendant.Play = Pendant.Transparent;
                Pendant.Stop = Pendant.BadRadial;
            }

            if (ModbusTCP.ReadDiscreteInput(ModbusDictionary.ModbusData[App.Settings.Version]["Error"].Addr))
                Pendant.Error = Pendant.Bad;
            else
                Pendant.Error = Pendant.Disabled;


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
                    Pendant.ErrorDate = date.ToString();
            }
            else
                Pendant.ErrorDate = "";

            Pendant.ErrorCode = code.ToString("X");

            if (ErrorCodes.Codes.TryGetValue(code, out string val))
                Pendant.ErrorDescription = val;
            else
                Pendant.ErrorDescription = "CAN NOT FIND ERROR IN TABLE.";
        }


    }
}
