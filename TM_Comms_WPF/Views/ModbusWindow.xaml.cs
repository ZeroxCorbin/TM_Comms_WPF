using ApplicationSettingsNS;
using SimpleModbus;
using SocketManagerNS;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TM_Comms;

namespace TM_Comms_WPF
{
    /// <summary>
    /// Interaction logic for ModbusWindow.xaml
    /// </summary>
    public partial class ModbusWindow : Window
    {
        public ModbusWindow(Window owner)
        {
            Owner = owner;

            InitializeComponent();

            SetBinding(Window.WidthProperty, new Binding("Width") { Source = this.DataContext, Mode=BindingMode.TwoWay });
            SetBinding(Window.HeightProperty, new Binding("Height") { Source = this.DataContext, Mode=BindingMode.TwoWay });
        }

        //public string ShowEditDialog()
        //{
        //    var obj = Dispatcher.BeginInvoke(DispatcherPriority.Render, new Func<string>(EditValue));
        //    obj.Wait();
        //    return (string)obj.Result;
        //}
        //private string EditValue()
        //{
        //    return "";
        //}
    }
}
