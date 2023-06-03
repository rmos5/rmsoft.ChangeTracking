using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace rmsoft.ChangeTracking
{
    public interface INotifyListChanged<T> : IList<T>, INotifyCollectionChanged
    {
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
                return; //Reset is not supported because ObservableCollection<T> reset event args does not contain inforamtion about removed items, .Net bug
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
            foreach(T obj in Item)
                OriginalList.Add(obj);
            AddItemEvents();
        }

        protected override void StopTrackingOverride(bool cancelChanges)
        {
            RemoveItemEvents();
        }

        protected List<T> OriginalList { get; private set; }

        protected override void SetOriginalValues(bool clear)
        {
            if (IsTracking)
                RemoveItemEvents();

            Item.Clear();
            foreach (T obj in OriginalList)
                Item.Add(obj);

            if (clear)
                OriginalList = null;

            if (IsTracking)
                AddItemEvents();
        }

        protected override LinkedListNode<NotifyCollectionChangedEventArgs> ApplyUndoChange()
        {
            LinkedListNode<NotifyCollectionChangedEventArgs> result = CurrentNode;

            if (IsTracking)
                RemoveItemEvents();

            switch (CurrentChange.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Item.RemoveAt(result.Value.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Item.Insert(result.Value.OldStartingIndex, (T)result.Value.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }

            result = CurrentNode.Previous;

            if (IsTracking)
                AddItemEvents();

            return result;
        }

        protected override LinkedListNode<NotifyCollectionChangedEventArgs> ApplyRedoChange()
        {
            LinkedListNode<NotifyCollectionChangedEventArgs> result = CurrentNode;

            if (IsTracking)
                RemoveItemEvents();

            switch (CurrentChange.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Item.Insert(result.Value.NewStartingIndex, (T)result.Value.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Item.RemoveAt(CurrentChange.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }

            result = CurrentNode.Next;

            if (IsTracking)
                AddItemEvents();

            return result;
        }
    }
}
