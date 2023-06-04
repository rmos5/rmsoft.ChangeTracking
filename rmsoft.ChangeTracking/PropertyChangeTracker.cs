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

    public class PropertyChangeTracker : ChangeTrackerBase<INotifyPropertyChanged, PropertyChanges>, IPropertyChangeTracking
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

        public PropertyChangeTracker(INotifyPropertyChanged item)
            : base(item)
        {
        }

        ~PropertyChangeTracker()
        {
            RemoveItemEvents();
        }

        public override bool HasChanges =>
            base.HasChanges
            && Changes.Any(o => o.CurrentIndex > 0);

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

        public bool IsChanged(string propertyName)
        {
            return Changes.Any(o => o.PropertyName == propertyName);
        }

        public override bool CanUndo()
        {
            return CurrentNode != null
                && (CurrentNode.Value.CanUndo
                || CurrentNode.Previous != null);
        }

        public override bool CanRedo()
        {
            return CurrentNode != null
                && (CurrentNode.Value.CanRedo
                || CurrentNode.Next != null);
        }

        protected void ApplyChange(string name, object value)
        {
            if (IsTracking)
                RemoveItemEvents();
            Item.GetType().GetProperty(name).SetValue(Item, value);
            if (IsTracking)
                AddItemEvents();
        }

        protected override LinkedListNode<PropertyChanges> ApplyUndoChange()
        {
            LinkedListNode<PropertyChanges> result = CurrentNode;

            if (!result.Value.CanUndo)
                result = CurrentNode.Previous;

            if (result == null)
                return result;

            result.Value.Undo();
            ApplyChange(result.Value.PropertyName, result.Value.Current);

            return result;
        }

        protected override LinkedListNode<PropertyChanges> ApplyRedoChange()
        {
            LinkedListNode<PropertyChanges> result = CurrentNode;

            if (!result.Value.CanRedo)
                result = CurrentNode.Next;

            if (result == null)
                return result;

            result.Value.Redo();
            ApplyChange(result.Value.PropertyName, result.Value.Current);

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
