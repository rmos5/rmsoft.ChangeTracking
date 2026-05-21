using System.ComponentModel;
using rmsoft.ChangeTracking;

namespace rmsoft.ChangeTracking.Tests.TestModels
{
    public class TrackableModel : INotifyPropertyChanged
    {
        private string _trackedName;
        private string _untrackedName;

        public event PropertyChangedEventHandler PropertyChanged;

        [PropertyChangeTracker]
        public string TrackedName
        {
            get => _trackedName;
            set
            {
                if (_trackedName == value)
                    return;

                _trackedName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackedName)));
            }
        }

        public string UntrackedName
        {
            get => _untrackedName;
            set
            {
                if (_untrackedName == value)
                    return;

                _untrackedName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UntrackedName)));
            }
        }
    }
}
