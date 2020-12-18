using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TM_Comms_WPF
{
    /// <summary>
    /// Interaction logic for Pendant.xaml
    /// </summary>
    public partial class Pendant : Window
    {
        public interface IPendantData
        {
            bool PowerInd { get; set; }
            bool AutoInd { get; set; }
            bool ManualInd { get; set; }
            bool EStopInd { get; set; }

            bool StopButton { get; set; }
            bool PlayPauseButton { get; set; }
            bool MinusButton { get; set; }
            bool PlusButton { get; set; }

        }


        public Pendant()
        {
            
            InitializeComponent();
        }
    }
}
