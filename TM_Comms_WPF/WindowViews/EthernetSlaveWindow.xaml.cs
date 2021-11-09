using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using TM_Comms_WPF.WindowViewModels;
using ControlzEx.Theming;

namespace TM_Comms_WPF.WindowViews
{
    public partial class EthernetSlaveWindow : MahApps.Metro.Controls.MetroWindow
    {
        public EthernetSlaveWindow(Window owner)
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((EthernetSlaveViewModel)DataContext).ViewClosing();
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            if (!CheckOnScreen.IsOnScreen(this))
            {
                this.Left = Owner.Left;
                this.Top = Owner.Top + Owner.ActualHeight;
            }
        }
    }
}
