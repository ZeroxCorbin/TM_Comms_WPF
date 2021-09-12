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

            SetBinding(Window.WidthProperty, new Binding("Width") { Source = this.DataContext, Mode = BindingMode.TwoWay });
            SetBinding(Window.HeightProperty, new Binding("Height") { Source = this.DataContext, Mode = BindingMode.TwoWay });
        }
    }
}
