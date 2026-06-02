using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Provides a base implementation for bindable objects with property change tracking and undo/redo support.
    /// </summary>
    public abstract partial class TrackedObjectBase : INotifyPropertyChanged, ITrackedObject, IPropertyChangesTracking
    {
        private bool disposed;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc />
        public event EventHandler? TrackerUpdated;

        /// <summary>
        /// Gets the tracker that records property changes for this object.
        /// </summary>
        protected IPropertyChangesTracking ChangeTracker { get; }

        /// <inheritdoc />
        public INotifyPropertyChanged Item => ChangeTracker.Item;

        /// <inheritdoc />
        public IEnumerable<PropertyChanges> Changes => ChangeTracker.Changes;

        /// <inheritdoc />
        public int ChangesCount => ChangeTracker.ChangesCount;

        /// <inheritdoc />
        public virtual bool HasChanges => ChangeTracker.HasChanges;

        /// <inheritdoc />
        public PropertyChanges? CurrentChange => ChangeTracker.CurrentChange;

        /// <inheritdoc />
        public PropertyChanges? NextChange => ChangeTracker.NextChange;

        /// <inheritdoc />
        public PropertyChanges? PreviousChange => ChangeTracker.PreviousChange;

        private bool isTrackingEnabled;

        /// <inheritdoc />
        public bool IsTrackingEnabled
        {
            get => isTrackingEnabled;
            set
            {
                if (isTrackingEnabled != value)
                {
                    isTrackingEnabled = value;
                    OnPropertyChanged(nameof(IsTrackingEnabled));
                    RefreshModelState();
                }
            }
        }

        /// <inheritdoc />
        public virtual bool IsTracking => ChangeTracker.IsTracking;

        /// <summary>
        /// Gets a command that starts tracking when tracking is inactive or cancels changes when tracking is active.
        /// </summary>
        public IContextCommand ToggleTrackingCommand { get; }

        /// <summary>
        /// Gets a command that undoes the current property change.
        /// </summary>
        public IContextCommand UndoChangesCommand { get; }

        /// <summary>
        /// Gets a command that redoes the next property change.
        /// </summary>
        public IContextCommand RedoChangesCommand { get; }

        /// <summary>
        /// Gets a command that applies current changes by stopping tracking without cancelling changes.
        /// </summary>
        public IContextCommand ApplyChangesCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedObjectBase" /> class.
        /// </summary>
        public TrackedObjectBase()
        {
            ChangeTracker = new PropertyChangesTracker(this);
            ChangeTracker.TrackerUpdated += OnTrackerUpdated;

            ToggleTrackingCommand = new ToggleTrackingCommandImpl(this);
            UndoChangesCommand = new UndoChangesCommandImpl(this);
            RedoChangesCommand = new RedoChangesCommandImpl(this);
            ApplyChangesCommand = new ApplyChangesCommandImpl(this);
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged" /> for the specified property.
        /// </summary>
        /// <param name="propertyName">The changed property name.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handles tracker updates and forwards the update to this object's state.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event data.</param>
        protected virtual void OnTrackerUpdated(object? sender, EventArgs e)
        {
            RefreshModelState();
            TrackerUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Raises property and command state notifications for tracking-related state.
        /// </summary>
        public virtual void RefreshModelState()
        {
            OnPropertyChanged(nameof(IsTracking));
            OnPropertyChanged(nameof(HasChanges));
            OnPropertyChanged(nameof(CurrentChange));
            OnPropertyChanged(nameof(Changes));

            ToggleTrackingCommand.RaiseCanExecuteChanged();
            UndoChangesCommand.RaiseCanExecuteChanged();
            RedoChangesCommand.RaiseCanExecuteChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }

        /// <inheritdoc />
        public virtual void StartTracking()
        {
            ThrowIfDisposed();

            if (!IsTrackingEnabled)
                throw new InvalidOperationException("Object tracking is not enabled.");

            ChangeTracker.StartTracking();
            RefreshModelState();
        }

        /// <inheritdoc />
        public virtual void StopTracking(bool cancelChanges)
        {
            StopTracking(cancelChanges, true);
        }

        /// <inheritdoc />
        public virtual void StopTracking(bool cancelChanges, bool clearHistory)
        {
            ThrowIfDisposed();

            ChangeTracker.StopTracking(cancelChanges, clearHistory);
            RefreshModelState();
        }

        /// <inheritdoc />
        public virtual bool CanUndo()
        {
            return ChangeTracker.CanUndo();
        }

        /// <inheritdoc />
        public virtual void Undo()
        {
            ChangeTracker.Undo();
            RefreshModelState();
        }

        /// <inheritdoc />
        public virtual bool CanRedo()
        {
            return ChangeTracker.CanRedo();
        }

        /// <inheritdoc />
        public virtual void Redo()
        {
            ChangeTracker.Redo();
            RefreshModelState();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (disposed)
                return;

            ChangeTracker.TrackerUpdated -= OnTrackerUpdated;
            ChangeTracker.Dispose();
            disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
