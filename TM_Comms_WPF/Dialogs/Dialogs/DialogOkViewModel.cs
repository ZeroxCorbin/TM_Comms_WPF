using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TM_Comms_WPF.Commands;
using TM_Comms_WPF.Dialogs.DialogService;

namespace TM_Comms_WPF.Dialogs
{
    class DialogOkViewModel : DialogViewModelBase
    {
        public ICommand OkCommand { get; }

        public DialogOkViewModel(DialogParameters parameters)
            : base(parameters)
        {
            this.OkCommand = new RelayCommand(OnOkClicked);
        }

        private void OnOkClicked(object parameter)
        {
            this.CloseDialogWithResult(parameter as Window, new DialogResultData() { Result = DialogResult.Ok, Value = string.Empty });
        }
    }
}
