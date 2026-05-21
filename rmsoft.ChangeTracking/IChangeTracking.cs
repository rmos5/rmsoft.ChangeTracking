using System;
using System.Collections.Generic;

namespace rmsoft.ChangeTracking
{
    public interface IChangeTracking
    {
        event EventHandler TrackerUpdated;
        int ChangesCount { get; }
        bool HasChanges { get; }
        bool IsTracking { get; }
        void StartTracking();
        void StopTracking(bool cancelChanges);
        bool CanUndo();
        void Undo();
        bool CanRedo();
        void Redo();
    }

    public interface IChangeTracking<TSource, TChange> : IChangeTracking
        where TSource : class
    {
        TSource Item { get; }
        IEnumerable<TChange> Changes { get; }
        TChange CurrentChange { get; }
        TChange NextChange { get; }
        TChange PreviousChange { get; }
    }
}
