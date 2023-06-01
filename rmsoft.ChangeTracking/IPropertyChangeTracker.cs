using System.ComponentModel;

namespace rmsoft.ChangeTracking
{
    public interface IPropertyChangeTracker : IChangeTracker<INotifyPropertyChanged, PropertyChanges>
    {
        bool IsChanged(string  propertyName);
    }
}
