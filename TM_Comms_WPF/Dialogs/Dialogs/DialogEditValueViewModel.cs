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
    class DialogEditValueViewModel : DialogViewModelBase
    {
        public string Value { get; set; }

        private ICommand okCommand = null;
        public ICommand OkCommand
        {
            get { return okCommand; }
            set { okCommand = value; }
        }

        private ICommand cancelCommand = null;
        public ICommand CancelCommand
        {
            get { return cancelCommand; }
            set { cancelCommand = value; }
        }

        public DialogEditValueViewModel(DialogParameters parameters)
            : base(parameters)
        {
            Value = parameters.Value;
            
            this.okCommand = new RelayCommand(OnOkClicked);
            this.cancelCommand = new RelayCommand(OnCancelClicked);
        }

        private void OnOkClicked(object parameter)
        {
            this.CloseDialogWithResult(parameter as Window, new DialogResultData() { Result = DialogResult.Ok, Value = Value });
        }

        private void OnCancelClicked(object parameter)
        {
            this.CloseDialogWithResult(parameter as Window, new DialogResultData() { Result = DialogResult.Cancel, Value = string.Empty });
        }
    }
}
