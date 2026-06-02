using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Provides an observable collection with selection, change tracking, and undo/redo support.
    /// </summary>
    /// <typeparam name="T">The collection item type.</typeparam>
    public partial class TrackedObservableCollection<T> : ObservableCollection<T>, ITrackedCollection<T>, IListChangeTracking<T>
    {
        private bool disposed;
        private T? selectedItem;

        /// <inheritdoc />
        public event EventHandler? TrackerUpdated;

        /// <summary>
        /// Gets the tracker that records collection changes for this collection.
        /// </summary>
        protected IListChangeTracking<T> ChangeTracker { get; }

        /// <inheritdoc />
        public INotifyListChanged<T> Item => ChangeTracker.Item;

        /// <inheritdoc />
        public IEnumerable<NotifyCollectionChangedEventArgs> Changes => ChangeTracker.Changes;

        /// <inheritdoc />
        public int ChangesCount => ChangeTracker.ChangesCount;

        /// <inheritdoc />
        public virtual bool HasChanges => ChangeTracker.HasChanges;

        /// <inheritdoc />
        public NotifyCollectionChangedEventArgs? CurrentChange => ChangeTracker.CurrentChange;

        /// <inheritdoc />
        public NotifyCollectionChangedEventArgs? NextChange => ChangeTracker.NextChange;

        /// <inheritdoc />
        public NotifyCollectionChangedEventArgs? PreviousChange => ChangeTracker.PreviousChange;

        /// <inheritdoc />
        public virtual bool IsTracking => ChangeTracker.IsTracking;

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

        /// <inheritdoc />
        public bool HasSelectedItem => SelectedItem != null;

        /// <summary>
        /// Gets or sets a value indicating whether collection change notifications should be suppressed.
        /// </summary>
        protected bool SuppressCollectionChangeEvent { get; set; }

        /// <summary>
        /// Gets a command that starts tracking when tracking is inactive or cancels changes when tracking is active.
        /// </summary>
        public IContextCommand ToggleTrackingCommand { get; }

        /// <summary>
        /// Gets a command that undoes the current collection change.
        /// </summary>
        public IContextCommand UndoChangesCommand { get; }

        /// <summary>
        /// Gets a command that redoes the next collection change.
        /// </summary>
        public IContextCommand RedoChangesCommand { get; }

        /// <summary>
        /// Gets a command that applies current changes by stopping tracking without cancelling changes.
        /// </summary>
        public IContextCommand ApplyChangesCommand { get; }

        /// <summary>
        /// Gets a command that removes the selected item.
        /// </summary>
        public IContextCommand RemoveItemCommand { get; }

        /// <summary>
        /// Gets a command that moves the selected item to an index supplied as the command parameter.
        /// </summary>
        public IContextCommand MoveItemCommand { get; }

        /// <summary>
        /// Gets a command that replaces an item at an index supplied as the command parameter with the selected item.
        /// </summary>
        public IContextCommand ReplaceItemCommand { get; }

        /// <summary>
        /// Gets a command that clears the collection.
        /// </summary>
        public IContextCommand ClearItemsCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedObservableCollection{T}" /> class.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedObservableCollection{T}" /> class that contains copied items.
        /// </summary>
        /// <param name="collection">The items to copy into the collection.</param>
        protected TrackedObservableCollection(IEnumerable<T> collection) : this()
        {
            foreach (T item in collection)
                Add(item);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedObservableCollection{T}" /> class that contains copied items.
        /// </summary>
        /// <param name="list">The items to copy into the collection.</param>
        protected TrackedObservableCollection(IList<T> list) : this((IEnumerable<T>)list)
        {
        }

        /// <summary>
        /// Handles tracker updates and forwards the update to this collection's state.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event data.</param>
        protected virtual void OnTrackerUpdated(object? sender, EventArgs e)
        {
            RefreshModelState();
            TrackerUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Raises a property change notification for the specified property.
        /// </summary>
        /// <param name="propertyName">The changed property name.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <inheritdoc />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!SuppressCollectionChangeEvent)
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Called after <see cref="SelectedItem" /> changes.
        /// </summary>
        protected virtual void OnSelectedItemChanged()
        {
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
            RemoveItemCommand.RaiseCanExecuteChanged();
            MoveItemCommand.RaiseCanExecuteChanged();
            ReplaceItemCommand.RaiseCanExecuteChanged();
            ClearItemsCommand.RaiseCanExecuteChanged();
        }

        /// <inheritdoc />
        public void Add(T item, bool select)
        {
            Add(item);
            if (select)
                SelectedItem = item;
        }

        /// <inheritdoc />
        public void Insert(int index, T item, bool select)
        {
            InsertItem(index, item);
            if (select)
                SelectedItem = item;
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            RefreshModelState();
        }

        /// <inheritdoc />
        public void Remove(T item, bool select)
        {
            int idx = IndexOf(item);
            if (idx < 0)
                return;

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

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            RefreshModelState();
        }

        /// <inheritdoc />
        public void Move(int oldIndex, int newIndex, bool select)
        {
            T item = this[oldIndex];
            Move(oldIndex, newIndex);
            if (select)
                SelectedItem = item;
        }

        /// <inheritdoc />
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            RefreshModelState();
        }

        /// <inheritdoc />
        public void ReplaceAt(int index, T item, bool select)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            base.SetItem(index, item);

            if (select)
                SelectedItem = item;

            RefreshModelState();
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            base.ClearItems();
            SelectedItem = default;
            RefreshModelState();
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
