using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Provides common undo/redo history management for concrete change trackers.
    /// </summary>
    /// <typeparam name="TSource">The tracked source type.</typeparam>
    /// <typeparam name="TChange">The type used to describe a tracked change.</typeparam>
    public abstract class ChangeTrackerBase<TSource, TChange> : IChangeTracking<TSource, TChange>
        where TSource : class
    {
        private readonly ObservableCollection<TChange> changes = new ObservableCollection<TChange>();
        private bool disposed;

        /// <inheritdoc />
        public event EventHandler? TrackerUpdated;

        /// <inheritdoc />
        public TSource Item { get; }

        /// <inheritdoc />
        public bool IsTracking { get; private set; }

        /// <inheritdoc />
        public IEnumerable<TChange> Changes => changes;

        /// <inheritdoc />
        public int ChangesCount => changes.Count;

        /// <inheritdoc />
        public virtual bool HasChanges => ChangesCount > 0;

        /// <summary>
        /// Gets the current zero-based position in the change history.
        /// </summary>
        public int CurrentIndex { get; protected set; } = -1;

        /// <inheritdoc />
        public TChange? CurrentChange => GetChange(CurrentIndex);

        /// <inheritdoc />
        public TChange? PreviousChange => GetChange(CurrentIndex - 1);

        /// <inheritdoc />
        public TChange? NextChange => GetChange(CurrentIndex + 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeTrackerBase{TSource, TChange}" /> class.
        /// </summary>
        /// <param name="item">The source object or collection to track.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is <see langword="null" />.</exception>
        protected ChangeTrackerBase(TSource item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        /// <summary>
        /// Gets the change at the specified history index.
        /// </summary>
        /// <param name="index">The zero-based history index.</param>
        /// <returns>The change at <paramref name="index" />, or the default value when the index is outside the history.</returns>
        protected TChange? GetChange(int index)
        {
            return index >= 0 && index < changes.Count
                ? changes[index]
                : default;
        }

        /// <summary>
        /// Starts concrete tracker event subscriptions and captures original values.
        /// </summary>
        protected abstract void StartTrackingOverride();

        /// <summary>
        /// Stops concrete tracker event subscriptions.
        /// </summary>
        /// <param name="cancelChanges">Whether the caller is cancelling changes.</param>
        protected abstract void StopTrackingOverride(bool cancelChanges);

        /// <summary>
        /// Restores original values captured when tracking started.
        /// </summary>
        /// <param name="clearAfterSet">Whether captured original values should be cleared after restoration.</param>
        protected abstract void SetOriginalValues(bool clearAfterSet);

        /// <summary>
        /// Applies the current undo operation.
        /// </summary>
        /// <returns>The new current history index.</returns>
        protected abstract int ApplyUndoChange();

        /// <summary>
        /// Applies the next redo operation.
        /// </summary>
        /// <returns>The new current history index.</returns>
        protected abstract int ApplyRedoChange();

        /// <summary>
        /// Raises <see cref="TrackerUpdated" />.
        /// </summary>
        protected void RaiseTrackerUpdated()
        {
            TrackerUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds a change to the history and clears any redo branch.
        /// </summary>
        /// <param name="change">The change to add.</param>
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

        /// <inheritdoc />
        public virtual bool CanRedo()
        {
            return HasChanges
                && CurrentIndex < ChangesCount - 1;
        }

        /// <inheritdoc />
        public virtual bool CanUndo()
        {
            return HasChanges
                && CurrentIndex >= 0;
        }

        /// <inheritdoc />
        public void Undo()
        {
            if (!CanUndo())
                return;

            CurrentIndex = ApplyUndoChange();
            RaiseTrackerUpdated();
        }

        /// <inheritdoc />
        public void Redo()
        {
            if (!CanRedo())
                return;

            CurrentIndex = ApplyRedoChange();
            RaiseTrackerUpdated();
        }

        /// <inheritdoc />
        public void StartTracking()
        {
            ThrowIfDisposed();

            if (IsTracking)
                throw new InvalidOperationException("Tracking is already active.");

            StartTrackingOverride();
            IsTracking = true;
            RaiseTrackerUpdated();
        }

        /// <inheritdoc />
        public void StopTracking(bool cancelChanges)
        {
            StopTracking(cancelChanges, true);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
