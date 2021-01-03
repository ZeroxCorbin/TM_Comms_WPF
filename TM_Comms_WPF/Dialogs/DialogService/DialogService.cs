using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return result;
        }
    }
}
