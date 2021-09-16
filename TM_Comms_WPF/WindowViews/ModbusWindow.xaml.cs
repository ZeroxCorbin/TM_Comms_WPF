using System.Windows;
using System.Windows.Data;
using TM_Comms_WPF.WindowViewModels;

namespace TM_Comms_WPF.WindowViews
{
    public partial class ModbusWindow : Window
    {
        public ModbusWindow(Window owner)
        {
            Owner = owner;

            InitializeComponent();

            _ = SetBinding(WidthProperty, new Binding("Width") { Source = DataContext, Mode = BindingMode.TwoWay });
            _ = SetBinding(HeightProperty, new Binding("Height") { Source = DataContext, Mode = BindingMode.TwoWay });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((ModbusViewModel)DataContext).ViewClosing();
        }
    }
}
