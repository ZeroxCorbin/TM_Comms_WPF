using System.Windows;
using System.Windows.Data;


namespace TM_Comms_WPF.Views
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
    }
}
