using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public partial class TrackedObservableCollection<T> : ObservableCollection<T>, ITrackedCollection<T>
    {
        public event EventHandler TrackerUpdated;

        protected IListChangeTracking<T> ChangeTracker { get; }

        public virtual bool HasChanges => ChangeTracker.HasChanges;

        public virtual bool IsTracking => ChangeTracker.IsTracking;

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

        private T selectedItem;

        public T SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem?.Equals(value) != true)
                {
                    selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    if (selectedItem is TrackedObjectBase tob)
                        tob.RefreshModelState();
                    OnSelectedItemChanged();
                    RefreshModelState();
                }
            }
        }

        public bool HasSelectedItem => SelectedItem != null;

        public IContextCommand ToggleTrackingCommand { get; }

        public IContextCommand UndoChangesCommand { get; }

        public IContextCommand RedoChangesCommand { get; }

        public IContextCommand ApplyChangesCommand { get; }

        public IContextCommand RemoveItemCommand { get; }

        public IContextCommand MoveItemCommand { get; }

        public IContextCommand ReplaceItemCommand { get; }

        public IContextCommand ClearItemsCommand { get; }

        protected TrackedObservableCollection()
        {
            ChangeTracker = new ListChangeTracker<T>(this);
            ChangeTracker.TrackerUpdated += OnTrackerUpdated;

            ToggleTrackingCommand = new ToggleTrackingCommandImpl(this);
            UndoChangesCommand = new UndoChangesCommandImpl(this);
            RedoChangesCommand = new RedoChangesCommandImpl(this);
            ApplyChangesCommand = new ApplyChangesCommandImpl(this);
            RemoveItemCommand = new RemoveItemCommandImpl(this);
            MoveItemCommand = new MoveItemCommandImpl(this);
            ReplaceItemCommand = new ReplaceItemCommandImpl(this);
            ClearItemsCommand = new ClearItemsCommandImpl(this);
        }

        protected TrackedObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        protected TrackedObservableCollection(IList<T> list) : base(list)
        {
        }

        protected virtual void OnTrackerUpdated(object sender, EventArgs e)
        {
            RefreshModelState();
            EventHandler h = TrackerUpdated;
            h?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSelectedItemChanged()
        {
        }

        public virtual void RefreshModelState()
        {
            OnPropertyChanged(nameof(IsTracking));
            OnPropertyChanged(nameof(HasChanges));

            ToggleTrackingCommand.RaiseCanExecuteChanged();
            UndoChangesCommand.RaiseCanExecuteChanged();
            RedoChangesCommand.RaiseCanExecuteChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
            RemoveItemCommand.RaiseCanExecuteChanged();
            MoveItemCommand.RaiseCanExecuteChanged();
            ReplaceItemCommand.RaiseCanExecuteChanged();
            ClearItemsCommand.RaiseCanExecuteChanged();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            SelectedItem = item;
            RefreshModelState();
        }

        protected override void RemoveItem(int index)
        {
            int idx = index;
            base.RemoveItem(index);
            if (Count == 0)
                SelectedItem = default(T);
            else
            {
                idx--;
                SelectedItem = this[idx < 0 ? 0 : idx];
            }

            RefreshModelState();
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            RefreshModelState();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            RefreshModelState();
        }

        public virtual void StartTracking()
        {
            if (!IsTrackingEnabled)
                throw new InvalidOperationException("Object tracking is not enabled.");

            ChangeTracker.StartTracking();
            RefreshModelState();
        }

        public virtual void StopTracking(bool cancelChanges)
        {
            ChangeTracker.StopTracking(cancelChanges);
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
    }
}
