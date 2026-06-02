using System;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Marks a property on a <see cref="TrackedObjectBase" />-derived type as participating in change tracking.
    /// </summary>
    /// <remarks>
    /// Marked properties must be instance properties with a getter and setter. Indexers, static properties, and
    /// read-only properties are rejected when tracking starts.
    /// </remarks>
    public class PropertyChangeTrackerAttribute : Attribute
    {
    }
}
