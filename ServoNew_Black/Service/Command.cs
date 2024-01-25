using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServoNew_Black.Service
{
    public delegate bool CanExecute();
    public class Command : System.Windows.Input.ICommand
    {
        private Action<object> _action;
        public Command(Action<object> WillAction) : this(WillAction, null)
        {

        }
        public Command(Action<object> WillAction, CanExecute CanExecute)
        {
            _action = WillAction;
            CanExecuteAble = CanExecute;
        }
        public CanExecute CanExecuteAble { get; set; } = null;
        public bool CanExecuteWhileNoCanExecute { get; set; } = true;

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067
        public bool CanExecute(object parameter)
        {
            if (CanExecuteAble == null) return CanExecuteWhileNoCanExecute;
            else
            {
                return CanExecuteAble.Invoke();
            }
        }
        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                _action(parameter);
            }
            else
            {
                _action(null);
            }
        }
    }
}
