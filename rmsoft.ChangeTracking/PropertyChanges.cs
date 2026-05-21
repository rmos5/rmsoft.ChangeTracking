using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public class PropertyChanges : ObservableCollection<object>
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

        private int currentIndex = -1;

        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                if (value != currentIndex)
                {
                    currentIndex = value;
                    OnPropertyChanged(nameof(CurrentIndex));
                    OnPropertyChanged(nameof(Current));
                }
            }
        }

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            base.OnPropertyChanged(e);
        }

        public new void Add(object value)
        {
            base.Add(value);
            CurrentIndex++;
        }
    }
}
