using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TM_Comms_WPF.WindowViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public string Title => "TM Comms";
        public double Left { get => App.Settings.MainWindow.Left; set { App.Settings.MainWindow.Left = value; OnPropertyChanged(); } }
        public double Top { get => App.Settings.MainWindow.Top; set { App.Settings.MainWindow.Top = value; OnPropertyChanged(); } }
        public WindowState WindowState { get => App.Settings.MainWindow.WindowState; set { App.Settings.MainWindow.WindowState = value; OnPropertyChanged(); } }

        public string RobotIPAddress
        {
            get { return App.Settings.RobotIP; }
            set { App.Settings.RobotIP = value; }
        }

        public TM_Comms.TMflowVersions SelectedVersion
        {
            get { return App.Settings.Version; }
            set { App.Settings.Version = value; }
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
