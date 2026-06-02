namespace rmsoft.ChangeTracking
{
    /// <summary>
    /// Represents an object that can enable and perform change tracking.
    /// </summary>
    public interface ITrackedObject : IChangeTracking
    {
        /// <summary>
        /// Gets or sets a value indicating whether calls to <see cref="IChangeTracking.StartTracking" /> are allowed.
        /// </summary>
        bool IsTrackingEnabled { get; set; }
    }
}
