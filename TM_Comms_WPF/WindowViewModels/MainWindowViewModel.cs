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
        //public double Width { get => App.Settings.ListenNodeWindow.Width; set { App.Settings.ListenNodeWindow.Width = value; OnPropertyChanged(); } }
        //public double Height { get => App.Settings.ListenNodeWindow.Height; set { App.Settings.ListenNodeWindow.Height = value; OnPropertyChanged(); } }
        //public WindowState WindowState { get => App.Settings.ListenNodeWindow.WindowState; set { App.Settings.ListenNodeWindow.WindowState = value; OnPropertyChanged(); } }


    }
}
