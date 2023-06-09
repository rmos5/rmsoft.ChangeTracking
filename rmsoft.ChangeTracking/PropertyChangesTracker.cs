using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace rmsoft.ChangeTracking
{
    public interface IPropertyChangeTracking : IChangeTracking<INotifyPropertyChanged, PropertyChanges>
    {
    }

    public class PropertyChangesTracker : ChangeTrackerBase<INotifyPropertyChanged, PropertyChanges>, IPropertyChangeTracking
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

        protected IEnumerable<string> GetTrackedPropertyNames() => Item.GetType().GetProperties().Where(o => o.GetCustomAttributes<PropertyChangeTrackerAttribute>(false).Any()).Select(o => o.Name);

        protected IEnumerable<string> GetChangedPropertyNames() => Changes.Select(o => o.PropertyName).Distinct();

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (GetTrackedPropertyNames().Contains(e.PropertyName))
            {
                PropertyChanges change = Changes.FirstOrDefault(o => o.PropertyName == e.PropertyName);
                bool add = change == null;
                if (change == null)
                {
                    change = new PropertyChanges(e.PropertyName);
                    change.Add(OriginalPropertyValues.First(o => o.Name == e.PropertyName).Value);
                }

                change.Add(Item.GetType().GetProperty(e.PropertyName).GetValue(Item));
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
            Item.GetType().GetProperty(name).SetValue(Item, value);
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

        protected PropertyValue GetInitialChange(PropertyValue change)
        {
            return OriginalPropertyValues.First(o => o.Name == change.Name);
        }

        protected override void StartTrackingOverride()
        {
            OriginalPropertyValues = Item.GetType().GetProperties().Where(o => o.GetCustomAttributes<PropertyChangeTrackerAttribute>(false).Any()).Select(o => new PropertyValue(o.Name, o.GetValue(Item))).ToList();
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
                Item.GetType().GetProperty(obj.Name).SetValue(Item, obj.Value);
            }

            if (clearAfterSet)
                OriginalPropertyValues = null;

            if (IsTracking)
                AddItemEvents();
        }
    }
}
