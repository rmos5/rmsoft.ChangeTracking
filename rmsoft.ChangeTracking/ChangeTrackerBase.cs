using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace rmsoft.ChangeTracking
{
    public abstract class ChangeTrackerBase<TSource, TChange> : IChangeTracking<TSource, TChange>
        where TSource : class
    {
        public event EventHandler TrackerUpdated;

        public TSource Item { get; }

        public virtual bool HasChanges => Changes.Any();

        public bool IsTracking { get; private set; }

        private LinkedList<TChange> changesList = new LinkedList<TChange>();

        public IEnumerable<TChange> Changes => changesList;

        protected LinkedListNode<TChange> CurrentNode { get; private set; }

        public TChange CurrentChange => CurrentNode == null ? default(TChange) : CurrentNode.Value;

        protected ChangeTrackerBase(TSource item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        protected abstract void StartTrackingOverride();

        protected abstract void StopTrackingOverride(bool cancelChanges);

        protected abstract void SetOriginalValues(bool clear);

        protected abstract LinkedListNode<TChange> ApplyUndoChange();

        protected abstract LinkedListNode<TChange> ApplyRedoChange();

        protected void RaiseTrackerUpdated()
        {
            EventHandler h = TrackerUpdated;
            h?.Invoke(this, EventArgs.Empty);
        }

        protected void AddChange(TChange change)
        {
            while (CanRedo())
            {
                changesList.RemoveLast();
            }

            CurrentNode = changesList.AddLast(change);
            RaiseTrackerUpdated();
        }

        public virtual bool CanRedo()
        {
            return CurrentNode?.Next != null;
        }

        public virtual bool CanUndo()
        {
            return CurrentNode?.Previous != null;
        }

        public void Undo()
        {
            CurrentNode = ApplyUndoChange();
            if (CurrentNode == null)
                changesList.Clear();
            RaiseTrackerUpdated();
        }

        public void Redo()
        {
            CurrentNode = ApplyRedoChange();
            RaiseTrackerUpdated();
        }

        public void StartTracking()
        {
            if (IsTracking)
                throw new InvalidOperationException("Tracking is already active.");

            StartTrackingOverride();
            IsTracking = true;
            RaiseTrackerUpdated();
        }

        public void StopTracking(bool cancelChanges)
        {
            if (!IsTracking)
                throw new InvalidOperationException("Tracking is not active.");

            StopTrackingOverride(cancelChanges);
            CurrentNode = null;
            changesList.Clear();
            IsTracking = false;

            if (cancelChanges)
                SetOriginalValues(true);

            RaiseTrackerUpdated();
        }
    }
}
