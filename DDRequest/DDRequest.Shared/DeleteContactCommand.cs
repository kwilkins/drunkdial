using System;
using System.Collections.Generic;
using System.Text;

namespace DDRequest
{
    using System.Linq;
    using System.Windows.Input;

    using Windows.UI.Popups;

    public class DeleteContactCommand : ICommand
    {
        public void Execute(object parameter)
        {
            ContactGridViewModel.Instance.RemoveContactById(parameter as string);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
