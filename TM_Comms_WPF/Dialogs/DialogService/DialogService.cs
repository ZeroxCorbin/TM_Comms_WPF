using System.Windows;

namespace TM_Comms_WPF.Dialogs.DialogService
{
    public class DialogService
    {
        public static DialogResultData OpenDialog(DialogViewModelBase vm, Window owner)
        {
            DialogWindow win = new DialogWindow();
            if (owner != null)
                win.Owner = owner;
            win.DataContext = vm;
            win.ShowDialog();
            DialogResultData result =
                (win.DataContext as DialogViewModelBase).UserDialogResult;

            if (result == null)
                result = new DialogResultData() { Result = DialogResult.Undefined, Value = "" };

            return result;
        }

        public static DialogResultData YesNoDialog(DialogParameters parameters, Window owner)
        {
            DialogWindow win = new DialogWindow();
            if (owner != null)
                win.Owner = owner;
            win.DataContext = new DialogYesNoViewModel(parameters);
            win.ShowDialog();
            DialogResultData result =
                (win.DataContext as DialogViewModelBase).UserDialogResult;

            if (result == null)
                result = new DialogResultData() { Result = DialogResult.Undefined, Value = "" };

            return result;
        }

        public static DialogResultData EditValueDialog(DialogParameters parameters, Window owner)
        {
            DialogWindow win = new DialogWindow();
            if (owner != null)
                win.Owner = owner;
            win.DataContext = new DialogEditValueViewModel(parameters);
            win.ShowDialog();
            DialogResultData result =
                (win.DataContext as DialogViewModelBase).UserDialogResult;

            if (result == null)
                result = new DialogResultData() { Result = DialogResult.Undefined, Value = "" };

            return result;
        }


        public static DialogResultData OkDialog(DialogParameters parameters, Window owner)
        {
            DialogWindow win = new DialogWindow();
            if (owner != null)
                win.Owner = owner;
            win.DataContext = new DialogOkViewModel(parameters);
            win.ShowDialog();
            DialogResultData result =
                (win.DataContext as DialogViewModelBase).UserDialogResult;

            if (result == null)
                result = new DialogResultData() { Result = DialogResult.Undefined, Value = "" };

            return result;
        }
    }
}
