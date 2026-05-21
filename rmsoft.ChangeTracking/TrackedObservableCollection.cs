using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public partial class TrackedObservableCollection<T> : ObservableCollection<T>, ITrackedCollection<T>, IListChangeTracking<T>
    {
        private bool disposed;
        private T? selectedItem;

        public event EventHandler? TrackerUpdated;

        protected IListChangeTracking<T> ChangeTracker { get; }

        public INotifyListChanged<T> Item => ChangeTracker.Item;

        public IEnumerable<NotifyCollectionChangedEventArgs> Changes => ChangeTracker.Changes;

        public int ChangesCount => ChangeTracker.ChangesCount;

        public virtual bool HasChanges => ChangeTracker.HasChanges;

        public NotifyCollectionChangedEventArgs? CurrentChange => ChangeTracker.CurrentChange;

        public NotifyCollectionChangedEventArgs? NextChange => ChangeTracker.NextChange;

        public NotifyCollectionChangedEventArgs? PreviousChange => ChangeTracker.PreviousChange;

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

        public T? SelectedItem
        {
            get => selectedItem;
            set
            {
                if (!EqualityComparer<T?>.Default.Equals(selectedItem, value))
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

        protected bool SuppressCollectionChangeEvent { get; set; }

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

        protected TrackedObservableCollection(IEnumerable<T> collection) : this()
        {
            foreach (T item in collection)
                Add(item);
        }

        protected TrackedObservableCollection(IList<T> list) : this((IEnumerable<T>)list)
        {
        }

        protected virtual void OnTrackerUpdated(object? sender, EventArgs e)
        {
            RefreshModelState();
            TrackerUpdated?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!SuppressCollectionChangeEvent)
                base.OnCollectionChanged(e);
        }

        protected virtual void OnSelectedItemChanged()
        {
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
            RemoveItemCommand.RaiseCanExecuteChanged();
            MoveItemCommand.RaiseCanExecuteChanged();
            ReplaceItemCommand.RaiseCanExecuteChanged();
            ClearItemsCommand.RaiseCanExecuteChanged();
        }

        public void Add(T item, bool select)
        {
            Add(item);
            if (select)
                SelectedItem = item;
        }

        public void Insert(int index, T item, bool select)
        {
            InsertItem(index, item);
            if (select)
                SelectedItem = item;
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            RefreshModelState();
        }

        public void Remove(T item, bool select)
        {
            int idx = IndexOf(item);
            Remove(item);

            if (select)
            {
                if (Count == 0)
                    SelectedItem = default;
                else
                {
                    idx--;
                    SelectedItem = this[idx < 0 ? 0 : idx];
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            RefreshModelState();
        }

        public void Move(int oldIndex, int newIndex, bool select)
        {
            T item = this[oldIndex];
            Move(oldIndex, newIndex);
            if (select)
                SelectedItem = item;
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            RefreshModelState();
        }

        public void ReplaceAt(int index, T item, bool select)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            base.SetItem(index, item);

            if (select)
                SelectedItem = item;

            RefreshModelState();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            SelectedItem = default;
            RefreshModelState();
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
