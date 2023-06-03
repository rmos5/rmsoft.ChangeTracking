namespace rmsoft.ChangeTracking
{
    public interface ITrackedCollection<T> : ITrackedObject, INotifyListChanged<T>
    {
        T SelectedItem { get; set; }

        bool HasSelectedItem { get; }
    }
}
