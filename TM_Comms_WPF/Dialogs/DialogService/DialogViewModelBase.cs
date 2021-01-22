using System.Windows;

namespace TM_Comms_WPF.Dialogs.DialogService
{
    public abstract class DialogViewModelBase
    {
        public DialogResultData UserDialogResult { get; private set; }
        private DialogParameters Parameters { get; set; }
        public string Title { get => Parameters.Title; }
        public string Message { get => Parameters.Message; }

        public DialogViewModelBase(DialogParameters parameters)
        {
            this.Parameters = parameters;
        }

        public void CloseDialogWithResult(Window dialog, DialogResultData result)
        {
            this.UserDialogResult = result;
            if (dialog != null)
                dialog.DialogResult = true;
        }
    }
}
