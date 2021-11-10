using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TM_Comms;
using TM_Comms_WPF.Core;

namespace TM_Comms_WPF.WindowViewModels
{
    public class MainWindowViewModel : Core.BaseViewModel
    {

        public string Title => "TM Comms";
        public double Left { get => App.Settings.MainWindow.Left; set { App.Settings.MainWindow.Left = value; OnPropertyChanged(); } }
        public double Top { get => App.Settings.MainWindow.Top; set { App.Settings.MainWindow.Top = value; OnPropertyChanged(); } }
        public WindowState WindowState { get => App.Settings.MainWindow.WindowState; set { App.Settings.MainWindow.WindowState = value; OnPropertyChanged(); } }

        public bool HasPort8080 { get => _HasPort8080; private set => SetProperty(ref _HasPort8080, value); }
        private bool _HasPort8080;
        public bool HasEthernetSlave { get => _HasEthernetSlave; private set => SetProperty(ref _HasEthernetSlave, value); }
        private bool _HasEthernetSlave;
        public bool HasExternalVision { get => _HasExternalVision; private set => SetProperty(ref _HasExternalVision, value); }
        private bool _HasExternalVision;

        public EthernetSlaveViewModel EthernetSlave { get; set; } = new EthernetSlaveViewModel();
        public ModbusViewModel Modbus { get; set; }
        public ListenNodeViewModel ListenNode { get; set; } = new ListenNodeViewModel();

        public int SelectedTabIndex 
        { 
            get => _SelectedTabIndex;
            set 
            { 
                SetProperty(ref _SelectedTabIndex, value);
            }
        }
        private int _SelectedTabIndex;

        public MainWindowViewModel()
        {
            Modbus = new ModbusViewModel(MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance);

            ClosingCommand = new RelayCommand(ClosingCallback, c => true);

            VersionChange();
        }

        public ICommand ClosingCommand { get; }
        private void ClosingCallback(object parameter)
        {
            Modbus.ViewClosing();
            EthernetSlave.ViewClosing();
            ListenNode.ViewClosing();
        }
        public string RobotIPAddress
        {
            get { return App.Settings.RobotIP; }
            set { App.Settings.RobotIP = value; }
        }

        public TM_Comms.TMflowVersions SelectedVersion
        {
            get { return App.Settings.Version;}
            set { App.Settings.Version = value; VersionChange(); }
        }
        private void VersionChange()
        {
            switch (SelectedVersion)
            {
                case TMflowVersions.V1_68_xxxx:
                    HasPort8080 = true;
                    HasEthernetSlave = false;
                    HasExternalVision = false;
                    break;
                case TMflowVersions.V1_72_xxxx:
                    HasPort8080 = true;
                    HasEthernetSlave = false;
                    HasExternalVision = false;
                    break;
                case TMflowVersions.V1_76_xxxx:
                    HasPort8080 = true;
                    HasEthernetSlave = true;
                    HasExternalVision = false;
                    break;
                case TMflowVersions.V1_80_xxxx:
                    HasPort8080 = false;
                    HasEthernetSlave = true;
                    HasExternalVision = true;
                    break;
                case TMflowVersions.V1_82_xxxx:
                    HasPort8080 = false;
                    HasEthernetSlave = true;
                    HasExternalVision = true;
                    break;
                case TMflowVersions.V1_84_xxxx:
                    HasPort8080 = false;
                    HasEthernetSlave = true;
                    HasExternalVision = true;
                    break;
            }

            EthernetSlave.Reload();
            Modbus.Reload();
            ListenNode.Reload();
        }

        public IEnumerable<TM_Comms.TMflowVersions> Versions
        {
            get
            {
                return Enum.GetValues(typeof(TM_Comms.TMflowVersions))
                    .Cast<TM_Comms.TMflowVersions>();
            }
        }

    }
}
