using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;

namespace TM_Comms_WPF.Views
{
    public partial class EthernetSlaveWindow : Window
    {
        public EthernetSlaveWindow(Window owner)
        {

            Owner = owner;

            InitializeComponent();

            _ = SetBinding(WidthProperty, new Binding("Width") { Source = DataContext, Mode = BindingMode.TwoWay });
            _ = SetBinding(HeightProperty, new Binding("Height") { Source = DataContext, Mode = BindingMode.TwoWay });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (IsFocused)
            {
                _ = MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
