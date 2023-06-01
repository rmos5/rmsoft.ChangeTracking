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
    public class ListChangeTracker<T> : ChangeTrackerBase<INotifyListChanged<T>, NotifyCollectionChangedEventArgs>
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
            for (int i = 0; i < Item.Count; i++)
            {
                OriginalList.Insert(0, Item[i]);
            }

            AddItemEvents();
        }

        protected override void StopTrackingOverride(bool cancelChanges)
        {
            RemoveItemEvents();
        }

        protected List<T> OriginalList { get; private set; }

        protected override void SetOriginalValues()
        {
            if (IsTracking)
                RemoveItemEvents();

            for (int i = 0; i < OriginalList.Count; i++)
            {
                if (Item.IndexOf(OriginalList[i]) != i)
                {
                    Item.Insert(i, OriginalList[i]);
                }
            }

            for (int i = Item.Count - 1; i > OriginalList.Count - 1; i--)
            {
                Item.RemoveAt(i);
            }

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
                    Item.RemoveAt(CurrentChange.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Item.Insert(CurrentChange.OldStartingIndex, (T)CurrentChange.OldItems[0]);
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
                    Item.Insert(CurrentChange.NewStartingIndex, (T)CurrentChange.NewItems[0]);
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

            result = result.Next;

            if (IsTracking)
                AddItemEvents();

            return result;
        }
    }
}
