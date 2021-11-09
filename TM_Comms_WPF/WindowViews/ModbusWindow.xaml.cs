using ControlzEx.Theming;
using System.Windows;
using System.Windows.Data;
using TM_Comms_WPF.WindowViewModels;

namespace TM_Comms_WPF.WindowViews
{
    public partial class ModbusWindow : MahApps.Metro.Controls.MetroWindow
    {
        public ModbusWindow(Window owner)
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();

            Owner = owner;

            InitializeComponent();

            _ = SetBinding(WidthProperty, new Binding("Width") { Source = DataContext, Mode = BindingMode.TwoWay });
            _ = SetBinding(HeightProperty, new Binding("Height") { Source = DataContext, Mode = BindingMode.TwoWay });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((ModbusViewModel)DataContext).ViewClosing();
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
