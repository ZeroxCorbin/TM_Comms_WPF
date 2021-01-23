using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM_Comms_WPF.Dialogs.DialogService
{
    public enum DialogResult
    {
        Undefined,
        Yes,
        No,
        Ok,
        Cancel
    }

    public class DialogResultData
    {
        public DialogResult Result {get;set;}
        public string Value { get; set; }
    }

}
