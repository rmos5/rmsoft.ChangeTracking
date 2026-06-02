using System;
using System.Windows.Input;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Represents a command that can notify listeners when its executable state changes.
    /// </summary>
    public interface IContextCommand : ICommand
    {
        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged" />.
        /// </summary>
        void RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Provides a base implementation for commands that operate against a strongly typed context.
    /// </summary>
    /// <typeparam name="TContext">The command context type.</typeparam>
    public abstract class ContextCommandBase<TContext> : IContextCommand
        where TContext : class
    {
        /// <summary>
        /// Gets the object the command operates on.
        /// </summary>
        protected TContext Context { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextCommandBase{TContext}" /> class.
        /// </summary>
        /// <param name="context">The object the command operates on.</param>
        /// <exception cref="ArgumentNullException"><paramref name="context" /> is <see langword="null" />.</exception>
        public ContextCommandBase(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        public abstract void Execute(object? parameter);

        /// <inheritdoc />
        public virtual bool CanExecute(object? parameter)
        {
            return false;
        }

        /// <inheritdoc />
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Toggles tracking for an <see cref="ITrackedObject" />.
    /// </summary>
    public class ToggleTrackingCommandImpl : ContextCommandBase<ITrackedObject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleTrackingCommandImpl" /> class.
        /// </summary>
        /// <param name="context">The tracked object controlled by the command.</param>
        public ToggleTrackingCommandImpl(ITrackedObject context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override bool CanExecute(object? parameter)
        {
            return Context.IsTrackingEnabled;
        }

        /// <inheritdoc />
        public override void Execute(object? parameter)
        {
            if (Context.IsTracking)
                Context.StopTracking(true);
            else
                Context.StartTracking();
        }
    }

    /// <summary>
    /// Provides a base implementation for commands that require active tracking.
    /// </summary>
    public abstract class TrackingCommandBase : ContextCommandBase<ITrackedObject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingCommandBase" /> class.
        /// </summary>
        /// <param name="context">The tracked object controlled by the command.</param>
        protected TrackingCommandBase(ITrackedObject context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override bool CanExecute(object? parameter)
        {
            return Context.IsTracking;
        }
    }

    /// <summary>
    /// Undoes the current change for an <see cref="ITrackedObject" />.
    /// </summary>
    public class UndoChangesCommandImpl : TrackingCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UndoChangesCommandImpl" /> class.
        /// </summary>
        /// <param name="context">The tracked object controlled by the command.</param>
        public UndoChangesCommandImpl(ITrackedObject context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override bool CanExecute(object? parameter)
        {
            return base.CanExecute(parameter)
                && Context.CanUndo();
        }

        /// <inheritdoc />
        public override void Execute(object? parameter)
        {
            Context.Undo();
        }
    }

    /// <summary>
    /// Redoes the next change for an <see cref="ITrackedObject" />.
    /// </summary>
    public class RedoChangesCommandImpl : TrackingCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedoChangesCommandImpl" /> class.
        /// </summary>
        /// <param name="context">The tracked object controlled by the command.</param>
        public RedoChangesCommandImpl(ITrackedObject context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override bool CanExecute(object? parameter)
        {
            return base.CanExecute(parameter)
                && Context.CanRedo();
        }

        /// <inheritdoc />
        public override void Execute(object? parameter)
        {
            Context.Redo();
        }
    }

    /// <summary>
    /// Applies current changes by stopping tracking without cancelling changes.
    /// </summary>
    public class ApplyChangesCommandImpl : TrackingCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyChangesCommandImpl" /> class.
        /// </summary>
        /// <param name="context">The tracked object controlled by the command.</param>
        public ApplyChangesCommandImpl(ITrackedObject context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override bool CanExecute(object? parameter)
        {
            return base.CanExecute(parameter)
                && Context.CanUndo();
        }

        /// <inheritdoc />
        public override void Execute(object? parameter)
        {
            Context.StopTracking(false);
        }
    }
}
