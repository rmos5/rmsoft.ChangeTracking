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
        protected struct PropertyValue
        {
            public string Name { get; }
            public object Value { get; }

            public PropertyValue(string name, object value)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                if (name.Trim().Length == 0)
                    throw new ArgumentException(nameof(name));

                Name = name;
                Value = value;
            }
        }

        public PropertyChangesTracker(INotifyPropertyChanged item)
            : base(item)
        {
        }

        ~PropertyChangesTracker()
        {
            RemoveItemEvents();
        }

        public override bool HasChanges =>
            base.HasChanges
            && Changes.Any(o => o.Count > 0);

        protected IEnumerable<string> GetTrackedPropertyNames() => trackedProperties.Keys;

        protected IEnumerable<string> GetChangedPropertyNames() => Changes.Select(o => o.PropertyName).Distinct();

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IEnumerable<string> changedProperties =
                string.IsNullOrWhiteSpace(e.PropertyName)
                    ? GetTrackedPropertyNames()
                    : new[] { e.PropertyName };

            foreach (string propertyName in changedProperties)
            {
                if (!trackedProperties.TryGetValue(propertyName, out PropertyInfo trackedProperty))
                    continue;

                PropertyChanges change = Changes.FirstOrDefault(o => o.PropertyName == propertyName);
                bool add = change == null;
                if (change == null)
                {
                    change = new PropertyChanges(propertyName);
                    change.Add(originalValues[propertyName].Value);
                }

                change.Add(trackedProperty.GetValue(Item));
                if (add)
                    AddChange(change);
            }
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
                || (CurrentChange != null
                && CurrentChange.CanUndo);
        }

        public override bool CanRedo()
        {
            return base.CanRedo()
                || (CurrentChange != null
                && CurrentChange.CanRedo);
        }

        protected void ApplyChange(string name, object value)
        {
            if (IsTracking)
                RemoveItemEvents();
            trackedProperties[name].SetValue(Item, value);
            if (IsTracking)
                AddItemEvents();
        }

        protected override int ApplyUndoChange()
        {
            int result = CurrentIndex;
            PropertyChanges change = CurrentChange;

            if (!change.CanUndo)
            {
                result -= 1;
                change = CurrentChange;
            }

            if (result < 0)
                return result;

            change.Undo();
            ApplyChange(change.PropertyName, change.Current);

            if (!change.CanUndo)
            {
                result -= 1;
            }

            return result;
        }

        protected override int ApplyRedoChange()
        {
            if (CurrentIndex < 0)
                CurrentIndex++;

            int result = CurrentIndex;
            PropertyChanges change = CurrentChange;

            if (!change.CanRedo)
            {
                CurrentIndex++;
                result = CurrentIndex;
                change = CurrentChange;
            }

            if (result < 0)
                return result;

            change.Redo();
            ApplyChange(change.PropertyName, change.Current);

            return result;
        }

        protected IEnumerable<PropertyValue> OriginalPropertyValues { get; private set; }

        private IDictionary<string, PropertyInfo> trackedProperties = new Dictionary<string, PropertyInfo>();
        private IDictionary<string, PropertyValue> originalValues = new Dictionary<string, PropertyValue>();

        protected PropertyValue GetInitialChange(PropertyValue change)
        {
            return OriginalPropertyValues.First(o => o.Name == change.Name);
        }

        protected override void StartTrackingOverride()
        {
            trackedProperties = Item.GetType()
                .GetProperties()
                .Where(o => o.GetCustomAttributes<PropertyChangeTrackerAttribute>(false).Any())
                .ToDictionary(o => o.Name, o => o);

            OriginalPropertyValues = trackedProperties
                .Select(o => new PropertyValue(o.Key, o.Value.GetValue(Item)))
                .ToList();
            originalValues = OriginalPropertyValues.ToDictionary(o => o.Name, o => o);
            AddItemEvents();
        }

        protected override void StopTrackingOverride(bool cancelChanges)
        {
            RemoveItemEvents();
        }

        protected override void SetOriginalValues(bool clearAfterSet)
        {
            if (IsTracking)
                RemoveItemEvents();

            foreach (PropertyValue obj in OriginalPropertyValues)
            {
                trackedProperties[obj.Name].SetValue(Item, obj.Value);
            }

            if (clearAfterSet)
            {
                OriginalPropertyValues = null;
                originalValues = new Dictionary<string, PropertyValue>();
            }

            if (IsTracking)
                AddItemEvents();
        }
    }
}
