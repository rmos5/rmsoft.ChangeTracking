using System.Collections.Generic;
using System.Collections.Specialized;

namespace rmsoft.ChangeTracking
{
    public interface INotifyListChanged<T> : IList<T>, INotifyCollectionChanged
    { 
    }
}
