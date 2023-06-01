using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace rmsoft.ChangeTracking
{
    public interface IContextCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }

    public abstract class ContextCommandBase<TContext> : IContextCommand
        where TContext : class
    {
        protected TContext Context { get; }

        public ContextCommandBase(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public event EventHandler CanExecuteChanged;

        public abstract void Execute(object parameter);

        public virtual bool CanExecute(object parameter)
        {
            return false;
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler h = CanExecuteChanged;
            h?.Invoke(this, EventArgs.Empty);
        }
    }
}
