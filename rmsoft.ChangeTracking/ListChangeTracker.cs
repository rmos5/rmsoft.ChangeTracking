using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Tracks collection changes on an <see cref="INotifyListChanged{T}" /> collection.
    /// </summary>
    /// <typeparam name="T">The collection item type.</typeparam>
    public interface IListChangeTracking<T> : IChangeTracking<INotifyListChanged<T>, NotifyCollectionChangedEventArgs>
    {
    }

    /// <summary>
    /// Tracks add, remove, move, replace, and reset changes for an <see cref="INotifyListChanged{T}" /> collection.
    /// </summary>
    /// <typeparam name="T">The collection item type.</typeparam>
    public class ListChangeTracker<T> : ChangeTrackerBase<INotifyListChanged<T>, NotifyCollectionChangedEventArgs>, IListChangeTracking<T>
    {
        private List<T>? originalList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListChangeTracker{T}" /> class.
        /// </summary>
        /// <param name="item">The collection whose changes should be tracked.</param>
        public ListChangeTracker(INotifyListChanged<T> item)
            : base(item)
        {
        }

        private void Item_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                StopTracking(true);
                return;
            }

            AddChange(e);
            if (originalList != null && originalList.SequenceEqual(Item))
                ClearChanges();
        }

        /// <summary>
        /// Subscribes to collection change notifications on <see cref="ChangeTrackerBase{TSource, TChange}.Item" />.
        /// </summary>
        protected void AddItemEvents()
        {
            Item.CollectionChanged += Item_CollectionChanged;
        }

        /// <summary>
        /// Unsubscribes from collection change notifications on <see cref="ChangeTrackerBase{TSource, TChange}.Item" />.
        /// </summary>
        protected void RemoveItemEvents()
        {
            Item.CollectionChanged -= Item_CollectionChanged;
        }

        /// <inheritdoc />
        protected override void StartTrackingOverride()
        {
            originalList = new List<T>(Item);
            AddItemEvents();
        }

        /// <inheritdoc />
        protected override void StopTrackingOverride(bool cancelChanges)
        {
            RemoveItemEvents();
        }

        /// <inheritdoc />
        protected override void SetOriginalValues(bool clearAfterSet)
        {
            if (originalList == null)
                return;

            if (IsTracking)
                RemoveItemEvents();

            Item.Clear();
            foreach (T obj in originalList)
                Item.Add(obj);

            if (clearAfterSet)
                originalList = null;

            if (IsTracking)
                AddItemEvents();
        }

        /// <inheritdoc />
        protected override int ApplyUndoChange()
        {
            int result = CurrentIndex;
            NotifyCollectionChangedEventArgs? change = CurrentChange;
            if (change == null)
                return result;

            if (IsTracking)
                RemoveItemEvents();

            switch (change.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (change.NewItems?.Count > 0)
                        Item.Remove((T)change.NewItems[0]!, true);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (change.OldItems?.Count > 0)
                        Item.Insert(change.OldStartingIndex, (T)change.OldItems[0]!, true);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (change.OldItems?.Count > 0)
                        Item.ReplaceAt(change.OldStartingIndex, (T)change.OldItems[0]!, true);
                    break;
                case NotifyCollectionChangedAction.Move:
                    Item.Move(change.NewStartingIndex, change.OldStartingIndex, true);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }

            result -= 1;

            if (IsTracking)
                AddItemEvents();

            return result;
        }

        /// <inheritdoc />
        protected override int ApplyRedoChange()
        {
            int result = CurrentIndex + 1;
            NotifyCollectionChangedEventArgs? change = NextChange;
            if (change == null)
                return CurrentIndex;

            if (IsTracking)
                RemoveItemEvents();

            switch (change.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (change.NewItems?.Count > 0)
                        Item.Insert(change.NewStartingIndex, (T)change.NewItems[0]!, true);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (change.OldItems?.Count > 0)
                        Item.Remove((T)change.OldItems[0]!, true);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (change.NewItems?.Count > 0)
                        Item.ReplaceAt(change.OldStartingIndex, (T)change.NewItems[0]!, true);
                    break;
                case NotifyCollectionChangedAction.Move:
                    Item.Move(change.OldStartingIndex, change.NewStartingIndex, true);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }

            if (IsTracking)
                AddItemEvents();

            return result;
        }
    }
}
