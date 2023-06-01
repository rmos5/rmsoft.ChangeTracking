using System;
using System.Collections.Generic;

namespace rmsoft.ChangeTracking
{
    public interface IChangeTracker
    {
        event EventHandler TrackerUpdated;
        bool HasChanges { get; }
        bool IsTracking { get; }
        void StartTracking();
        void StopTracking(bool cancelChanges);
        bool CanUndo();
        void Undo();
        bool CanRedo();
        void Redo();
    }

    public interface IChangeTracker<TSource, TChange> : IChangeTracker
        where TSource : class
    {
        TSource Item { get; }

        IEnumerable<TChange> Changes { get; }
        TChange CurrentChange { get; }
    }
}
