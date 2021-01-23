using System.ComponentModel;
using System.Windows.Controls;
using static TM_Comms.ExternalDetection;

namespace TM_Comms_WPF
{
    public partial class ExternalVisionResultControl : UserControl
    {

        public Annotation Annotation { get; set; }


        public ExternalVisionResultControl(Annotation annotation)
        {
            Annotation = annotation;
            DataContext = Annotation;

            InitializeComponent();

        }
    }
}
