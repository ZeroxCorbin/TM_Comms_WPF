using System.Windows.Controls;
using TM_Comms_WPF.ControlViewModels;

namespace TM_Comms_WPF.ControlViews
{
    /// <summary>
    /// Interaction logic for PositionControl.xaml
    /// </summary>
    public partial class PositionControl : UserControl
    {
        public PositionControl()
        {
            InitializeComponent();
        }

        private void ImgDragDrop_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((PositionControlViewModel)DataContext).DragDropStart(Parent);
        }

        private void UserControl_Drop(object sender, System.Windows.DragEventArgs e)
        {
            ((PositionControlViewModel)DataContext).DragDropEnd(Parent);
        }
    }
}
