using System;
using System.Collections.Generic;

namespace rmsoft.ChangeTracking
{
    public interface IPropertyChangeTracker
    {
        event EventHandler TrackerUpdated;
        bool HasChanges { get; }
        bool IsTracking { get; }
        IEnumerable<KeyValuePair<string, PropertyChanges>> Changes { get; }
        KeyValuePair<string, PropertyChanges> CurrentChange { get; set; }
        IEnumerable<string> TrackedPropertyNames { get; }
        IEnumerable<string> ChangedPropertyNames { get; }
        void StartTracking();
        void StopTracking();
        bool CanUndo(string propertyName);
        bool Undo(string propertyName);
        bool CanRedo(string propertyName);
        bool Redo(string propertyName);
        bool ResetChanges(string propertyName);
        void ResetChanges();
    }
}
