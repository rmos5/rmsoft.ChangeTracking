using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Represents the value history for a single tracked property change.
    /// </summary>
    public class PropertyChanges : ObservableCollection<object?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChanges" /> class.
        /// </summary>
        /// <param name="propertyName">The name of the tracked property.</param>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyName" /> is empty or whitespace.</exception>
        public PropertyChanges(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            if (propertyName.Trim().Length == 0)
                throw new ArgumentException("Invalid property name.", nameof(propertyName));

            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the name of the tracked property.
        /// </summary>
        public string PropertyName { get; }

        private int currentIndex = -1;

        /// <summary>
        /// Gets or sets the current value index in this property's value history.
        /// </summary>
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

        /// <summary>
        /// Gets the value at <see cref="CurrentIndex" />, or <see langword="null" /> when the index is outside the history.
        /// </summary>
        public object? Current =>
            CurrentIndex >= 0 && CurrentIndex < Count
                ? this[CurrentIndex]
                : null;

        /// <summary>
        /// Gets a value indicating whether this property value history can move to the previous value.
        /// </summary>
        public bool CanUndo => CurrentIndex > 0;

        /// <summary>
        /// Moves <see cref="CurrentIndex" /> to the previous value when possible.
        /// </summary>
        public void Undo()
        {
            if (CanUndo)
                CurrentIndex--;
        }

        /// <summary>
        /// Gets a value indicating whether this property value history can move to the next value.
        /// </summary>
        public bool CanRedo => Count > 0 && CurrentIndex < Count - 1;

        /// <summary>
        /// Moves <see cref="CurrentIndex" /> to the next value when possible.
        /// </summary>
        public void Redo()
        {
            if (CanRedo)
                CurrentIndex++;
        }

        /// <summary>
        /// Raises a property change notification for this value history.
        /// </summary>
        /// <param name="propertyName">The changed property name.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Adds a value to the history and makes it the current value.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public new void Add(object? value)
        {
            base.Add(value);
            CurrentIndex = Count - 1;
        }
    }
}
