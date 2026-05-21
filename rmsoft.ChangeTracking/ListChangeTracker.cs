using System.Collections.Generic;
using System.Collections.Specialized;

namespace rmsoft.ChangeTracking
{
    public interface IListChangeTracking<T> : IChangeTracking<INotifyListChanged<T>, NotifyCollectionChangedEventArgs>
    {
    }

    public class ListChangeTracker<T> : ChangeTrackerBase<INotifyListChanged<T>, NotifyCollectionChangedEventArgs>, IListChangeTracking<T>
    {
        private List<T>? originalList;

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
        }

        protected void AddItemEvents()
        {
            Item.CollectionChanged += Item_CollectionChanged;
        }

        protected void RemoveItemEvents()
        {
            Item.CollectionChanged -= Item_CollectionChanged;
        }

        protected override void StartTrackingOverride()
        {
            originalList = new List<T>(Item);
            AddItemEvents();
        }

        protected override void StopTrackingOverride(bool cancelChanges)
        {
            RemoveItemEvents();
        }

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
