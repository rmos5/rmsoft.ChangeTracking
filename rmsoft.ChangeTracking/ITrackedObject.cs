namespace rmsoft.ChangeTracking
{
    public interface ITrackedObject : IChangeTracking
    {
        bool IsTrackingEnabled { get; set; }
    }
}
