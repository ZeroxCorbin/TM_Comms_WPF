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
    class DialogYesNoViewModel : DialogViewModelBase
    {
        private ICommand yesCommand = null;
        public ICommand YesCommand
        {
            get { return yesCommand; }
            set { yesCommand = value; }
        }

        private ICommand noCommand = null;
        public ICommand NoCommand
        {
            get { return noCommand; }
            set { noCommand = value; }
        }

        public DialogYesNoViewModel(DialogParameters parameters)
            : base(parameters)
        {
            this.yesCommand = new RelayCommand(OnYesClicked);
            this.noCommand = new RelayCommand(OnNoClicked);
        }

        private void OnYesClicked(object parameter)
        {
            this.CloseDialogWithResult(parameter as Window, new DialogResultData() { Result = DialogResult.Yes, Value = string.Empty });
        }

        private void OnNoClicked(object parameter)
        {
            this.CloseDialogWithResult(parameter as Window, new DialogResultData() { Result = DialogResult.No, Value = string.Empty });
        }
    }
}
