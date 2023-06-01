using System;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public partial class TrackedObjectBase : PropertyChangedBase, IChangeTracker
    {
        public event EventHandler TrackerUpdated;
        
        protected IPropertyChangeTracker ChangeTracker { get; }

        public virtual bool IsTracking => ChangeTracker.IsTracking;

        public virtual bool HasChanges => ChangeTracker.HasChanges;

        public IContextCommand ToggleEditingCommand { get; }

        public IContextCommand UndoChangesCommand { get; }

        public IContextCommand RedoChangesCommand { get; }

        public IContextCommand ApplyChangesCommand { get; }

        public TrackedObjectBase()
        {
            ChangeTracker = new PropertyChangeTracker(this);
            ChangeTracker.TrackerUpdated += ChangeTracker_TrackerUpdated;

            ToggleEditingCommand = new ToggleEditingCommandImpl(this);
            UndoChangesCommand = new UndoChangesCommandImpl(this);
            RedoChangesCommand = new RedoChangesCommandImpl(this);
            ApplyChangesCommand = new ApplyChangesCommandImpl(this);
        }

        private void ChangeTracker_TrackerUpdated(object sender, EventArgs e)
        {
            RefreshModelState();
            EventHandler h = TrackerUpdated;
            h?.Invoke(this, e);
        }
        
        public virtual void RefreshModelState()
        {
            OnPropertyChanged(nameof(IsTracking));
            OnPropertyChanged(nameof(HasChanges));

            ToggleEditingCommand.RaiseCanExecuteChanged();
            UndoChangesCommand.RaiseCanExecuteChanged();
            RedoChangesCommand.RaiseCanExecuteChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }

        public void StartTracking()
        {
            ChangeTracker.StartTracking();
            RefreshModelState();
        }

        public void StopTracking(bool cancelChanges)
        {
            ChangeTracker.StopTracking(cancelChanges);
            RefreshModelState();
        }

        public bool CanUndo()
        {
            return ChangeTracker.CanUndo();
        }

        public void Undo()
        {
            ChangeTracker.Undo();
        }

        public bool CanRedo()
        {
            return ChangeTracker.CanRedo();
        }

        public void Redo()
        {
            ChangeTracker.Redo();
        }
    }
}
