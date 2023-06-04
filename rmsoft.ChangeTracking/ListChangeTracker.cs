using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public interface INotifyListChanged<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        void Move(int oldIndex, int newIndex);

        void ReplaceAt(int index, T item);
    }

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
            //} else if (e.Action == NotifyCollectionChangedAction.Replace)
            //{
            //    return; //ObservableCollection<T>.SetItem raises Replace event, but behaves wrongly, replaced item removed from collection, .Net bug
            //}

            AddChange(e);
        }

        protected void RemoveItemEvents()
        {
            Item.CollectionChanged -= Item_CollectionChanged;
        }

        protected void AddItemEvents()
        {
            Item.CollectionChanged += Item_CollectionChanged;
        }

        public override bool CanUndo()
        {
            return base.CanUndo()
                || CurrentNode != null;
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

        protected override LinkedListNode<NotifyCollectionChangedEventArgs> ApplyUndoChange()
        {
            LinkedListNode<NotifyCollectionChangedEventArgs> result = CurrentNode;
            NotifyCollectionChangedEventArgs change = result.Value;

            if (IsTracking)
                RemoveItemEvents();

            switch (CurrentChange.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Item.RemoveAt(change.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Item.Insert(change.OldStartingIndex, (T)change.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Item.ReplaceAt(change.OldStartingIndex, (T)change.OldItems[0]);
                    if (result.Previous?.Value?.Action == NotifyCollectionChangedAction.Replace)
                    {
                        result = result.Previous;
                        change = result.Value;
                        Item.ReplaceAt(change.OldStartingIndex, (T)change.OldItems[0]);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Item.Move(change.NewStartingIndex, change.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }

            result = result.Previous;

            if (IsTracking)
                AddItemEvents();

            return result;
        }

        protected override LinkedListNode<NotifyCollectionChangedEventArgs> ApplyRedoChange()
        {
            LinkedListNode<NotifyCollectionChangedEventArgs> result = CurrentNode;
            NotifyCollectionChangedEventArgs change = result.Value;

            if (IsTracking)
                RemoveItemEvents();

            switch (change.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Item.Insert(change.NewStartingIndex, (T)change.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Item.RemoveAt(change.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Item.ReplaceAt(change.NewStartingIndex, (T)change.NewItems[0]);
                    if (result.Next?.Value?.Action == NotifyCollectionChangedAction.Replace)
                    {
                        result = result.Next;
                        change = result.Value;
                        Item.ReplaceAt(change.NewStartingIndex, (T)change.NewItems[0]);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Item.Move(change.OldStartingIndex, change.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }

            result = result.Next;

            if (IsTracking)
                AddItemEvents();

            return result;
        }
    }
}
