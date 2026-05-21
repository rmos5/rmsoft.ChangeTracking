using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace rmsoft.ChangeTracking
{
    public abstract class ChangeTrackerBase<TSource, TChange> : IChangeTracking<TSource, TChange>
        where TSource : class
    {
        private readonly ObservableCollection<TChange> changes = new ObservableCollection<TChange>();
        private bool disposed;

        public event EventHandler? TrackerUpdated;

        public TSource Item { get; }

        public bool IsTracking { get; private set; }

        public IEnumerable<TChange> Changes => changes;

        public int ChangesCount => changes.Count;

        public virtual bool HasChanges => ChangesCount > 0;

        public int CurrentIndex { get; protected set; } = -1;

        public TChange? CurrentChange => GetChange(CurrentIndex);

        public TChange? PreviousChange => GetChange(CurrentIndex - 1);

        public TChange? NextChange => GetChange(CurrentIndex + 1);

        protected ChangeTrackerBase(TSource item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        protected TChange? GetChange(int index)
        {
            return index >= 0 && index < changes.Count
                ? changes[index]
                : default;
        }

        protected abstract void StartTrackingOverride();

        protected abstract void StopTrackingOverride(bool cancelChanges);

        protected abstract void SetOriginalValues(bool clearAfterSet);

        protected abstract int ApplyUndoChange();

        protected abstract int ApplyRedoChange();

        protected void RaiseTrackerUpdated()
        {
            TrackerUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected void AddChange(TChange change)
        {
            while (CanRedo())
            {
                changes.Remove(changes.Last());
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
            if (!CanUndo())
                return;

            CurrentIndex = ApplyUndoChange();
            RaiseTrackerUpdated();
        }

        public void Redo()
        {
            if (!CanRedo())
                return;

            CurrentIndex = ApplyRedoChange();
            RaiseTrackerUpdated();
        }

        public void StartTracking()
        {
            ThrowIfDisposed();

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
            ThrowIfDisposed();

            if (!IsTracking)
                throw new InvalidOperationException("Tracking is not active.");

            IsTracking = false;
            StopTrackingOverride(cancelChanges);

            if (cancelChanges)
                SetOriginalValues(clearHistory);

            if (clearHistory)
            {
                changes.Clear();
                CurrentIndex = -1;
            }

            RaiseTrackerUpdated();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            if (IsTracking)
            {
                IsTracking = false;
                StopTrackingOverride(false);
            }

            disposed = true;
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
