using System;
using System.Collections.Generic;

namespace rmsoft.ChangeTracking
{
    public class PropertyChanges : List<object>
    {
        public PropertyChanges(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            if (propertyName.Trim().Length == 0)
                throw new ArgumentException("Invalid property name.", nameof(propertyName));

            PropertyName = propertyName;    
        }

        public string PropertyName { get; }

        public int CurrentIndex { get; private set; } = -1;

        public object Current => Count > 0 ? this[CurrentIndex] : null;

        public bool CanUndo => CurrentIndex > 0;
        public void Undo()
        {
            CurrentIndex--;
        }

        public bool CanRedo => Count > 0 && CurrentIndex < Count - 1;
        public void Redo()
        {
            CurrentIndex++;
        }

        public new void Add(object value)
        {
            base.Add(value);
            CurrentIndex++;
        }
    }
}
