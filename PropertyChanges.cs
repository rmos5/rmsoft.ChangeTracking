using System.Collections.Generic;
using System.Linq;

namespace rmsoft.ChangeTracking
{
    public class PropertyChanges : List<object>
    {
        int CurrentIndex { get; set; } = -1;
        public object Current => Count > 0 ? this[CurrentIndex] : null;

        public bool CanUndo => CurrentIndex > 0;
        public bool Undo()
        {
            if (CurrentIndex == 0)
                return false;

            CurrentIndex--;
            return true;
        }

        public bool CanRedo => Count > 0 && CurrentIndex < Count - 1;
        public bool Redo()
        {
            if (CurrentIndex == Count - 1)
                return false;

            CurrentIndex++;
            return true;
        }

        public void UpdateCurrent(object value)
        {
            int idx = IndexOf(value);
            if (idx < 0)
            {
                Add(value);
                CurrentIndex = Count - 1;
            }
            else
            {
                CurrentIndex = idx;
                if (idx == 0)
                {
                    Clear();
                    Add(value);
                }
            }
        }

        public void Reset()
        {
            if (Count == 0)
            {
                CurrentIndex = -1;
                return;
            }
            else
            {
                object change = this.First();
                Clear();
                Add(change);
                CurrentIndex = 0;
            }
        }
    }
}
