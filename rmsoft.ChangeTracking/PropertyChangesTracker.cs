using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Tracks property changes on an <see cref="INotifyPropertyChanged" /> object.
    /// </summary>
    public interface IPropertyChangesTracking : IChangeTracking<INotifyPropertyChanged, PropertyChanges>
    {
    }

    /// <summary>
    /// Tracks chronological changes for properties marked with <see cref="PropertyChangeTrackerAttribute" />.
    /// </summary>
    public class PropertyChangesTracker : ChangeTrackerBase<INotifyPropertyChanged, PropertyChanges>, IPropertyChangesTracking
    {
        private readonly Dictionary<string, PropertyInfo> trackedProperties = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, object?>? originalPropertyValues;
        private Dictionary<string, object?>? currentPropertyValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangesTracker" /> class.
        /// </summary>
        /// <param name="item">The object whose property notifications should be tracked.</param>
        public PropertyChangesTracker(INotifyPropertyChanged item)
            : base(item)
        {
        }

        /// <inheritdoc />
        public override bool HasChanges =>
            base.HasChanges
            && Changes.Any(o => o.Count > 0);

        /// <summary>
        /// Gets the names of properties that are configured for tracking.
        /// </summary>
        /// <returns>The tracked property names.</returns>
        protected IEnumerable<string> GetTrackedPropertyNames() => trackedProperties.Keys;

        /// <summary>
        /// Gets the names of properties that have recorded changes.
        /// </summary>
        /// <returns>The changed property names.</returns>
        protected IEnumerable<string> GetChangedPropertyNames() => Changes.Select(o => o.PropertyName).Distinct();

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            IEnumerable<string> propertyNames = string.IsNullOrEmpty(e.PropertyName)
                ? trackedProperties.Keys
                : new[] { e.PropertyName };

            foreach (string propertyName in propertyNames)
            {
                TrackPropertyChange(propertyName);
            }
        }

        private void TrackPropertyChange(string propertyName)
        {
            if (!trackedProperties.TryGetValue(propertyName, out PropertyInfo? property) || property == null)
                return;

            if (originalPropertyValues == null)
                throw new InvalidOperationException("Original property values have not been captured.");

            if (currentPropertyValues == null)
                throw new InvalidOperationException("Current property values have not been captured.");

            object? previousValue = currentPropertyValues[propertyName];
            object? currentValue = property.GetValue(Item);
            if (Equals(previousValue, currentValue))
                return;

            object? originalValue = originalPropertyValues[propertyName];
            if (Equals(originalValue, currentValue))
            {
                currentPropertyValues[propertyName] = currentValue;
                ClearRedoChanges();
                RemoveChanges(o => o.PropertyName == propertyName);
                return;
            }

            PropertyChanges change = new PropertyChanges(propertyName);
            change.Add(previousValue);
            change.Add(currentValue);
            currentPropertyValues[propertyName] = currentValue;
            AddChange(change);
        }

        /// <summary>
        /// Subscribes to property change notifications on <see cref="ChangeTrackerBase{TSource, TChange}.Item" />.
        /// </summary>
        protected void AddItemEvents()
        {
            Item.PropertyChanged += Item_PropertyChanged;
        }

        /// <summary>
        /// Unsubscribes from property change notifications on <see cref="ChangeTrackerBase{TSource, TChange}.Item" />.
        /// </summary>
        protected void RemoveItemEvents()
        {
            Item.PropertyChanged -= Item_PropertyChanged;
        }

        /// <inheritdoc />
        public override bool CanUndo()
        {
            return base.CanUndo()
                || CurrentChange?.CanUndo == true;
        }

        /// <inheritdoc />
        public override bool CanRedo()
        {
            return base.CanRedo()
                || CurrentChange?.CanRedo == true;
        }

        /// <summary>
        /// Applies a tracked property value to the source item.
        /// </summary>
        /// <param name="name">The tracked property name.</param>
        /// <param name="value">The value to apply.</param>
        /// <exception cref="InvalidOperationException">The tracked property cannot be found.</exception>
        protected void ApplyChange(string name, object? value)
        {
            if (!trackedProperties.TryGetValue(name, out PropertyInfo? property) || property == null)
                throw new InvalidOperationException($"Tracked property '{name}' was not found.");

            if (IsTracking)
                RemoveItemEvents();
            property.SetValue(Item, value);
            currentPropertyValues![name] = value;
            if (IsTracking)
                AddItemEvents();
        }

        /// <inheritdoc />
        protected override int ApplyUndoChange()
        {
            PropertyChanges? change = CurrentChange;

            if (change == null || !change.CanUndo)
                return CurrentIndex;

            change.Undo();
            ApplyChange(change.PropertyName, change.Current);

            return CurrentIndex - 1;
        }

        /// <inheritdoc />
        protected override int ApplyRedoChange()
        {
            int result = CurrentIndex + 1;
            PropertyChanges? change = NextChange;

            if (change == null || !change.CanRedo)
                return CurrentIndex;

            change.Redo();
            ApplyChange(change.PropertyName, change.Current);

            return result;
        }

        /// <inheritdoc />
        protected override void StartTrackingOverride()
        {
            trackedProperties.Clear();
            foreach (PropertyInfo property in Item.GetType().GetProperties().Where(o => o.GetCustomAttributes<PropertyChangeTrackerAttribute>(false).Any()))
            {
                ValidateTrackedProperty(property);
                trackedProperties[property.Name] = property;
            }

            originalPropertyValues = trackedProperties.ToDictionary<KeyValuePair<string, PropertyInfo>, string, object?>(
                o => o.Key,
                o => o.Value.GetValue(Item));
            currentPropertyValues = new Dictionary<string, object?>(originalPropertyValues);

            AddItemEvents();
        }

        private static void ValidateTrackedProperty(PropertyInfo property)
        {
            if (property.GetIndexParameters().Length > 0)
                throw new InvalidOperationException($"Tracked property '{property.Name}' cannot be an indexer.");

            if (property.GetMethod == null)
                throw new InvalidOperationException($"Tracked property '{property.Name}' must have a getter.");

            if (property.SetMethod == null)
                throw new InvalidOperationException($"Tracked property '{property.Name}' must have a setter.");

            if (property.GetMethod.IsStatic || property.SetMethod.IsStatic)
                throw new InvalidOperationException($"Tracked property '{property.Name}' cannot be static.");
        }

        /// <inheritdoc />
        protected override void StopTrackingOverride(bool cancelChanges)
        {
            RemoveItemEvents();
        }

        /// <inheritdoc />
        protected override void SetOriginalValues(bool clearAfterSet)
        {
            if (originalPropertyValues == null)
                return;

            if (IsTracking)
                RemoveItemEvents();

            foreach (KeyValuePair<string, object?> originalValue in originalPropertyValues)
            {
                if (trackedProperties.TryGetValue(originalValue.Key, out PropertyInfo? property) && property != null)
                {
                    property.SetValue(Item, originalValue.Value);
                    currentPropertyValues![originalValue.Key] = originalValue.Value;
                }
            }

            if (clearAfterSet)
            {
                originalPropertyValues = null;
                currentPropertyValues = null;
            }

            if (IsTracking)
                AddItemEvents();
        }
    }
}
