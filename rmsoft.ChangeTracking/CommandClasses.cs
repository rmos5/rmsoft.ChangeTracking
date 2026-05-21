using System;
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

    public class ToggleTrackingCommandImpl : ContextCommandBase<ITrackedObject>
    {
        public ToggleTrackingCommandImpl(ITrackedObject context)
            : base(context)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return Context.IsTrackingEnabled;
        }

        public override void Execute(object parameter)
        {
            if (Context.IsTracking)
                Context.StopTracking(true);
            else
                Context.StartTracking();
        }
    }

    public abstract class TrackingCommandBase : ContextCommandBase<ITrackedObject>
    {
        protected TrackingCommandBase(ITrackedObject context)
            : base(context)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return Context.IsTracking;
        }
    }

    public class UndoChangesCommandImpl : TrackingCommandBase
    {
        public UndoChangesCommandImpl(ITrackedObject context)
            : base(context)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter)
                && Context.CanUndo();
        }

        public override void Execute(object parameter)
        {
            Context.Undo();
        }
    }

    public class RedoChangesCommandImpl : TrackingCommandBase
    {
        public RedoChangesCommandImpl(ITrackedObject context)
            : base(context)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter)
                && Context.CanRedo();
        }

        public override void Execute(object parameter)
        {
            Context.Redo();
        }
    }

    public class ApplyChangesCommandImpl : TrackingCommandBase
    {
        public ApplyChangesCommandImpl(ITrackedObject context)
            : base(context)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter)
                && Context.CanUndo();
        }

        public override void Execute(object parameter)
        {
            Context.StopTracking(false);
        }
    }
}
