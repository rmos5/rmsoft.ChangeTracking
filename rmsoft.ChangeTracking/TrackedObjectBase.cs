using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public abstract partial class TrackedObjectBase : INotifyPropertyChanged, ITrackedObject, IPropertyChangesTracking
    {
        private bool disposed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? TrackerUpdated;

        protected IPropertyChangesTracking ChangeTracker { get; }

        public INotifyPropertyChanged Item => ChangeTracker.Item;

        public IEnumerable<PropertyChanges> Changes => ChangeTracker.Changes;

        public int ChangesCount => ChangeTracker.ChangesCount;

        public virtual bool HasChanges => ChangeTracker.HasChanges;

        public PropertyChanges? CurrentChange => ChangeTracker.CurrentChange;

        public PropertyChanges? NextChange => ChangeTracker.NextChange;

        public PropertyChanges? PreviousChange => ChangeTracker.PreviousChange;

        private bool isTrackingEnabled;

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

        public virtual bool IsTracking => ChangeTracker.IsTracking;

        public IContextCommand ToggleTrackingCommand { get; }

        public IContextCommand UndoChangesCommand { get; }

        public IContextCommand RedoChangesCommand { get; }

        public IContextCommand ApplyChangesCommand { get; }

        public TrackedObjectBase()
        {
            ChangeTracker = new PropertyChangesTracker(this);
            ChangeTracker.TrackerUpdated += OnTrackerUpdated;

            ToggleTrackingCommand = new ToggleTrackingCommandImpl(this);
            UndoChangesCommand = new UndoChangesCommandImpl(this);
            RedoChangesCommand = new RedoChangesCommandImpl(this);
            ApplyChangesCommand = new ApplyChangesCommandImpl(this);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnTrackerUpdated(object? sender, EventArgs e)
        {
            RefreshModelState();
            TrackerUpdated?.Invoke(this, e);
        }

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

        public virtual void StartTracking()
        {
            ThrowIfDisposed();

            if (!IsTrackingEnabled)
                throw new InvalidOperationException("Object tracking is not enabled.");

            ChangeTracker.StartTracking();
            RefreshModelState();
        }

        public virtual void StopTracking(bool cancelChanges)
        {
            StopTracking(cancelChanges, true);
        }

        public virtual void StopTracking(bool cancelChanges, bool clearHistory)
        {
            ThrowIfDisposed();

            ChangeTracker.StopTracking(cancelChanges, clearHistory);
            RefreshModelState();
        }

        public virtual bool CanUndo()
        {
            return ChangeTracker.CanUndo();
        }

        public virtual void Undo()
        {
            ChangeTracker.Undo();
            RefreshModelState();
        }

        public virtual bool CanRedo()
        {
            return ChangeTracker.CanRedo();
        }

        public virtual void Redo()
        {
            ChangeTracker.Redo();
            RefreshModelState();
        }

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
