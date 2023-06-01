using System.Collections.Generic;
using System.Collections.Specialized;

namespace rmsoft.ChangeTracking
{
    public interface IListChangeTracker<T> : IChangeTracker<IList<T>, NotifyCollectionChangedEventArgs>
    {
    }
}
