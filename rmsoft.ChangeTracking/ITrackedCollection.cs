using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public interface INotifyListChanged<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        void Add(T item, bool select);

        void Insert(int index, T item, bool select);

        void Remove(T item, bool select);

        void Move(int oldIndex, int newIndex, bool select);

        void ReplaceAt(int index, T item, bool select);
    }

    public interface ITrackedCollection<T> : ITrackedObject, INotifyListChanged<T>
    {
        T? SelectedItem { get; set; }

        bool HasSelectedItem { get; }
    }
}
