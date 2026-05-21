using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace rmsoft.ChangeTracking
{
    public interface IPropertyChangesTracking : IChangeTracking<INotifyPropertyChanged, PropertyChanges>
    {
    }

    public class PropertyChangesTracker : ChangeTrackerBase<INotifyPropertyChanged, PropertyChanges>, IPropertyChangesTracking
    {
        private readonly Dictionary<string, PropertyInfo> trackedProperties = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, object?>? originalPropertyValues;

        public PropertyChangesTracker(INotifyPropertyChanged item)
            : base(item)
        {
        }

        public override bool HasChanges =>
            base.HasChanges
            && Changes.Any(o => o.Count > 0);

        protected IEnumerable<string> GetTrackedPropertyNames() => trackedProperties.Keys;

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
            if (!trackedProperties.TryGetValue(propertyName, out PropertyInfo property))
                return;

            if (originalPropertyValues == null)
                throw new InvalidOperationException("Original property values have not been captured.");

            PropertyChanges? change = Changes.FirstOrDefault(o => o.PropertyName == propertyName);
            bool add = change == null;
            if (change == null)
            {
                change = new PropertyChanges(propertyName);
                change.Add(originalPropertyValues[propertyName]);
            }

            change.Add(property.GetValue(Item));
            if (add)
                AddChange(change);
        }

        protected void AddItemEvents()
        {
            Item.PropertyChanged += Item_PropertyChanged;
        }

        protected void RemoveItemEvents()
        {
            Item.PropertyChanged -= Item_PropertyChanged;
        }

        public override bool CanUndo()
        {
            return base.CanUndo()
                || CurrentChange?.CanUndo == true;
        }

        public override bool CanRedo()
        {
            return base.CanRedo()
                || CurrentChange?.CanRedo == true;
        }

        protected void ApplyChange(string name, object? value)
        {
            if (!trackedProperties.TryGetValue(name, out PropertyInfo property))
                throw new InvalidOperationException($"Tracked property '{name}' was not found.");

            if (IsTracking)
                RemoveItemEvents();
            property.SetValue(Item, value);
            if (IsTracking)
                AddItemEvents();
        }

        protected override int ApplyUndoChange()
        {
            int result = CurrentIndex;
            PropertyChanges? change = CurrentChange;

            if (change == null)
                return result;

            if (!change.CanUndo)
            {
                result -= 1;
                change = GetChange(result);
            }

            if (result < 0 || change == null)
                return result;

            change.Undo();
            ApplyChange(change.PropertyName, change.Current);

            if (!change.CanUndo)
                result -= 1;

            return result;
        }

        protected override int ApplyRedoChange()
        {
            int result = CurrentIndex < 0 ? 0 : CurrentIndex;
            PropertyChanges? change = GetChange(result);

            if (change == null)
                return result;

            if (!change.CanRedo)
            {
                result += 1;
                change = GetChange(result);
            }

            if (change == null)
                return CurrentIndex;

            change.Redo();
            ApplyChange(change.PropertyName, change.Current);

            return result;
        }

        protected override void StartTrackingOverride()
        {
            trackedProperties.Clear();
            foreach (PropertyInfo property in Item.GetType().GetProperties().Where(o => o.GetCustomAttributes<PropertyChangeTrackerAttribute>(false).Any()))
            {
                trackedProperties[property.Name] = property;
            }

            originalPropertyValues = trackedProperties.ToDictionary<KeyValuePair<string, PropertyInfo>, string, object?>(
                o => o.Key,
                o => o.Value.GetValue(Item));

            AddItemEvents();
        }

        protected override void StopTrackingOverride(bool cancelChanges)
        {
            RemoveItemEvents();
        }

        protected override void SetOriginalValues(bool clearAfterSet)
        {
            if (originalPropertyValues == null)
                return;

            if (IsTracking)
                RemoveItemEvents();

            foreach (KeyValuePair<string, object?> originalValue in originalPropertyValues)
            {
                if (trackedProperties.TryGetValue(originalValue.Key, out PropertyInfo property))
                    property.SetValue(Item, originalValue.Value);
            }

            if (clearAfterSet)
                originalPropertyValues = null;

            if (IsTracking)
                AddItemEvents();
        }
    }
}
