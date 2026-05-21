using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace rmsoft.ChangeTracking
{
    public abstract class ChangeTrackerBase<TSource, TChange> : IChangeTracking<TSource, TChange>
        where TSource : class
    {
        public event EventHandler TrackerUpdated;

        public TSource Item { get; }

        public bool IsTracking { get; private set; }

        private ObservableCollection<TChange> changes = new ObservableCollection<TChange>();

        public IEnumerable<TChange> Changes => changes;

        public int ChangesCount => changes.Count;

        public virtual bool HasChanges => ChangesCount > 0;

        public int CurrentIndex { get; protected set; } = -1;

        public TChange CurrentChange => CurrentIndex < 0 ? default(TChange) : changes[CurrentIndex];

        public TChange PreviousChange => 
            CurrentIndex > 0 
            ? changes[CurrentIndex - 1] 
            : default(TChange);

        public TChange NextChange => 
            CurrentIndex < ChangesCount - 1 
            && HasChanges 
            ? changes[CurrentIndex + 1] 
            : default(TChange);

        protected ChangeTrackerBase(TSource item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        protected abstract void StartTrackingOverride();

        protected abstract void StopTrackingOverride(bool cancelChanges);

        protected abstract void SetOriginalValues(bool clearAfterSet);

        protected abstract int ApplyUndoChange();

        protected abstract int ApplyRedoChange();

        protected void RaiseTrackerUpdated()
        {
            EventHandler h = TrackerUpdated;
            h?.Invoke(this, EventArgs.Empty);
        }

        protected void AddChange(TChange change)
        {
            TChange last;
            while (CanRedo())
            {
                last = changes.Last();
                changes.Remove(last);
            }

            changes.Add(change);
            CurrentIndex++;
            RaiseTrackerUpdated();
        }

        public virtual bool CanRedo()
        {
            return HasChanges
                && CurrentIndex < ChangesCount - 1;
        }

        public virtual bool CanUndo()
        {
            return HasChanges
                && CurrentIndex >= 0;

        }

        public void Undo()
        {
            CurrentIndex = ApplyUndoChange();
            RaiseTrackerUpdated();
        }

        public void Redo()
        {
            CurrentIndex = ApplyRedoChange();
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
            StopTracking(cancelChanges, true);
        }

        public void StopTracking(bool cancelChanges, bool clearHistory)
        {
            if (!IsTracking)
                throw new InvalidOperationException("Tracking is not active.");

            IsTracking = false;
            StopTrackingOverride(cancelChanges);
            if (clearHistory)
            {
                changes.Clear();
                CurrentIndex = -1;
            }
            
            if (cancelChanges)
                SetOriginalValues(true);

            RaiseTrackerUpdated();
        }
    }
}
