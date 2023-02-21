using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace rmsoft.ChangeTracking
{
    public class PropertyChangeTracker : IPropertyChangeTracker
    {
        public event EventHandler TrackerUpdated;
        public INotifyPropertyChanged Target { get; }
        protected IDictionary<string, PropertyChanges> TrackedProperties { get; } = new ConcurrentDictionary<string, PropertyChanges>();
        public bool IsTracking { get; private set; }
        public IEnumerable<string> TrackedPropertyNames => TrackedProperties.Keys;
        public IEnumerable<string> ChangedPropertyNames => Changes.Select(o => o.Key);
        public IEnumerable<KeyValuePair<string, PropertyChanges>> Changes => TrackedProperties.Where(o => o.Value.Count > 1);
        public bool HasChanges => Changes.Any();
        public KeyValuePair<string, PropertyChanges> CurrentChange { get; set; } = default(KeyValuePair<string, PropertyChanges>);

        public PropertyChangeTracker(INotifyPropertyChanged target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        ~PropertyChangeTracker()
        {
            Target.PropertyChanged -= Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsTracking)
            {
                PropertyInfo property = Target.GetType().GetProperty(e.PropertyName);
                if (property.GetCustomAttributes<PropertyChangeTrackerAttribute>(true).Any())
                {
                    UpdateChange(e.PropertyName, Target.GetType().GetProperty(e.PropertyName).GetValue(Target));
                }
            }
        }

        private void RaiseTrackerUpdated()
        {
            EventHandler h = TrackerUpdated;
            h?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateChange(string propertyName, object value)
        {
            if (!TrackedProperties.ContainsKey(propertyName))
                TrackedProperties.Add(propertyName, new PropertyChanges());

            TrackedProperties[propertyName].UpdateCurrent(value);
            CurrentChange = TrackedProperties.First(o=>o.Key == propertyName);
            RaiseTrackerUpdated();
        }

        public void StartTracking()
        {
            if (IsTracking)
                throw new InvalidOperationException("Tracking is already active.");

            TrackedProperties.Clear();
            PropertyChanges changes;
            foreach (PropertyInfo obj in Target.GetType().GetProperties().Where(o => o.GetCustomAttribute<PropertyChangeTrackerAttribute>() != null))
            {
                changes = new PropertyChanges();
                changes.Add(obj.GetValue(Target));
                TrackedProperties.Add(obj.Name, changes);
            }

            IsTracking = true;
            Target.PropertyChanged += Target_PropertyChanged;
            RaiseTrackerUpdated();
        }

        public void StopTracking()
        {
            if (!IsTracking)
                throw new InvalidOperationException("Tracking is not active.");
            IsTracking = false;
            Target.PropertyChanged -= Target_PropertyChanged;
            RaiseTrackerUpdated();
        }

        public bool ResetChanges(string propertyName)
        {
            PropertyChanges changes;
            if (TrackedProperties.ContainsKey(propertyName))
            {
                changes = TrackedProperties[propertyName];
                changes.Reset();
                Target.GetType().GetProperty(propertyName).SetValue(Target, changes.Current);
                return true;
            }

            RaiseTrackerUpdated();
            return false;
        }

        public void ResetChanges()
        {
            foreach (string obj in TrackedProperties.Keys)
            {
                ResetChanges(obj);
            }
        }

        public bool CanUndo(string propertyName)
        {
            return TrackedProperties[propertyName].CanUndo;
        }

        public bool Undo(string propertyName)
        {
            bool result = TrackedProperties[propertyName].Undo();
            if (result)
                Target.GetType().GetProperty(propertyName).SetValue(Target, TrackedProperties[propertyName].Current);
            RaiseTrackerUpdated();
            return result;
        }

        public bool CanRedo(string propertyName)
        {
            return TrackedProperties[propertyName].CanRedo;
        }

        public bool Redo(string propertyName)
        {
            bool result = TrackedProperties[propertyName].Redo();
            if (result)
                Target.GetType().GetProperty(propertyName).SetValue(Target, TrackedProperties[propertyName].Current);
            RaiseTrackerUpdated();
            return result;
        }
    }
}
