using System;
using System.Collections.Generic;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Defines common change-tracking operations for an object or collection.
    /// </summary>
    public interface IChangeTracking : IDisposable
    {
        /// <summary>
        /// Occurs when the tracker state, current change, or undo/redo availability changes.
        /// </summary>
        event EventHandler? TrackerUpdated;

        /// <summary>
        /// Gets the number of tracked changes in the history.
        /// </summary>
        int ChangesCount { get; }

        /// <summary>
        /// Gets a value indicating whether the tracker currently contains changes.
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Gets a value indicating whether change tracking is currently active.
        /// </summary>
        bool IsTracking { get; }

        /// <summary>
        /// Starts recording changes.
        /// </summary>
        void StartTracking();

        /// <summary>
        /// Stops recording changes.
        /// </summary>
        /// <param name="cancelChanges">
        /// <see langword="true" /> to restore the original values; <see langword="false" /> to keep current values.
        /// </param>
        void StopTracking(bool cancelChanges);

        /// <summary>
        /// Stops recording changes.
        /// </summary>
        /// <param name="cancelChanges">
        /// <see langword="true" /> to restore the original values; <see langword="false" /> to keep current values.
        /// </param>
        /// <param name="clearHistory">
        /// <see langword="true" /> to clear the change history; <see langword="false" /> to keep it available.
        /// </param>
        void StopTracking(bool cancelChanges, bool clearHistory);

        /// <summary>
        /// Returns whether the tracker can undo the current change.
        /// </summary>
        /// <returns><see langword="true" /> when an undo operation is available; otherwise, <see langword="false" />.</returns>
        bool CanUndo();

        /// <summary>
        /// Undoes the current change when an undo operation is available.
        /// </summary>
        void Undo();

        /// <summary>
        /// Returns whether the tracker can redo the next change.
        /// </summary>
        /// <returns><see langword="true" /> when a redo operation is available; otherwise, <see langword="false" />.</returns>
        bool CanRedo();

        /// <summary>
        /// Redoes the next change when a redo operation is available.
        /// </summary>
        void Redo();
    }

    /// <summary>
    /// Defines change-tracking operations for a specific source and change-record type.
    /// </summary>
    /// <typeparam name="TSource">The source object or collection being tracked.</typeparam>
    /// <typeparam name="TChange">The type used to describe each tracked change.</typeparam>
    public interface IChangeTracking<TSource, TChange> : IChangeTracking
        where TSource : class
    {
        /// <summary>
        /// Gets the source object or collection being tracked.
        /// </summary>
        TSource Item { get; }

        /// <summary>
        /// Gets the tracked change history.
        /// </summary>
        IEnumerable<TChange> Changes { get; }

        /// <summary>
        /// Gets the current change in the undo/redo history.
        /// </summary>
        TChange? CurrentChange { get; }

        /// <summary>
        /// Gets the next change in the redo history.
        /// </summary>
        TChange? NextChange { get; }

        /// <summary>
        /// Gets the previous change in the undo history.
        /// </summary>
        TChange? PreviousChange { get; }
    }
}
