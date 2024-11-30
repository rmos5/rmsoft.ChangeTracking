using System.Collections.Generic;
using System.Collections.Specialized;

namespace rmsoft.ChangeTracking
{
    public interface IListChangeTracking<T> : IChangeTracking<INotifyListChanged<T>, NotifyCollectionChangedEventArgs>
    {
    }

    public class ListChangeTracker<T> : ChangeTrackerBase<INotifyListChanged<T>, NotifyCollectionChangedEventArgs>, IListChangeTracking<T>
    {
        public ListChangeTracker(INotifyListChanged<T> item)
            : base(item)
        {
        }

        ~ListChangeTracker()
        {
            RemoveItemEvents();
        }

        private void Item_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                StopTracking(true);
                return; //Reset is not supported because ObservableCollection<T> reset event args does not contain inforamtion about removed items, .Net bug
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
            OriginalList = new List<T>(Item.Count);
            foreach (T obj in Item)
                OriginalList.Add(obj);
            AddItemEvents();
        }

        protected override void StopTrackingOverride(bool cancelChanges)
        {
            RemoveItemEvents();
        }

        protected List<T> OriginalList { get; private set; }

        protected override void SetOriginalValues(bool clearAfterSet)
        {
            if (IsTracking)
                RemoveItemEvents();

            Item.Clear();
            foreach (T obj in OriginalList)
                Item.Add(obj);

            if (clearAfterSet)
                OriginalList = null;

            if (IsTracking)
                AddItemEvents();
        }

        protected override int ApplyUndoChange()
        {
            int result = CurrentIndex;
            NotifyCollectionChangedEventArgs change = CurrentChange;

            if (IsTracking)
                RemoveItemEvents();

            switch (CurrentChange.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Item.Remove((T)change.NewItems[0], true);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Item.Insert(change.OldStartingIndex, (T)change.OldItems[0], true);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Item.ReplaceAt(change.OldStartingIndex, (T)change.OldItems[0], true);
                    if (CanUndo() && PreviousChange.Action == NotifyCollectionChangedAction.Replace)
                    {
                        result -= 1;
                        Item.ReplaceAt(change.OldStartingIndex, (T)change.OldItems[0], true);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Item.Move(change.NewStartingIndex, change.OldStartingIndex, true);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
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
            NotifyCollectionChangedEventArgs change = NextChange;

            if (IsTracking)
                RemoveItemEvents();

            switch (change.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Item.Insert(change.NewStartingIndex, (T)change.NewItems[0], true);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Item.Remove((T)change.OldItems[0], true);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Item.ReplaceAt(change.OldStartingIndex, (T)change.NewItems[0], true);
                    if (CanRedo() && NextChange.Action == NotifyCollectionChangedAction.Replace)
                    {
                        result += 1;
                        Item.ReplaceAt(change.OldStartingIndex, (T)change.NewItems[0], true);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Item.Move(change.OldStartingIndex, change.NewStartingIndex, true);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }

            if (IsTracking)
                AddItemEvents();

            return result;
        }
    }
}
